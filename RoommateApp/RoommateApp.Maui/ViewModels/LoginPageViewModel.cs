using RoommateApp.Core.Services;
using RoommateApp.Core.ViewModels;
using RoommateApp.Core.ViewModels.Commands;

namespace RoommateApp.Maui.ViewModels {
    /// <summary>
    /// ViewModel pro přihlašovací stránku
    /// </summary>
    public class LoginPageViewModel : BaseViewModel {
        private readonly AuthService _authService;

        public LoginPageViewModel(AuthService authService) {
            _authService = authService;

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            RegisterCommand = new RelayCommand(GoToRegister);
            ForgotPasswordCommand = new RelayCommand(ShowForgotPasswordAlert);
        }

        #region Properties

        private string _email = string.Empty;
        public string Email {
            get => _email;
            set {
                SetProperty(ref _email, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password = string.Empty;
        public string Password {
            get => _password;
            set {
                SetProperty(ref _password, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _rememberMe;
        public bool RememberMe {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        #endregion

        #region Commands

        public AsyncRelayCommand LoginCommand { get; }
        public RelayCommand RegisterCommand { get; }
        public RelayCommand ForgotPasswordCommand { get; }

        #endregion

        #region Private Methods

        private bool CanLogin() {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;
        }

        private async Task LoginAsync() {
            ClearError();
            IsBusy = true;

            try {
                var uzivatel = await _authService.PrihlasitAsync(Email, Password);

                if (uzivatel == null) {
                    SetError("Nesprávný email nebo heslo");
                    return;
                }

                // Uložení údajů
                if (RememberMe) {
                    Preferences.Set("SavedEmail", Email);
                    Preferences.Set("SavedPassword", Password);
                } else {
                    Preferences.Remove("SavedEmail");
                    Preferences.Remove("SavedPassword");
                }

                Preferences.Set("LoggedInUserId", uzivatel.Id);
                App.CurrentUserId = uzivatel.Id;
                App.CurrentUser = uzivatel;

                await Application.Current.MainPage.DisplayAlert("Úspěch", $"Vítejte, {uzivatel.Jmeno}!", "OK");
                await Shell.Current.GoToAsync("//main");
            } catch (Exception ex) {
                SetError($"Chyba při přihlašování: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        private async void GoToRegister() {
            await Shell.Current.GoToAsync("//register");
        }

        private async void ShowForgotPasswordAlert() {
            await Application.Current.MainPage.DisplayAlert("Zapomenuté heslo", 
                "Kontaktujte správce aplikace pro resetování hesla.", 
                "OK");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Načte uložené přihlašovací údaje
        /// </summary>
        public void LoadSavedCredentials() {
            if (Preferences.ContainsKey("SavedEmail")) {
                Email = Preferences.Get("SavedEmail", string.Empty);
                Password = Preferences.Get("SavedPassword", string.Empty);
                RememberMe = true;
            }
        }

        #endregion
    }
}