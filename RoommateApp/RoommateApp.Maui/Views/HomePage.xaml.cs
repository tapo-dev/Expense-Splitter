using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using RoommateApp.Core.Data;
using RoommateApp.Core.Models;
using RoommateApp.Core.Services;

namespace RoommateApp.Maui.Views {
    public partial class HomePage : ContentPage {
        private readonly AppDbContext _db;
        private readonly SpravceUctu _spravceUctu;
        private readonly DataSeeder _dataSeeder;
        
        public ObservableCollection<Skupina> Skupiny { get; set; } = new();
        
        private Uzivatel _aktualniUzivatel;

        public HomePage(AppDbContext db, SpravceUctu spravceUctu, DataSeeder dataSeeder) {
            InitializeComponent();
            _db = db;
            _spravceUctu = spravceUctu;
            _dataSeeder = dataSeeder;
            
            BindingContext = this;
        }
        
        protected override async void OnAppearing() {
            base.OnAppearing();
            await NacistData();
        }
        
        private async Task NacistData() {
            try {
                CelkovyDluhLabel.Text = "Načítání...";
                CelkovaPohledavkaLabel.Text = "";
                
                var uzivatel = await _db.Uzivatele
                    .AsNoTracking()
                    .Include(u => u.Clenstvi)
                    .ThenInclude(c => c.Skupina)
                    .Include(u => u.Dluhy)
                    .ThenInclude(d => d.Veritel)
                    .Include(u => u.Dluhy)
                    .ThenInclude(d => d.Dluznik)
                    .FirstOrDefaultAsync(u => u.Id == App.CurrentUserId);
                    
                if (uzivatel == null) {
                    await HandleUzivatelNotFound();
                    return;
                }
                
                _aktualniUzivatel = uzivatel;
                
                var skupinyIds = _aktualniUzivatel.Clenstvi.Select(c => c.SkupinaId).ToList();
                var skupiny = await _db.Skupiny
                    .AsNoTracking()
                    .Include(s => s.Clenstvi)
                    .ThenInclude(c => c.Uzivatel)
                    .Include(s => s.Vydaje)
                    .ThenInclude(v => v.Platil)
                    .Include(s => s.Vydaje)
                    .ThenInclude(v => v.Dluhy)
                    .Where(s => skupinyIds.Contains(s.Id))
                    .ToListAsync();
                    
                Skupiny.Clear();
                foreach (var skupina in skupiny) {
                    Skupiny.Add(skupina);
                }
                SkupinyRychlePristupCollectionView.ItemsSource = Skupiny;
                
                AktualizovatShrnutiSkupin(skupiny);
            } catch (Exception ex) {
                CelkovyDluhLabel.Text = "Došlo k chybě při načítání";
                CelkovaPohledavkaLabel.Text = "";
                await DisplayAlert("Chyba", $"Nepodařilo se načíst data: {ex.Message}", "OK");
            }
        }

        private async Task HandleUzivatelNotFound() {
            Preferences.Remove("LoggedInUserId");
            App.CurrentUserId = 0;
            App.CurrentUser = null;
            
            await DisplayAlert("Upozornění", "Váš účet již neexistuje. Budete přesměrován na přihlášení.", "OK");
            await Shell.Current.GoToAsync("//login");
        }
        
        private void AktualizovatShrnutiSkupin(List<Skupina> skupiny) {
            SkupinyContainer.Children.Clear();
            
            if (skupiny == null || skupiny.Count == 0) {
                ZobrazitPrazdneSkupiny();
                return;
            }
            
            decimal celkovyDluh = 0;
            decimal celkovaPohledavka = 0;
            
            foreach (var skupina in skupiny) {
                var dluhySkupiny = skupina.ZiskejDluhy() ?? new List<Dluh>();
                decimal bilance = VypocitatBilanci(dluhySkupiny, ref celkovyDluh, ref celkovaPohledavka);
                
                var skupinaGrid = VytvoritGridProSkupinu(skupina, bilance);
                SkupinyContainer.Children.Add(skupinaGrid);
            }
            
            CelkovyDluhLabel.Text = $"Pohromadě dlužíš {celkovyDluh} Kč ve všech skupinách";
            CelkovaPohledavkaLabel.Text = $"Také dluží ostatní: {celkovaPohledavka} Kč";
        }

