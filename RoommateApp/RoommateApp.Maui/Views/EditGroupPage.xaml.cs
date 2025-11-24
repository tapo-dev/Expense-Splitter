using Microsoft.EntityFrameworkCore;
using RoommateApp.Core.Data;
using RoommateApp.Core.Models;

namespace RoommateApp.Maui.Views {
    [QueryProperty(nameof(SkupinaId), "skupinaId")]
    public partial class EditGroupPage : ContentPage {
        private readonly AppDbContext _db;
        private Skupina _skupina;
        private List<Uzivatel> _dostupniUzivatele = new();
        private List<Uzivatel> _filtrovaníUzivatele = new();
        private bool _jsemAdmin = false;
        
        public string SkupinaId { get; set; }
        
        public EditGroupPage(AppDbContext db) {
            InitializeComponent();
            _db = db;
        }
        
        protected override async void OnAppearing() {
            base.OnAppearing();
            
            if (int.TryParse(SkupinaId, out int skupinaId)) {
                await NacistSkupinu(skupinaId);
            } else {
                await DisplayAlert("Chyba", "Neplatné ID skupiny", "OK");
                await Navigation.PopAsync();
            }
        }
        
        private async Task NacistSkupinu(int skupinaId) {
            try {
                _skupina = await _db.Skupiny
                    .Include(s => s.Clenstvi)
                    .ThenInclude(c => c.Uzivatel)
                    .FirstOrDefaultAsync(s => s.Id == skupinaId);
                    
                if (_skupina == null) {
                    await DisplayAlert("Chyba", "Skupina nebyla nalezena", "OK");
                    await Navigation.PopAsync();
                    return;
                }
                
                var mojeClenstvi = _skupina.Clenstvi.FirstOrDefault(c => c.UzivatelId == App.CurrentUserId);
                _jsemAdmin = mojeClenstvi?.JeAdmin ?? false;
                
                if (!_jsemAdmin) {
                    await DisplayAlert("Upozornění", "Nemáte oprávnění upravovat tuto skupinu", "OK");
                    await Navigation.PopAsync();
                    return;
                }
                
                NazevEntry.Text = _skupina.Nazev;
                
                await NacistDostupneUzivatele();
                ZobrazitCleny();
                ZobrazitDostupneUzivatele();
                
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst skupinu: {ex.Message}", "OK");
            }
        }
        
        private async Task NacistDostupneUzivatele() {
            try {
                var clenstviIds = _skupina.Clenstvi.Select(c => c.UzivatelId).ToList();
                
                _dostupniUzivatele = await _db.Uzivatele
                    .Where(u => !clenstviIds.Contains(u.Id))
                    .OrderBy(u => u.Jmeno)
                    .ToListAsync();
                    
                _filtrovaníUzivatele = _dostupniUzivatele.ToList();
            } catch (Exception) {
                // Chyba při načítání uživatelů - pokračujeme dál
            }
        }
        
        private void ZobrazitCleny() {
            ClenoveSeznamContainer.Children.Clear();
            
            foreach (var clenstvi in _skupina.Clenstvi.OrderBy(c => c.Uzivatel.Jmeno)) {
                var frame = VytvoritFrameProClena(clenstvi);
                ClenoveSeznamContainer.Children.Add(frame);
            }
        }

