using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.Linq;

namespace ALM.View
{
    /// <summary>
    /// Логика взаимодействия для HorizonSelecterWindow.xaml
    /// </summary>
    partial class HorizonSelecterWindow : Window, IResult
    {
        #region Ctor

        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="dbHorizons">Список доступных горизонтов</param>
        public HorizonSelecterWindow(ObservableCollection<string> dbHorizons)
        {
            InitializeComponent();
            IsSuccess = false;

            var model = new HorizonSelecterViewModel(dbHorizons);
            DataContext = model;
            model.CancelCommand = new RelayCommand(obj => Hide());
            model.SelectCommand = new RelayCommand(obj =>
            {
                IsSuccess = true;
                Result = model.Horizons[model.SelectedHorizon];
                Hide();
            });
        }

        #endregion

        #region Public Properties

        public object Result { get; internal set; }
        public bool IsSuccess { get; internal set; }

        #endregion
    }
    /// <summary>
    /// Модель представления для HorizonSelecterWindow.xaml
    /// </summary>
    class HorizonSelecterViewModel : BaseViewModel
    {
        #region Private Fields

        /// <summary>
        /// Выбранный горизонт
        /// </summary>
        int selectedHorizon;
        /// <summary>
        /// Список доступных для чтения горизонтов
        /// </summary>
        readonly ObservableCollection<string> horizons;

        #endregion

        #region Ctor

        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="dbHorizons">Список доступных для чтения горизонтов</param>
        public HorizonSelecterViewModel(ObservableCollection<string> dbHorizons) 
            => horizons = new ObservableCollection<string>(dbHorizons.OrderBy(s => s));
        
        #endregion

        #region Public Properties

        /// <inheritdoc cref="horizons"/>
        public ObservableCollection<string> Horizons => horizons;
        /// <inheritdoc cref="selectedHorizon"/>
        public int SelectedHorizon
        {
            get => selectedHorizon;
            set
            {
                selectedHorizon = value;
                OnPropertyChanged(nameof(SelectedHorizon));
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Команда продолжения
        /// </summary>
        public ICommand SelectCommand { get; set; }
        /// <summary>
        /// Команда прекращения действий
        /// </summary>
        public ICommand CancelCommand { get; set; }

        #endregion
    }
}
