using System.Diagnostics;
using System.Threading;
using System;

namespace ALM.E2ETests
{
    /// <summary>
    /// Менеджер жизни тестируемого процесса
    /// </summary>
    public class CadFixture : IDisposable
    {
        /// <summary>
        /// Тестируемый процесс
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Инициализация менеджера
        /// </summary>
        public CadFixture()
        {
            const int sleepTime = 12_000;

            const string cadAppPath = @"C:\Program Files\Autodesk\AutoCAD 2021\acad.exe";
            const string cadAppArgs = "/b load_plugin.scr";

            Process = Process.Start(cadAppPath, cadAppArgs);
            Process.WaitForInputIdle();

            Thread.Sleep(sleepTime);
        }

        /// <summary>
        /// Закрытие тестируемого процесса
        /// </summary>
        public void Dispose()
            => Process.Kill();
    }
}
