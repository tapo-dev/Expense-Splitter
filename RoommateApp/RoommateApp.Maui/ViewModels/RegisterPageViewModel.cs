using RoommateApp.Core.Services;
using RoommateApp.Core.ViewModels;
using RoommateApp.Core.ViewModels.Commands;
using System.Text.RegularExpressions;

namespace RoommateApp.Maui.ViewModels {
    /// <summary>
    /// ViewModel pro registrační stránku
    /// </summary>
    public class RegisterPageViewModel : BaseViewModel {
        private readonly AuthService _authService;

        public RegisterPageViewModel(AuthService authService) {
            _authService = authService;

            RegisterCommand = new AsyncRelayCommand(RegisterAsync, CanRegister);
            BackToLoginCommand = new RelayCommand(GoBackToLogin);
        }

        #region Properties

        private string _jmeno = string.Empty;
        public string Jmeno {
            get => _jmeno;
            set {
                SetProperty(ref _jmeno, value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _email = string.Empty;
        public string Email {
            get => _email;
            set {
                SetProperty(ref _email, value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password = string.Empty;
        public string Password {
            get => _password;
            set {
                SetProperty(ref _password, value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword {
            get => _confirmPassword;
            set {
                SetProperty(ref _confirmPassword, value);
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Commands

        public AsyncRelayCommand RegisterCommand { get; }
        public RelayCommand BackToLoginCommand { get; }

        #endregion

        #region Private Methods

        private bool CanRegister() {
            return !string.IsNullOrWhiteSpace(Jmeno) && 
                   !string.IsNullOrWhiteSpace(Email) && 
                   !string.IsNullOrWhiteSpace(Password) && 
                   !string.IsNullOrWhiteSpace(ConfirmPassword) && 
                   !IsBusy;
        }

        private async Task RegisterAsync() {
            ClearError();
            IsBusy = true;

            try {
                if (!IsValidEmail(Email)) {
                    SetError("Zadejte platný email");
                    return;
                }

                if (Password.Length < 3) {
                    SetError("Heslo musí mít alespoň 3 znaky");
                    return;
                }

                if (Password != ConfirmPassword) {
                    SetError("Hesla se neshodují");
                    return;
                }

                var (novyUzivatel, chyba) = await _authService.RegistrovatAsync(Jmeno.Trim(), Email.Trim(), Password);

                if (novyUzivatel == null) {
                    SetError(chyba);
                    return;
                }

                Preferences.Set("LoggedInUserId", novyUzivatel.Id);
                App.CurrentUserId = novyUzivatel.Id;
                App.CurrentUser = novyUzivatel;

                await Application.Current.MainPage.DisplayAlert("Úspěch", "Registrace byla úspěšná", "OK");
                
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//main");
            } catch (Exception ex) {
                SetError($"Chyba při registraci: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        private async void GoBackToLogin() {
            await Shell.Current.GoToAsync("//login");
        }

        private bool IsValidEmail(string email) {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try {
                var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
                return regex.IsMatch(email);
            } catch {
                return false;
            }
        }

        #endregion
    }
}