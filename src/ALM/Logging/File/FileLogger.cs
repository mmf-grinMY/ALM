using System.IO;

namespace ALM.Logging
{
    /// <summary>
    /// Файловый логер событий
    /// </summary>
    sealed class FileLogger : ILogger
    {
        #region Private Fields

        /// <summary>
        /// Объект для синхронизации потоков
        /// </summary>
        static readonly object lockObj = new object();
        /// <summary>
        /// Расположение файла логов
        /// </summary>
        readonly string filePath;

        #endregion

        #region Ctor

        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="name">Расположение файла логов</param>
        public FileLogger(string name, bool logOverwrite = true)
        {
            var root = Path.Combine(Constants.AssemblyPath, "Logs");

            if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            filePath = Path.Combine(root, name);

            if (logOverwrite && File.Exists(filePath)) File.Delete(filePath);
        }

        #endregion

        #region Public Methods

        public void Log(LogLevel level, string message, System.Exception ex)
        {
            lock (lockObj)
            {
                bool isEmpty = true;
                var builder = new System.Text.StringBuilder();
                if (message != null && message != string.Empty)
                {
                    builder
                        .Append(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append(" | ").Append(level).AppendLine(":")
                        .Append('\t').AppendLine(message);

                    isEmpty = false;
                }

                if (ex != null)
                {
                    if (isEmpty) 
                        builder.Append(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append(" | ").Append(level).AppendLine(":");

                    builder
                        .Append('\t').AppendLine(ex.GetType().ToString())
                        .Append('\t').AppendLine(ex.Message)
                        .Append('\t').AppendLine(ex.Source)
                        .AppendLine(ex.StackTrace);

                    isEmpty = false;
                }

                if (!isEmpty)
                {
                    System.IO.File.AppendAllText(filePath, builder.ToString());
                }
            }
        }
        public void StartCommandExecute(string command)
        {
            lock (lockObj)
            {
                var builder = new System.Text.StringBuilder();
                var separator = new string('=', 20);
                builder.AppendLine().AppendLine(separator)
                    .AppendLine("Start executing " + command).AppendLine(separator).AppendLine();
            }
        }
        public void StopCommandExecute(string command)
        {
            lock (lockObj)
            {
                var builder = new System.Text.StringBuilder();
                var separator = new string('=', 20);
                builder.AppendLine().AppendLine(separator)
                    .AppendLine("Stop executing " + command).AppendLine(separator).AppendLine();
            }
        }

        #endregion
    }
}