        private void ZobrazitPrazdneSkupiny() {
            var zadneSkupinyLabel = new Label {
                Text = "Nemáte žádné skupiny",
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Gray,
                FontSize = 14,
                Margin = new Thickness(0, 10)
            };
            SkupinyContainer.Children.Add(zadneSkupinyLabel);
            
            CelkovyDluhLabel.Text = "Pohromadě dlužíš 0 Kč ve všech skupinách";
            CelkovaPohledavkaLabel.Text = "Také dluží ostatní: 0 Kč";
        }

        private decimal VypocitatBilanci(List<Dluh> dluhySkupiny, ref decimal celkovyDluh, ref decimal celkovaPohledavka) {
            decimal bilance = 0;
            
            foreach (var dluh in dluhySkupiny) {
                if (dluh.JeSplaceno) continue;
                
                if (dluh.DluznikId == 0 || dluh.VeritelId == 0) continue;
                
                if (dluh.DluznikId == _aktualniUzivatel.Id) {
                    bilance -= dluh.Castka;
                    celkovyDluh += dluh.Castka;
                } else if (dluh.VeritelId == _aktualniUzivatel.Id) {
                    bilance += dluh.Castka;
                    celkovaPohledavka += dluh.Castka;
                }
            }
            
            return bilance;
        }

        private Grid VytvoritGridProSkupinu(Skupina skupina, decimal bilance) {
            var skupinaGrid = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Margin = new Thickness(0, 2)
            };
            
            var nazevLabel = new Label {
                Text = $"• {skupina.Nazev}",
                VerticalOptions = LayoutOptions.Center,
                FontSize = 14
            };
            skupinaGrid.Children.Add(nazevLabel);
            
            var castkaLabel = new Label {
                Text = bilance == 0 ? "0 Kč" : $"{bilance:+#;-#;0} Kč",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                FontSize = 14,
                TextColor = bilance > 0 ? Colors.Green : bilance < 0 ? Colors.Red : Colors.Gray
            };
            
            skupinaGrid.Children.Add(castkaLabel);
            Grid.SetColumn(castkaLabel, 1);
            
            return skupinaGrid;
        }
        
        private async void OnSkupinaSelected(object sender, SelectionChangedEventArgs e) {
            if (e.CurrentSelection.FirstOrDefault() is Skupina vybranaSkupina) {
                ((CollectionView)sender).SelectedItem = null;
        
                try {
                    await Shell.Current.GoToAsync($"//main/groups?skupinaId={vybranaSkupina.Id}");
                } catch (Exception ex) {
                    await DisplayAlert("Chyba navigace", $"Nepodařilo se přejít na detail skupiny: {ex.Message}", "OK");
                }
            }
        }
        
        private async void OnAddExpenseClicked(object sender, EventArgs e) {
            try {
                var addExpensePage = Handler.MauiContext.Services.GetService<AddExpensePage>();
                if (addExpensePage != null) {
                    await Navigation.PushAsync(addExpensePage);
                } else {
                    await DisplayAlert("Chyba", "Nepodařilo se vytvořit stránku pro přidání výdaje.", "OK");
                }
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Došlo k chybě při navigaci: {ex.Message}", "OK");
            }
        }
        
        private async void OnResetDatabaseClicked(object sender, EventArgs e) {
            try {
                bool reset = await DisplayAlert("Reset databáze", "Opravdu chcete resetovat databázi a naplnit ji testovacími daty?", "Ano", "Ne");
                
                if (reset) {
                    await _dataSeeder.ResetDatabaseAsync();
                    await DisplayAlert("Úspěch", "Databáze byla resetována s novými testovacími daty.", "OK");
                    await Shell.Current.GoToAsync("//login");
                }
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Nepodařilo se resetovat databázi: {ex.Message}", "OK");
            }
        }
    }
}