        private Frame VytvoritFrameProClena(Clenstvi clenstvi) {
            var frame = new Frame {
                Padding = new Thickness(10, 5),
                BackgroundColor = Colors.White,
                BorderColor = Colors.LightGray,
                CornerRadius = 5,
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            var grid = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            
            var infoLayout = new VerticalStackLayout();
            infoLayout.Children.Add(new Label {
                Text = clenstvi.Uzivatel.Jmeno,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            });
            infoLayout.Children.Add(new Label {
                Text = clenstvi.Uzivatel.Email,
                FontSize = 12,
                TextColor = Colors.Gray
            });
            
            grid.Children.Add(infoLayout);
            
            if (clenstvi.JeAdmin) {
                var adminLabel = new Label {
                    Text = "Admin",
                    BackgroundColor = Colors.Gold,
                    TextColor = Colors.Black,
                    Padding = new Thickness(5, 2),
                    FontSize = 12,
                    VerticalOptions = LayoutOptions.Center
                };
                grid.Children.Add(adminLabel);
                Grid.SetColumn(adminLabel, 1);
            }
            
            if (MuzeOdebratClena(clenstvi)) {
                var odebratButton = new Button {
                    Text = "Odebrat",
                    BackgroundColor = Colors.Red,
                    TextColor = Colors.White,
                    FontSize = 12,
                    Padding = new Thickness(10, 5),
                    VerticalOptions = LayoutOptions.Center
                };
                
                odebratButton.Clicked += async (s, e) => {
                    await OdebratClena(clenstvi);
                };
                
                grid.Children.Add(odebratButton);
                Grid.SetColumn(odebratButton, 2);
            }
            
            frame.Content = grid;
            return frame;
        }

        private bool MuzeOdebratClena(Clenstvi clenstvi) {
            if (clenstvi.UzivatelId == App.CurrentUserId)
                return false;
            
            int pocetAdminu = _skupina.Clenstvi.Count(c => c.JeAdmin);
            if (clenstvi.JeAdmin && pocetAdminu == 1)
                return false;
                
            return true;
        }
        
        private void ZobrazitDostupneUzivatele() {
            DostupniUzivateleContainer.Children.Clear();
            
            if (_filtrovaníUzivatele.Count == 0) {
                DostupniUzivateleContainer.Children.Add(new Label {
                    Text = "Žádní dostupní uživatelé",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray
                });
                return;
            }
            
            foreach (var uzivatel in _filtrovaníUzivatele) {
                var frame = VytvoritFrameProUzivatele(uzivatel);
                DostupniUzivateleContainer.Children.Add(frame);
            }
        }

        private Frame VytvoritFrameProUzivatele(Uzivatel uzivatel) {
            var frame = new Frame {
                Padding = new Thickness(10, 5),
                BackgroundColor = Colors.White,
                BorderColor = Colors.LightGray,
                CornerRadius = 5,
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            var grid = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            
            var infoLayout = new VerticalStackLayout();
            infoLayout.Children.Add(new Label {
                Text = uzivatel.Jmeno,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            });
            infoLayout.Children.Add(new Label {
                Text = uzivatel.Email,
                FontSize = 12,
                TextColor = Colors.Gray
            });
            
            grid.Children.Add(infoLayout);
            
            var pridatButton = new Button {
                Text = "Přidat",
                BackgroundColor = Colors.Green,
                TextColor = Colors.White,
                FontSize = 12,
                Padding = new Thickness(10, 5),
                VerticalOptions = LayoutOptions.Center
            };
            
            pridatButton.Clicked += async (s, e) => {
                await PridatClena(uzivatel);
            };
            
            grid.Children.Add(pridatButton);
            Grid.SetColumn(pridatButton, 1);
            
            frame.Content = grid;
            return frame;
        }
        
        private void OnVyhledavaniTextChanged(object sender, TextChangedEventArgs e) {
            var hledanyText = e.NewTextValue?.ToLower() ?? "";
            
            if (string.IsNullOrWhiteSpace(hledanyText)) {
                _filtrovaníUzivatele = _dostupniUzivatele.ToList();
            } else {
                _filtrovaníUzivatele = _dostupniUzivatele
                    .Where(u => u.Jmeno.ToLower().Contains(hledanyText) || 
                               u.Email.ToLower().Contains(hledanyText))
                    .ToList();
            }
            
            ZobrazitDostupneUzivatele();
        }
        
        private async Task PridatClena(Uzivatel uzivatel) {
            try {
                var clenstvi = new Clenstvi(uzivatel.Id, _skupina.Id, false) {
                    Uzivatel = uzivatel,
                    Skupina = _skupina,
                    DatumPridani = DateTime.UtcNow
                };
                
                _db.Clenstvi.Add(clenstvi);
                await _db.SaveChangesAsync();
                
                _skupina.Clenstvi.Add(clenstvi);
                _dostupniUzivatele.Remove(uzivatel);
                _filtrovaníUzivatele.Remove(uzivatel);
                
                ZobrazitCleny();
                ZobrazitDostupneUzivatele();
                
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Nepodařilo se přidat člena: {ex.Message}", "OK");
            }
        }
        
        private async Task OdebratClena(Clenstvi clenstvi) {
            bool potvrdit = await DisplayAlert("Odebrat člena?", 
                $"Opravdu chcete odebrat {clenstvi.Uzivatel.Jmeno} ze skupiny?", 
                "Ano", "Ne");
                
            if (!potvrdit) return;
            
            try {
                bool maDluhy = await _db.Dluhy
                    .Include(d => d.Dluznik)
                    .Include(d => d.Veritel)
                    .AnyAsync(d => !d.JeSplaceno && 
                              (d.DluznikId == clenstvi.UzivatelId || d.VeritelId == clenstvi.UzivatelId) &&
                              _skupina.Vydaje.Any(v => v.Dluhy.Contains(d)));
                              
                if (maDluhy) {
                    UpozorneniLabel.Text = "Tohoto člena nelze odebrat, protože má nezaplacené dluhy ve skupině.";
                    UpozorneniLabel.IsVisible = true;
                    return;
                }
                
                _db.Clenstvi.Remove(clenstvi);
                await _db.SaveChangesAsync();
                
                _skupina.Clenstvi.Remove(clenstvi);
                _dostupniUzivatele.Add(clenstvi.Uzivatel);
                _filtrovaníUzivatele = _dostupniUzivatele.ToList();
                
                ZobrazitCleny();
                ZobrazitDostupneUzivatele();
                
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Nepodařilo se odebrat člena: {ex.Message}", "OK");
            }
        }
        
        private async void OnSaveClicked(object sender, EventArgs e) {
            try {
                ErrorLabel.IsVisible = false;
                UpozorneniLabel.IsVisible = false;
                
                if (string.IsNullOrWhiteSpace(NazevEntry.Text)) {
                    ErrorLabel.Text = "Název skupiny nemůže být prázdný";
                    ErrorLabel.IsVisible = true;
                    return;
                }
                
                _skupina.Nazev = NazevEntry.Text.Trim();
                await _db.SaveChangesAsync();
                
                await DisplayAlert("Úspěch", "Změny byly uloženy", "OK");
                await Navigation.PopAsync();
                
            } catch (Exception ex) {
                ErrorLabel.Text = $"Chyba při ukládání: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }
        
        private async void OnDeleteGroupClicked(object sender, EventArgs e) {
            bool potvrdit = await DisplayAlert("Smazat skupinu?", 
                $"Opravdu chcete smazat skupinu '{_skupina.Nazev}'? Tato akce je nevratná a smaže všechny výdaje a dluhy skupiny.", 
                "Ano, smazat", "Ne");
                
            if (!potvrdit) return;
            
            try {
                var dluhy = await _db.Dluhy
                    .Where(d => _skupina.Vydaje.Any(v => v.Dluhy.Contains(d)))
                    .ToListAsync();
                _db.Dluhy.RemoveRange(dluhy);
                
                _db.Vydaje.RemoveRange(_skupina.Vydaje);
                _db.Clenstvi.RemoveRange(_skupina.Clenstvi);
                _db.Skupiny.Remove(_skupina);
                
                await _db.SaveChangesAsync();
                
                await DisplayAlert("Úspěch", "Skupina byla smazána", "OK");
                await Shell.Current.GoToAsync("//main");
                
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Nepodařilo se smazat skupinu: {ex.Message}", "OK");
            }
        }
        
        private async void OnCancelClicked(object sender, EventArgs e) {
            await Navigation.PopAsync();
        }
    }
}