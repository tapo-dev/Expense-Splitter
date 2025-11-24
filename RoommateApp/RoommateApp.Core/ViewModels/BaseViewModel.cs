using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RoommateApp.Core.ViewModels {
    /// <summary>
    /// Základní třída pro všechny ViewModels
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isBusy;
        public bool IsBusy {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _errorMessage;
        public string ErrorMessage {
            get => _errorMessage;
            set {
                if (SetProperty(ref _errorMessage, value)) {
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        /// <summary>
        /// Má chybu?
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Nastaví property a vyvolá PropertyChanged
        /// </summary>
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "") {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Vyvolá PropertyChanged event
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Vyčistí chybu
        /// </summary>
        protected void ClearError() {
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Nastaví chybu
        /// </summary>
        protected void SetError(string message) {
            ErrorMessage = message;
        }
    }
}