using ALM.Logging;

using System.IO;
using System;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ALM.Entities
{
    /// <summary>
    /// Параметры отрисовки
    /// </summary>
    public class Primitive
    {
        #region Privat Fields

        /// <summary>
        /// Уникальный Guid объекта
        /// </summary>
        readonly Guid guid;

        #endregion

        #region Public Properties

        /// <summary>
        /// Геометрический объект
        /// </summary>
        public string Geometry { get; }
        /// <summary>
        /// Параметры легендаризации
        /// </summary>
        public JObject DrawSettings { get; }
        /// <summary>
        /// Общие параметры отрисовки
        /// </summary>
        public JObject Param { get; }
        /// <summary>
        /// Имя слоя
        /// </summary>
        public string LayerName { get; }
        /// <summary>
        /// Уникальный номер примитива
        /// </summary>
        public int SystemId { get; }
        /// <summary>
        /// Имя слинкованной таблицы
        /// </summary>
        public string BaseName { get; }
        /// <summary>
        /// Столбец линковки
        /// </summary>
        public string ChildField { get; }
        /// <summary>
        /// Уникальный порядковый номер
        /// </summary>
        public int Id { get; }
        /// <inheritdoc cref="guid"/>
        public string Guid => guid.ToString().ToUpper();

        #endregion

        #region Ctors

        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="wkt">Геометрия примитива в формате WKT</param>
        /// <param name="settings">Параметры отрисовки</param>
        /// <param name="param">Дополнительные параметры</param>
        /// <param name="layername">Имя слоя</param>
        /// <param name="systemid">Уникальный номер</param>
        /// <param name="baseName">Имя слинкованной таблицы</param>
        /// <param name="childFields">Столбец линковки</param>
        /// <param name="id">Номер объекта в таблице запросов</param>
        /// <param name="guid">Уникальный Guid объекта</param>
        public Primitive(string wkt,
                         string settings,
                         string param,
                         string layername,
                         string systemid,
                         string baseName,
                         string childFields,
                         string id,
                         string guid)
        {
            int Int32Parse(string source, string exceptionSource)
            {
                try
                {
                    return Convert.ToInt32(source);
                }
                catch (FormatException e)
                {
                    throw e.AddData(guid, exceptionSource.ToUpper() + ": " + source);
                }
                catch (OverflowException e)
                {
                    throw e.AddData(guid, exceptionSource.ToUpper() + ": " + source);
                }
            }

            Geometry = wkt;
            DrawSettings = JsonParse(settings, nameof(settings), guid);
            Param = JsonParse(param, nameof(param), guid);
            
            try
            {
                LayerName = System.Text.RegularExpressions.Regex.Replace(layername, "[<>\\*\\?/|\\\\\":;,=]", "_");
            }
            catch (System.Text.RegularExpressions.RegexMatchTimeoutException e)
            {
                throw e.AddData(guid, "LAYER: "+ layername);
            }
            catch (ArgumentNullException e)
            {
                throw e.AddData(guid, "LAYER: " + layername);
            }
            catch (ArgumentException e)
            {
                throw e.AddData(guid, "LAYER: " + layername);
            }

            SystemId = Int32Parse(systemid, "SYSTEM_ID");
            BaseName = baseName;
            ChildField = childFields;
            Id = Int32Parse(id, nameof(id));

            try
            {
                this.guid = System.Guid.Parse(guid);
            }
            catch (FormatException e)
            {
                throw e.AddData(guid, "GUID: " + guid);
            }
            catch (ArgumentNullException e)
            {
                throw e.AddData(guid, "GUID: " + guid);
            }
        }
        internal Primitive(string wkt,
                         string settings,
                         string param,
                         string layername,
                         int systemid,
                         string baseName,
                         string childFields,
                         int id,
                         Guid guid)
        {
            var strGuid = guid.ToString().ToUpper();

            Geometry = wkt;
            DrawSettings = JsonParse(settings, nameof(settings), strGuid);
            Param = JsonParse(param, nameof(param), strGuid);

            try
            {
                LayerName = System.Text.RegularExpressions.Regex.Replace(layername, "[<>\\*\\?/|\\\\\":;,=]", "_");
            }
            catch (System.Text.RegularExpressions.RegexMatchTimeoutException e)
            {
                throw e.AddData(strGuid, "LAYER: " + layername);
            }
            catch (ArgumentNullException e)
            {
                throw e.AddData(strGuid, "LAYER: " + layername);
            }
            catch (ArgumentException e)
            {
                throw e.AddData(strGuid, "LAYER: " + layername);
            }

            SystemId = systemid;
            BaseName = baseName;
            ChildField = childFields;
            Id = id;
            this.guid = guid;
        }

        #endregion

        private JObject JsonParse(string source, string exceptionSource, string guid)
        {
            try
            {
                return JObject.Parse(source);
            }
            catch (JsonReaderException e)
            {
                // FontName имеет не имя шрифта а абсолютный путь к нему
                if (e.Message.StartsWith("Bad JSON escape sequence: \\M. Path 'FontName'"))
                {
                    var obj = JObject.Parse(source.Replace("\\", "\\\\"));
                    obj["FontName"] = Path.GetFileName(obj["FontName"].ToString());
                    return obj;
                }
                else
                {
                    throw e.AddData(guid, exceptionSource.ToUpper() + ": " + source);
                }
            }
        }
    }
    public static class ExceptionTools
    {
        private static readonly string sourceKey = "STR";
        private static readonly string guidKey = "GUID";
        public static Exception AddData(this Exception e, string guid, string source) 
        {
            e.Data.Add(sourceKey, source);
            e.Data.Add(guidKey, guid);

            return e;
        }
        public static void Log(this Exception e, ILogger logger, string message)
        {
            logger.LogWarning("Чтение объекта {1}\n" + message + "\n{0}!", e.Data[sourceKey], e.Data[guidKey]);
        }
    }
}