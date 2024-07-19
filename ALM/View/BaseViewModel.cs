using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace ALM
{
    /// <summary>
    /// Базовый класс модели представления
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Изменение свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Изменить свойство
        /// </summary>
        /// <param name="property">Имя свойства</param>
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
