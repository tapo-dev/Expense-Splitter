using Microsoft.EntityFrameworkCore;
using RoommateApp.Core.Data;
using RoommateApp.Core.Models;
using RoommateApp.Core.Services;
using RoommateApp.Core.Strategies;

namespace RoommateApp.Maui.Views {
    public partial class AddExpensePage : ContentPage {
        private readonly AppDbContext _db;
        private readonly SpravceUctu _spravceUctu;
        private List<Skupina> _skupiny = new();
        private List<Uzivatel> _clenovePridanehoVydaje = new();
        private List<Uzivatel> _vsichniClenovePridanehoVydaje = new();
        private Dictionary<int, CheckBox> _ucastniciCheckboxy = new();
        
        private Uzivatel _aktualniUzivatel;
        
        public AddExpensePage(AppDbContext db, SpravceUctu spravceUctu) {
            InitializeComponent();
            _db = db;
            _spravceUctu = spravceUctu;
            
            DatumPicker.Date = DateTime.Today;
        }
        
        protected override async void OnAppearing() {
            base.OnAppearing();
            await NacistData();
        }
        
        private async Task NacistData() {
            try {
                _aktualniUzivatel = await _db.Uzivatele
                    .Include(u => u.Clenstvi)
                    .ThenInclude(c => c.Skupina)
                    .FirstOrDefaultAsync(u => u.Id == App.CurrentUserId);
                    
                if (_aktualniUzivatel == null) {
                    await DisplayAlert("Chyba", "Nebyl nalezen žádný uživatel.", "OK");
                    await Navigation.PopAsync();
                    return;
                }
                
                var clenstviIds = _aktualniUzivatel.Clenstvi.Select(c => c.SkupinaId).ToList();
                _skupiny = await _db.Skupiny
                    .Include(s => s.Clenstvi)
                    .ThenInclude(c => c.Uzivatel)
                    .Where(s => clenstviIds.Contains(s.Id))
                    .ToListAsync();
                    
                SkupinaPicker.ItemsSource = _skupiny;
                SkupinaPicker.SelectedIndexChanged += OnSkupinaSelected;
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst data: {ex.Message}", "OK");
            }
        }
        
        private void OnSkupinaSelected(object sender, EventArgs e) {
            if (SkupinaPicker.SelectedItem is Skupina vybranaSkupina) {
                _vsichniClenovePridanehoVydaje = vybranaSkupina.ZiskejCleny();
                _clenovePridanehoVydaje.Clear();
                
                PlatilPicker.ItemsSource = _vsichniClenovePridanehoVydaje;
                
                var indexAktualnihoUzivatele = _vsichniClenovePridanehoVydaje.FindIndex(u => u.Id == App.CurrentUserId);
                if (indexAktualnihoUzivatele >= 0) {
                    PlatilPicker.SelectedIndex = indexAktualnihoUzivatele;
                } else if (_vsichniClenovePridanehoVydaje.Count > 0) {
                    PlatilPicker.SelectedIndex = 0;
                }
                
                VytvoritCheckboxyUcastniku();
                AktualizovatInfoUcastniku();
            }
        }

        private void VytvoritCheckboxyUcastniku() {
            UcastniciCheckboxContainer.Children.Clear();
            _ucastniciCheckboxy.Clear();
            
            foreach (var uzivatel in _vsichniClenovePridanehoVydaje) {
                var checkbox = new CheckBox {
                    IsChecked = true
                };
                
                checkbox.CheckedChanged += (s, args) => {
                    if (checkbox.IsChecked) {
                        if (!_clenovePridanehoVydaje.Contains(uzivatel)) {
                            _clenovePridanehoVydaje.Add(uzivatel);
                        }
                    } else {
                        _clenovePridanehoVydaje.Remove(uzivatel);
                    }
                };
                
                if (!_clenovePridanehoVydaje.Contains(uzivatel)) {
                    _clenovePridanehoVydaje.Add(uzivatel);
                }
                
                _ucastniciCheckboxy[uzivatel.Id] = checkbox;
                
                var horizontalLayout = new HorizontalStackLayout {
                    Spacing = 10,
                    VerticalOptions = LayoutOptions.Center
                };
                
                horizontalLayout.Children.Add(checkbox);
                horizontalLayout.Children.Add(new Label {
                    Text = uzivatel.Jmeno,
                    VerticalOptions = LayoutOptions.Center
                });
                
                UcastniciCheckboxContainer.Children.Add(horizontalLayout);
            }
        }

