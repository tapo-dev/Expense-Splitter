using RoommateApp.Core.Data;
using RoommateApp.Core.Models;
using RoommateApp.Core.Services;

namespace RoommateApp.Maui.Views {
    public partial class EditProfilePage : ContentPage {
        private readonly AppDbContext _db;
        private readonly AuthService _authService;
        private Uzivatel _aktualniUzivatel;

        public EditProfilePage(AppDbContext db, AuthService authService) {
            InitializeComponent();
            _db = db;
            _authService = authService;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            
            _aktualniUzivatel = App.CurrentUser;
            
            if (_aktualniUzivatel != null) {
                JmenoEntry.Text = _aktualniUzivatel.Jmeno;
                EmailEntry.Text = _aktualniUzivatel.Email;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e) {
            try {
                ErrorMessageLabel.IsVisible = false;
                
                if (string.IsNullOrWhiteSpace(JmenoEntry.Text)) {
                    ShowError("Jméno nemůže být prázdné.");
                    return;
                }
                
                bool zmenitHeslo = !string.IsNullOrWhiteSpace(PasswordEntry.Text);
                if (zmenitHeslo) {
                    if (PasswordEntry.Text.Length < 3) {
                        ShowError("Heslo musí mít alespoň 3 znaky.");
                        return;
                    }
                    
                    if (PasswordEntry.Text != ConfirmPasswordEntry.Text) {
                        ShowError("Hesla se neshodují.");
                        return;
                    }
                }
                
                _aktualniUzivatel.Jmeno = JmenoEntry.Text.Trim();
                
                if (zmenitHeslo) {
                    _aktualniUzivatel.Heslo = PasswordEntry.Text;
                }
                
                await _db.SaveChangesAsync();
                App.CurrentUser = _aktualniUzivatel;
                
                await DisplayAlert("Úspěch", "Profil byl úspěšně aktualizován.", "OK");
                await Navigation.PopAsync();
            } catch (Exception ex) {
                ShowError($"Chyba při ukládání změn: {ex.Message}");
            }
        }

        private void ShowError(string message) {
            ErrorMessageLabel.Text = message;
            ErrorMessageLabel.IsVisible = true;
        }
        
        private async void OnCancelClicked(object sender, EventArgs e) {
            await Navigation.PopAsync();
        }
    }
}