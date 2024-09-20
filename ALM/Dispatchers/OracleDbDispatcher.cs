using ALM.Entities;
using ALM.Logging;
using ALM.View;

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;
using System.Threading;
using System.Windows;
using System.Data;
using System.Text;
using System.IO;
using System;

using Oracle.ManagedDataAccess.Client;

using Newtonsoft.Json;

namespace ALM
{
    /// <summary>
    ///  Диспетчер для работы с БД Oracle
    /// </summary>
    class OracleDbDispatcher : IDbDispatcher
    {
        #region Private Fields

        /// <summary>
        /// Подключение к Oracle БД
        /// </summary>
        internal readonly OracleConnection connection;
        /// <summary>
        /// Текущий горизонт
        /// </summary>
        readonly string horizon;
        private readonly ILogger logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="connectionStr">Строка подключения</param>
        /// <param name="gorizont">Выбранный горизонт</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public OracleDbDispatcher(ILogger logger, string connectionStr = null, string gorizont = null)
        {
            this.logger = logger;

            var isCreated = false;
            var isCanceled = false;
            object[] result;

            while (!isCreated && !isCanceled)
            {
                try
                {
                    connection?.Dispose();
                    (connection = new OracleConnection(connectionStr)).Open();
                    isCreated = true;
                }
                catch (OracleException e)
                {
                    MessageBox.Show(e.GetCodeDescription(), "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                    result = DbHelper.ConnectionStr;
                    isCanceled = (bool)result[1];
                    if (result[0] is object[] array)
                    {
                        connectionStr = array[0].ToString();
                        IsBoundingBoxChecked = (bool)array[1];
                    }
                }
                catch (InvalidOperationException)
                {
                    result = DbHelper.ConnectionStr;
                    isCanceled = (bool)result[1];
                    if (result[0] is object[] array)
                    {
                        connectionStr = array[0].ToString();
                        IsBoundingBoxChecked = (bool)array[1];
                    }
                }
            }

            if (gorizont is null)
            {
                result = DbHelper.SelectGorizont(Gorizonts);

                if ((bool)result[1])
                    throw new InvalidOperationException();

                this.horizon = result[0].ToString();
            }
            else
            {
                this.horizon = gorizont;
            }
        }

        #endregion

        #region Public Properties

        public ObservableCollection<string> Gorizonts
        {
            get
            {
                var gorizonts = new ObservableCollection<string>();
                const string command =
"SELECT DISTINCT SUBSTR(table_name, 2, INSTR(table_name, '_', 2) - 2) AS pattern FROM all_tables " + 
"WHERE table_name LIKE 'K%_TRANS_CLONE' AND SUBSTR(table_name, 2, INSTR(table_name, '_', 2) - 2) IN (" + 
"SELECT SUBSTR(table_name, 2, INSTR(table_name, '_', 2) - 2) FROM all_tables WHERE table_name LIKE 'K%_TRANS_OPEN_SUBLAYERS')";

                using (var reader = new OracleCommand(command, connection).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gorizonts.Add(reader.GetString(0));
                    }
                }

                return gorizonts;
            }
        }
        public uint Count
        {
            get
            {
                string command = "SELECT COUNT(*) FROM k" + horizon + "_trans_clone";

                using (var reader = new OracleCommand(command, connection).ExecuteReader())
                {
                    reader.Read();
                    return Convert.ToUInt32(reader.GetInt32(0));
                }
            }
        }
        public bool IsBoundingBoxChecked { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Освобождение занятых ресурсов
        /// </summary>
        public void Dispose() => connection?.Dispose();
        public string GetExternalDbLink(string baseName)
        {
            string command = "SELECT data FROM LINKS WHERE NAME = '" + baseName + "'";

            using (var reader = new OracleCommand(command, connection).ExecuteReader())
            {
                if (reader.Read()) return reader.GetString(0);
            }

            return string.Empty;
        }
        public DataTable GetDataTable(string command)
        {
            using (var reader = new OracleCommand(command, connection).ExecuteReader())
            {
                var dataTable = new DataTable();
                dataTable.Load(reader);

                return dataTable;
            }
        }
        private bool TryGetLongGeometry(string geometry, string guid, string numbColumnName, out string longGeometry)
        {
            var builder = new StringBuilder(10_000).Append(geometry);
            var query = $"SELECT \"PAGE\" FROM \"K{horizon}_TRANS_CLONE_GEOWKT\" WHERE OBJECTGUID = '{guid}' ORDER BY \"";
            try
            {
                using (var command = new OracleCommand(query + numbColumnName + "\"", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            builder.Append(reader.GetString(0));
                        }
                    }
                }

                longGeometry = builder.ToString();
                return true;
            }
            catch (OracleException)
            {
                longGeometry = string.Empty;
                return false;
            }
        }
        public string GetLongGeometry(Primitive primitive)
        {
            if (TryGetLongGeometry(primitive.Geometry, primitive.Guid, "NUMB", out string longGeometry))
            {
                return longGeometry;
            }
            else if (TryGetLongGeometry(primitive.Geometry, primitive.Guid, "NUMBER", out longGeometry))
            {
                return longGeometry;
            }
            else
            {
                throw new InvalidOperationException($"Не удалось прочитать длинную геометрию объекта \"{primitive.Guid}\"");
            }
        }
        public async void ReadAsync(CancellationToken token, ConcurrentQueue<Primitive> queue, DrawInfoViewModel model, Session session)
        {
            await Task.Run(async () =>
            {
                var readPosition = model.readPosition;
                var totalCount = Count;
                var percent = readPosition * 100 / totalCount;
                var command = string.Empty;

                using (var stream = new StreamReader(Path.Combine(Constants.AssemblyPath, "draw.sql")))
                {
                    command = string.Format(stream.ReadToEnd(), horizon, readPosition,
                        session.Right, session.Left, session.Top, session.Bottom);
                }

                DbDataReader reader = null;

                try
                { 
                    reader = await new OracleCommand(command, connection).ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        if (token.IsCancellationRequested) return;

                        while (queue.Count > Constants.QueueLimit) await Task.Delay(Constants.ReaderSleepTime);

                        try
                        {
                            var primitive = new Primitive(reader["geowkt"].ToString(),
                                                          reader["drawjson"].ToString(),
                                                          reader["paramjson"].ToString(),
                                                          reader["layername"] + " _ " + reader["sublayername"],
                                                          reader["systemid"].ToString(),
                                                          reader["basename"].ToString(),
                                                          reader["childfields"].ToString(),
                                                          reader["rn"].ToString(),
                                                          reader["objectguid"].ToString());

                            queue.Enqueue(primitive);
                        }
                        catch (JsonReaderException e)
                        {
                            e.Log(logger, "Невозможно преобразовать в json объект строку");
                        }
                        catch (RegexMatchTimeoutException e)
                        {
                            e.Log(logger, "Превышен предел времени преобразования строки");
                        }
                        catch (FormatException e)
                        {
                            e.Log(logger, "Неверный формат представления величины");
                        }
                        catch (OverflowException e)
                        {
                            e.Log(logger, "Первышен размер допустимого диапазона чисел int32");
                        }
                        catch (ArgumentNullException e)
                        {
                            e.Log(logger, "Невозможно обработать нулевой аргумент");
                        }
                        catch (ArgumentException e)
                        {
                            e.Log(logger, "Невозможно обработать параметр");
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e);
                        }
                        finally
                        {
                            var currentPrecent = ++readPosition * 100 / totalCount;

                            if (currentPrecent > percent)
                            {
                                percent = currentPrecent;
                                model.ReadProgress = percent;
                            }
                        }
                    }
                }
                // Вызывается из-за отсутсвия некоторых запрашиваемых столбцов в таблице
                catch (OracleException e)
                {
                    logger.LogError(e);
                }
                finally
                {
                    model.readPosition = readPosition;
                    model.isReadEnded = true;
                    reader?.Dispose();
                }
            }, token);
        }
        public IEnumerable<string> GetLayers()
        {
            string command = "SELECT layername, sublayername FROM ( " + 
                $"SELECT DISTINCT b.layername, b.sublayername FROM k{horizon}_trans_clone a" + 
                $" INNER JOIN k{horizon}_trans_open_sublayers b ON a.sublayerguid = b.sublayerguid)";

            using (var reader = new OracleCommand(command, connection).ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return Regex.Replace(reader.GetString(0) + " _ " + reader.GetString(1), "[<>\\*\\?/|\\\\\":;,=]", "_");
                }
            }
        }

#endregion
    }
}