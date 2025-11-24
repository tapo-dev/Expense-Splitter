using Microsoft.EntityFrameworkCore;
using RoommateApp.Core.Data;
using RoommateApp.Core.Models;
using RoommateApp.Core.Services;
using System.Collections.ObjectModel;

namespace RoommateApp.Maui.Views {
    public partial class ProfilePage : ContentPage {
        private readonly AppDbContext _db;
        private readonly AuthService _authService;
        private Uzivatel _aktualniUzivatel;
        
        public ObservableCollection<SkupinaDluhy> Skupiny { get; set; } = new();
        
        public ProfilePage(AppDbContext db, AuthService authService) {
            InitializeComponent();
            _db = db;
            _authService = authService;
        }
        
        protected override async void OnAppearing() {
            base.OnAppearing();
            await NacistData();
        }
        
        private async Task NacistData() {
            try {
                await NacistAktualnihoUzivatele();
                if (_aktualniUzivatel == null) return;
                
                NastavitZakladniInformace();
                await NacistAVytvoritDluhyProSkupiny();
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst data: {ex.Message}", "OK");
            }
        }

        private async Task NacistAktualnihoUzivatele() {
            if (App.CurrentUser == null && App.CurrentUserId > 0) {
                _aktualniUzivatel = await _authService.NacistUzivateleAsync(App.CurrentUserId);
                if (_aktualniUzivatel != null) {
                    App.CurrentUser = _aktualniUzivatel;
                }
            } else {
                _aktualniUzivatel = App.CurrentUser;
            }
            
            if (_aktualniUzivatel == null) {
                await DisplayAlert("Chyba", "Uživatel nenalezen", "OK");
                await Shell.Current.GoToAsync("//login");
            }
        }

        private void NastavitZakladniInformace() {
            JmenoLabel.Text = _aktualniUzivatel.Jmeno;
            EmailLabel.Text = _aktualniUzivatel.Email;
            
            var prvniClenstvi = _aktualniUzivatel.Clenstvi?.OrderBy(c => c.DatumPridani).FirstOrDefault();
            if (prvniClenstvi != null) {
                DatumRegistraceLabel.Text = prvniClenstvi.DatumPridani.ToString("dd.MM.yyyy");
            } else {
                DatumRegistraceLabel.Text = "Neuvedeno";
            }
        }

        private async Task NacistAVytvoritDluhyProSkupiny() {
            var skupinyIds = _aktualniUzivatel.Clenstvi?.Select(c => c.SkupinaId).ToList() ?? new List<int>();
            var skupiny = await _db.Skupiny
                .Include(s => s.Vydaje)
                .ThenInclude(v => v.Dluhy)
                .Where(s => skupinyIds.Contains(s.Id))
                .ToListAsync();
                
            SkupinyDluhyContainer.Children.Clear();
            
            foreach (var skupina in skupiny) {
                var dluhy = skupina.ZiskejDluhy();
                
                decimal dluzim = dluhy
                    .Where(d => d.DluznikId == _aktualniUzivatel.Id && !d.JeSplaceno)
                    .Sum(d => d.Castka);
                
                decimal dluziMi = dluhy
                    .Where(d => d.VeritelId == _aktualniUzivatel.Id && !d.JeSplaceno)
                    .Sum(d => d.Castka);
                
                var frame = VytvoritFrameProSkupinu(skupina.Nazev, dluzim, dluziMi);
                SkupinyDluhyContainer.Children.Add(frame);
            }
        }

        private Frame VytvoritFrameProSkupinu(string nazevSkupiny, decimal dluzim, decimal dluziMi) {
            var frame = new Frame {
                BackgroundColor = Color.FromArgb("#f0f0f0"),
                Padding = new Thickness(15),
                CornerRadius = 10,
                Margin = new Thickness(0, 0, 0, 15)
            };
            
            var stackLayout = new StackLayout();
            
            var nazevLabel = new Label {
                Text = $"Ve skupině: {nazevSkupiny}",
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            stackLayout.Children.Add(nazevLabel);
            
            var grid = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                RowDefinitions = {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                RowSpacing = 5
            };
            
            var dluzisLabel = new Label { Text = "• Dlužíš:" };
            grid.Children.Add(dluzisLabel);
            
            var dluzimCastkaLabel = new Label {
                Text = $"{dluzim} Kč",
                HorizontalOptions = LayoutOptions.End
            };
            grid.Children.Add(dluzimCastkaLabel);
            Grid.SetColumn(dluzimCastkaLabel, 1);
            
            var dluziTiLabel = new Label { Text = "• Tobě dluží:" };
            grid.Children.Add(dluziTiLabel);
            Grid.SetRow(dluziTiLabel, 1);
            
            var dluziMiCastkaLabel = new Label {
                Text = $"{dluziMi} Kč",
                HorizontalOptions = LayoutOptions.End
            };
            grid.Children.Add(dluziMiCastkaLabel);
            Grid.SetRow(dluziMiCastkaLabel, 1);
            Grid.SetColumn(dluziMiCastkaLabel, 1);
            
            stackLayout.Children.Add(grid);
            frame.Content = stackLayout;
            
            return frame;
        }
        
        private async void OnOdhlasitClicked(object sender, EventArgs e) {
            bool potvrdit = await DisplayAlert("Odhlášení", "Opravdu se chcete odhlásit?", "Ano", "Ne");
    
            if (potvrdit) {
                Preferences.Remove("LoggedInUserId");
                Preferences.Remove("SavedEmail");
                Preferences.Remove("SavedPassword");
        
                App.CurrentUserId = 0;
                App.CurrentUser = null;
        
                await Shell.Current.GoToAsync("//login");
            }
        }
        
        private async void OnNastaveniClicked(object sender, EventArgs e) {
            var editProfilePage = Handler.MauiContext.Services.GetService<EditProfilePage>();
            if (editProfilePage != null) {
                await Navigation.PushAsync(editProfilePage);
            } else {
                await DisplayAlert("Chyba", "Nepodařilo se otevřít stránku pro úpravu profilu.", "OK");
            }
        }
    }

    /// <summary>
    /// Pomocná třída pro přehled dluhů ve skupině
    /// </summary>
    public class SkupinaDluhy {
        public string NazevSkupiny { get; set; }
        public decimal DluzimCastka { get; set; }
        public decimal DluziMiCastka { get; set; }
    }
}