        private void AktualizovatInfoUcastniku() {
            if (_vsichniClenovePridanehoVydaje.Count > 0) {
                UcastniciInfo.Text = $"Vyberte, kdo se účastní výdaje ({_vsichniClenovePridanehoVydaje.Count} členů):";
            } else {
                UcastniciInfo.Text = "Skupina nemá žádné členy";
            }
        }
        
        private async void OnSaveClicked(object sender, EventArgs e) {
            try {
                if (string.IsNullOrWhiteSpace(PopisEntry.Text)) {
                    await DisplayAlert("Chyba", "Zadejte popis výdaje.", "OK");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(CastkaEntry.Text) || !decimal.TryParse(CastkaEntry.Text, out decimal castka)) {
                    await DisplayAlert("Chyba", "Zadejte platnou částku.", "OK");
                    return;
                }
                
                if (SkupinaPicker.SelectedItem == null) {
                    await DisplayAlert("Chyba", "Vyberte skupinu.", "OK");
                    return;
                }
                
                if (PlatilPicker.SelectedItem == null) {
                    await DisplayAlert("Chyba", "Vyberte, kdo výdaj platil.", "OK");
                    return;
                }
                
                if (_clenovePridanehoVydaje.Count == 0) {
                    await DisplayAlert("Chyba", "Vyberte alespoň jednoho účastníka výdaje.", "OK");
                    return;
                }
                
                var vybranaSkupina = (Skupina)SkupinaPicker.SelectedItem;
                var vybranyPlatce = (Uzivatel)PlatilPicker.SelectedItem;
                var datumVydaje = DatumPicker.Date;
                
                var novyVydaj = new Vydaj(
                    PopisEntry.Text, 
                    castka, 
                    datumVydaje, 
                    vybranyPlatce.Id, 
                    vybranaSkupina.Id) {
                    Platil = vybranyPlatce,
                    Skupina = vybranaSkupina
                };
                
                _db.Vydaje.Add(novyVydaj);
                await _db.SaveChangesAsync();
                
                VypocitatAUlozitDluhy(novyVydaj, vybranaSkupina, vybranyPlatce, castka);
                
                await _db.SaveChangesAsync();
                
                await DisplayAlert("Úspěch", "Výdaj byl úspěšně přidán.", "OK");
                await Navigation.PopAsync();
            } catch (Exception ex) {
                await DisplayAlert("Chyba", $"Nepodařilo se uložit výdaj: {ex.Message}", "OK");
            }
        }

        private void VypocitatAUlozitDluhy(Vydaj novyVydaj, Skupina vybranaSkupina, Uzivatel vybranyPlatce, decimal castka) {
            var dluhy = new List<Dluh>();
            
            var pocetUcastniku = _clenovePridanehoVydaje.Count;
            
            if (_clenovePridanehoVydaje.Any(u => u.Id == vybranyPlatce.Id)) {
                pocetUcastniku--;
            }
            
            if (pocetUcastniku > 0 && castka > 0) {
                var castkaNaOsobu = castka / (_clenovePridanehoVydaje.Count);
                
                foreach (var uzivatel in _clenovePridanehoVydaje) {
                    if (uzivatel.Id == vybranyPlatce.Id) {
                        continue;
                    }
                    
                    var dluh = new Dluh(uzivatel.Id, vybranyPlatce.Id, castkaNaOsobu) {
                        Dluznik = uzivatel,
                        Veritel = vybranyPlatce,
                        JeSplaceno = false
                    };
                    
                    dluhy.Add(dluh);
                    novyVydaj.Dluhy.Add(dluh);
                }
                
                _db.Dluhy.AddRange(dluhy);
            }
        }
        
        private async void OnCancelClicked(object sender, EventArgs e) {
            await Navigation.PopAsync();
        }
    }
}