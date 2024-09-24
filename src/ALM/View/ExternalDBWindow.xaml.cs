using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;

namespace ALM.View
{
    /// <summary>
    /// Логика взаимодействия для ExternalDbWindow.xaml
    /// </summary>
    partial class ExternalDbWindow : Window
    {
        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="view">Данные для отображения</param>
        public ExternalDbWindow(DataView view)
        {
            InitializeComponent();
            DataContext = new ExternalDbViewModel(view);
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "АБС.ОТМ.,М_")
            {
                // Создание столбца
                DataGridTextColumn textColumn = new DataGridTextColumn
                {
                    Header = "АБС.ОТМ.,М_"
                };
                // textColumn.Binding = new Binding("[" + e.PropertyName + "]");

                Binding binding = new Binding
                {
                    ElementName = "dataGrid",
                    Path = new PropertyPath($"Items[{e.Column.DisplayIndex}].[{e.PropertyName}]")
                };
                textColumn.Binding = binding;

                // Добавление столбца в DataGrid
                (sender as DataGrid).Columns.Add(textColumn);

                // Отмена автоматической генерации столбца
                e.Cancel = true;
            }
        }
    }
    /// <summary>
    /// Внутренняя логика взаимодействия для ExternalDbWindow.xaml
    /// </summary>
    class ExternalDbViewModel : BaseViewModel
    {
        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="view">Данные для отображения</param>
        public ExternalDbViewModel(DataView view) => View = view;
        /// <summary>
        /// Данные для отображения
        /// </summary>
        public DataView View { get; }
    }
}
