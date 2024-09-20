using System.Diagnostics;
using System.Threading;
using System;

namespace ALM.E2ETests
{
    public class CadFixture : IDisposable
    {
        public Process Process { get; }

        public CadFixture()
        {
            const string cadAppPath = @"C:\Program Files\Autodesk\AutoCAD 2021\acad.exe";
            const string cadAppArgs = "/b load_plugin.scr";

            Process = Process.Start(cadAppPath, cadAppArgs);
            Process.WaitForInputIdle();

            Thread.Sleep(10_000);
        }

        public void Dispose()
            => Process.Kill();
    }
}
