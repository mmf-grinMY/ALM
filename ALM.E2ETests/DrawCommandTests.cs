using System.Windows.Automation;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using Xunit;

namespace ALM.E2ETests
{
    public class DrawCommandTests : IClassFixture<CadFixture>
    {
        private readonly Process _process;

        public DrawCommandTests(CadFixture fixture)
            =>  _process = fixture.Process;
        
        [Fact]
        public void RunDrawCommand_ExecuteWithoutProblems()
        {
            AutomationElement mainWindow = AutomationElement.FromHandle(_process.MainWindowHandle);
            
            AutomationElement commandLine = mainWindow.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "local:AutoCompleteEdit_1"));

            if (commandLine != null)
            {   
                commandLine.SetFocus();

                SendKeys.SendWait("MMP_DRAW");
                SendKeys.SendWait("{ENTER}");
            }

            Thread.Sleep(1000);

            var loginWindow = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

            // TODO: Добавить поиск кнопок действий
        }
    }
}
