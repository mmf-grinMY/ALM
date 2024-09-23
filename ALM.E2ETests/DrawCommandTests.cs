using System.Windows.Automation;
using System.Diagnostics;
using System.Threading;

using Xunit;

namespace ALM.E2ETests
{
    /// <summary>
    /// Класс содержит тест проверки работы команды MMP_DRAW
    /// </summary>
    /// <remarks>
    /// При первом запуске тестов появится окно загрузки сборки с плагином, нужно выбрать пункт "Всегда загружать".
    /// 
    /// <list type="number">
    /// <item>
    /// <description>Особенности тестирования</description>
    /// </item>
    /// <item>
    /// <description>AutoElement окно содержит только дочерние элементы.</description>
    /// </item>
    /// <item>
    /// <description>Окна плагина принадлежат собственному процессу.</description>
    /// </item>
    /// </list>
    /// 
    /// </remarks>
    public class DrawCommandTests : IClassFixture<CadFixture>
    {
        /// <summary>
        /// Тестируемый процесс
        /// </summary>
        private readonly Process _process;

        /// <summary>
        /// Инициализация тестов
        /// </summary>
        /// <param name="fixture">Менеджер жизни тестируемого процесса</param>
        public DrawCommandTests(CadFixture fixture)
            => _process = fixture.Process;

        /// <summary>
        /// Тестирование работы команды <b>MMP_DRAW</b>
        /// </summary>
        [Fact]
        public void RunDrawCommand_ExecuteWithoutProblems()
        {
            #region Инициализация главного окна AutoCAD
            
            const int sleepTime = 1_000;

            var mainWindow = AutomationElement.FromHandle(_process.MainWindowHandle);

            var commandLineCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, "local:AutoCompleteEdit_1");
            mainWindow.FindFirst(TreeScope.Descendants, commandLineCondition).SendKeys("MMP_DRAW{ENTER}");

            Thread.Sleep(sleepTime);

            #endregion

            #region Инициализация окна подключения

            var wpfCondition = new PropertyCondition(AutomationElement.FrameworkIdProperty, "WPF");
            var loginWindowCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, "loginWindow");

            var loginWindow = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, new AndCondition(wpfCondition, loginWindowCondition));

            loginWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "usernamebox")).SendKeys("GEOUSER");
            loginWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "passwordBox")).SendKeys("GEOUSER");
            loginWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "host")).SendKeys("localhost");
            loginWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "dbName")).SendKeys("XEPDB1");
            loginWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "saveButton")).SendKeys("{ENTER}");

            #endregion
        }
    }
    /// <summary>
    /// Класс расширения функциональности <see cref="AutomationElement"/>
    /// </summary>
    public static class AutomationElementExtensions
    {
        /// <summary>
        /// Ввод информации в элемент управления
        /// </summary>
        /// <param name="element">Исходный элемент управления</param>
        /// <param name="keys">Список клавиш для нажатия</param>
        public static void SendKeys(this AutomationElement element, string keys)
        {
            element.SetFocus();

            System.Windows.Forms.SendKeys.SendWait(keys);
        }
    }
}
