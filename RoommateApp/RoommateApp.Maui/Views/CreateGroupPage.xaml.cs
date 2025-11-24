using Microsoft.EntityFrameworkCore;
using RoommateApp.Core.Data;
using RoommateApp.Core.Models;

namespace RoommateApp.Maui.Views {
    public partial class CreateGroupPage : ContentPage {
        private readonly AppDbContext _db;
        private List<Uzivatel> _vsichniUzivatele = new();
        private List<Uzivatel> _filtrovaníUzivatele = new();
        private Dictionary<int, CheckBox> _uzivateleCheckboxy = new();
        private List<Uzivatel> _vybraniClenove = new();
        
        public CreateGroupPage(AppDbContext db) {
            InitializeComponent();
            _db = db;
        }
        
        protected override async void OnAppearing() {
            base.OnAppearing();
            await NacistUzivatele();
        }
        
        private async Task NacistUzivatele() {
            try {
                _vsichniUzivatele = await _db.Uzivatele
                    .Where(u => u.Id != App.CurrentUserId)
                    .OrderBy(u => u.Jmeno)
                    .ToListAsync();
                    
                _filtrovaníUzivatele = _vsichniUzivatele.ToList();
                
                var aktualniUzivatel = await _db.Uzivatele.FindAsync(App.CurrentUserId);
                if (aktualniUzivatel != null) {
                    _vybraniClenove.Add(aktualniUzivatel);
                    AktualizovatVybraneClenoveLabel();
                }
                
                ZobrazitUzivatele();
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst uživatele: {ex.Message}", "OK");
            }
        }
        
        private void ZobrazitUzivatele() {
            UzivateleContainer.Children.Clear();
            _uzivateleCheckboxy.Clear();
            
            if (_filtrovaníUzivatele.Count == 0) {
                UzivateleContainer.Children.Add(new Label {
                    Text = "Žádní uživatelé nenalezeni",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray
                });
                return;
            }
            
            foreach (var uzivatel in _filtrovaníUzivatele) {
                var checkbox = new CheckBox {
                    IsChecked = _vybraniClenove.Any(u => u.Id == uzivatel.Id)
                };
                
                checkbox.CheckedChanged += (s, e) => {
                    if (checkbox.IsChecked) {
                        if (!_vybraniClenove.Any(u => u.Id == uzivatel.Id)) {
                            _vybraniClenove.Add(uzivatel);
                        }
                    } else {
                        _vybraniClenove.RemoveAll(u => u.Id == uzivatel.Id);
                    }
                    AktualizovatVybraneClenoveLabel();
                };
                
                _uzivateleCheckboxy[uzivatel.Id] = checkbox;
                
                var horizontalLayout = new HorizontalStackLayout {
                    Spacing = 10,
                    Padding = new Thickness(5)
                };
                
                horizontalLayout.Children.Add(checkbox);
                
                var labelLayout = new VerticalStackLayout {
                    VerticalOptions = LayoutOptions.Center
                };
                
                labelLayout.Children.Add(new Label {
                    Text = uzivatel.Jmeno,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold
                });
                
                labelLayout.Children.Add(new Label {
                    Text = uzivatel.Email,
                    FontSize = 12,
                    TextColor = Colors.Gray
                });
                
                horizontalLayout.Children.Add(labelLayout);
                
                UzivateleContainer.Children.Add(horizontalLayout);
            }
        }
        
        private void OnVyhledavaniTextChanged(object sender, TextChangedEventArgs e) {
            var hledanyText = e.NewTextValue?.ToLower() ?? "";
            
            if (string.IsNullOrWhiteSpace(hledanyText)) {
                _filtrovaníUzivatele = _vsichniUzivatele.ToList();
            } else {
                _filtrovaníUzivatele = _vsichniUzivatele
                    .Where(u => u.Jmeno.ToLower().Contains(hledanyText) || 
                               u.Email.ToLower().Contains(hledanyText))
                    .ToList();
            }
            
            ZobrazitUzivatele();
        }
        
        private void AktualizovatVybraneClenoveLabel() {
            if (_vybraniClenove.Count == 0) {
                VybraniClenoveLabel.Text = "Zatím nikdo";
                VybraniClenoveLabel.TextColor = Colors.Gray;
            } else {
                var jmena = string.Join(", ", _vybraniClenove.Select(u => u.Jmeno));
                VybraniClenoveLabel.Text = jmena;
                VybraniClenoveLabel.TextColor = Colors.Black;
            }
        }
        
        private async void OnCreateClicked(object sender, EventArgs e) {
            try {
                ErrorLabel.IsVisible = false;
                
                if (string.IsNullOrWhiteSpace(NazevEntry.Text)) {
                    ErrorLabel.Text = "Zadejte název skupiny";
                    ErrorLabel.IsVisible = true;
                    return;
                }
                
                if (_vybraniClenove.Count < 2) {
                    ErrorLabel.Text = "Skupina musí mít alespoň 2 členy (včetně vás)";
                    ErrorLabel.IsVisible = true;
                    return;
                }
                
                var novaSkupina = new Skupina(NazevEntry.Text.Trim());
                _db.Skupiny.Add(novaSkupina);
                await _db.SaveChangesAsync();
                
                bool prvniClen = true;
                foreach (var uzivatel in _vybraniClenove) {
                    bool jeAdmin = prvniClen && uzivatel.Id == App.CurrentUserId;
                    
                    var clenstvi = new Clenstvi(uzivatel.Id, novaSkupina.Id, jeAdmin) {
                        Uzivatel = uzivatel,
                        Skupina = novaSkupina,
                        DatumPridani = DateTime.UtcNow
                    };
                    
                    _db.Clenstvi.Add(clenstvi);
                    prvniClen = false;
                }
                
                await _db.SaveChangesAsync();
                
                await DisplayAlert("Úspěch", $"Skupina '{novaSkupina.Nazev}' byla úspěšně vytvořena", "OK");
                await Navigation.PopAsync();
                
            } catch (Exception ex) {
                ErrorLabel.Text = $"Chyba při vytváření skupiny: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }
        
        private async void OnCancelClicked(object sender, EventArgs e) {
            await Navigation.PopAsync();
        }
    }
}