using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RoommateApp.Core.Data;
using RoommateApp.Core.Factories;
using RoommateApp.Core.Models;
using RoommateApp.Core.Observers;
using RoommateApp.Core.Services;
using RoommateApp.Core.ViewModels;
using RoommateApp.Core.ViewModels.Commands;
using RoommateApp.Maui.Views;

namespace RoommateApp.Maui.ViewModels {
    /// <summary>
    /// Wrapper pro dluh s barvou podle typu
    /// </summary>
    public class DluhWrapper {
        public Dluh Dluh { get; set; }
        public Color BackgroundColor { get; set; }
        
        public DluhWrapper(Dluh dluh, int currentUserId) {
            Dluh = dluh;
            
            if (dluh.VeritelId == currentUserId) {
                BackgroundColor = Colors.LightGreen; // Můžeš označit jako splacený
            } else if (dluh.DluznikId == currentUserId) {
                BackgroundColor = Colors.LightCoral; // Dlužíš ty
            } else {
                BackgroundColor = Colors.LightGray; // Netýká se tě
            }
        }
    }
    
    /// <summary>
    /// ViewModel pro GroupsPage - komplexní správa skupin
    /// </summary>
    public class GroupsPageViewModel : BaseViewModel, IObserver {
        private readonly AppDbContext _db;
        private readonly SkupinaService _skupinaService;
        
        public GroupsPageViewModel(AppDbContext db, SkupinaService skupinaService) {
            _db = db;
            _skupinaService = skupinaService;
            
            Skupiny = new ObservableCollection<Skupina>();
            Vydaje = new ObservableCollection<Vydaj>();
            Dluhy = new ObservableCollection<DluhWrapper>();
            
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            SkupinaSelectedCommand = new AsyncRelayCommand<Skupina>(OnSkupinaSelectedAsync);
            VydajSelectedCommand = new RelayCommand<Vydaj>(OnVydajSelected);
            OznacitDluhSplacenyCommand = new AsyncRelayCommand<DluhWrapper>(OznacitDluhJakoSplacenyAsync);
            AddExpenseCommand = new AsyncRelayCommand(AddExpenseAsync);
            AddGroupCommand = new AsyncRelayCommand(AddGroupAsync);
            SkupinaMenuCommand = new AsyncRelayCommand(ShowSkupinaMenuAsync);
        }

        #region Properties

        public ObservableCollection<Skupina> Skupiny { get; }
        public ObservableCollection<Vydaj> Vydaje { get; }
        public ObservableCollection<DluhWrapper> Dluhy { get; }

        private Skupina _vybranaSkupina;
        private bool _isLoadingSkupina = false;

        public Skupina VybranaSkupina {
            get => _vybranaSkupina;
            set {
                if (SetProperty(ref _vybranaSkupina, value) && value != null && !_isLoadingSkupina) {
                    Task.Run(async () => await OnSkupinaSelectedAsync(value));
                }
            }
        }

        private Uzivatel _aktualniUzivatel;
        public Uzivatel AktualniUzivatel {
            get => _aktualniUzivatel;
            set => SetProperty(ref _aktualniUzivatel, value);
        }

        private string _nazevSkupiny = "";
        public string NazevSkupiny {
            get => _nazevSkupiny;
            set => SetProperty(ref _nazevSkupiny, value);
        }

        private string _pocetClenu = "";
        public string PocetClenu {
            get => _pocetClenu;
            set => SetProperty(ref _pocetClenu, value);
        }

        private string _celkoveVydaje = "";
        public string CelkoveVydaje {
            get => _celkoveVydaje;
            set => SetProperty(ref _celkoveVydaje, value);
        }

        private string _stavVyrovnani = "";
        public string StavVyrovnani {
            get => _stavVyrovnani;
            set => SetProperty(ref _stavVyrovnani, value);
        }

        private bool _isDetailVydajeVisible;
        public bool IsDetailVydajeVisible {
            get => _isDetailVydajeVisible;
            set => SetProperty(ref _isDetailVydajeVisible, value);
        }

        private string _nazevVydaje = "";
        public string NazevVydaje {
            get => _nazevVydaje;
            set => SetProperty(ref _nazevVydaje, value);
        }

        private string _castkaVydaje = "";
        public string CastkaVydaje {
            get => _castkaVydaje;
            set => SetProperty(ref _castkaVydaje, value);
        }

        private string _datumVydaje = "";
        public string DatumVydaje {
            get => _datumVydaje;
            set => SetProperty(ref _datumVydaje, value);
        }

        private string _platilVydaj = "";
        public string PlatilVydaj {
            get => _platilVydaj;
            set => SetProperty(ref _platilVydaj, value);
        }

        private string _pocetUcastniku = "";
        public string PocetUcastniku {
            get => _pocetUcastniku;
            set => SetProperty(ref _pocetUcastniku, value);
        }

        private bool _zadneDluhy;
        public bool ZadneDluhy {
            get => _zadneDluhy;
            set => SetProperty(ref _zadneDluhy, value);
        }

        #endregion

        #region Commands

        public AsyncRelayCommand LoadDataCommand { get; }
        public AsyncRelayCommand<Skupina> SkupinaSelectedCommand { get; }
        public RelayCommand<Vydaj> VydajSelectedCommand { get; }
        public AsyncRelayCommand<DluhWrapper> OznacitDluhSplacenyCommand { get; }
        public AsyncRelayCommand AddExpenseCommand { get; }
        public AsyncRelayCommand AddGroupCommand { get; }
        public AsyncRelayCommand SkupinaMenuCommand { get; }

        #endregion

        #region Public Methods

        public async Task LoadDataAsync() {
            ClearError();
            IsBusy = true;

            try {
                AktualniUzivatel = await _db.Uzivatele
                    .Include(u => u.Clenstvi)
                    .ThenInclude(c => c.Skupina)
                    .FirstOrDefaultAsync(u => u.Id == App.CurrentUserId);

                if (AktualniUzivatel == null) {
                    SetError("Uživatel nebyl nalezen");
                    return;
                }

                var clenstviIds = AktualniUzivatel.Clenstvi.Select(c => c.SkupinaId).ToList();
                var skupiny = await _db.Skupiny
                    .Include(s => s.Clenstvi)
                    .ThenInclude(c => c.Uzivatel)
                    .Where(s => clenstviIds.Contains(s.Id))
                    .ToListAsync();

                Skupiny.Clear();
                foreach (var skupina in skupiny) {
                    Skupiny.Add(skupina);
                }
                
                if (Skupiny.Count > 0 && VybranaSkupina == null) {
                    _isLoadingSkupina = true;
                    VybranaSkupina = Skupiny[0];
                    _isLoadingSkupina = false;
                    await OnSkupinaSelectedAsync(Skupiny[0]);
                }
            } catch (Exception ex) {
                SetError($"Chyba při načítání dat: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        public async Task LoadSkupinaByIdAsync(int skupinaId) {
            if (Skupiny.Count == 0) {
                await LoadDataAsync();
            }
    
            var skupina = Skupiny.FirstOrDefault(s => s.Id == skupinaId);
            if (skupina != null) {
                _isLoadingSkupina = true;
                VybranaSkupina = skupina;
                _isLoadingSkupina = false;
                await OnSkupinaSelectedAsync(skupina);
            }
        }

        #endregion

        #region Private Methods

        private async Task OnSkupinaSelectedAsync(Skupina skupina) {
            if (skupina == null) return;

            ClearError();
            IsBusy = true;

            try {
                var skupinaDetail = await _db.Skupiny
                    .Include(s => s.Clenstvi)
                    .ThenInclude(c => c.Uzivatel)
                    .Include(s => s.Vydaje)
                    .ThenInclude(v => v.Platil)
                    .Include(s => s.Vydaje)
                    .ThenInclude(v => v.Dluhy)
                    .ThenInclude(d => d.Dluznik)
                    .Include(s => s.Vydaje)
                    .ThenInclude(v => v.Dluhy)
                    .ThenInclude(d => d.Veritel)
                    .FirstOrDefaultAsync(s => s.Id == skupina.Id);

                if (skupinaDetail == null) {
                    SetError("Skupina nebyla nalezena");
                    return;
                }

                _isLoadingSkupina = true;
                VybranaSkupina = skupinaDetail;
                _isLoadingSkupina = false;

                NazevSkupiny = skupinaDetail.Nazev;
                PocetClenu = $"{skupinaDetail.Clenstvi.Count} členů";

                decimal celkoveVydaje = skupinaDetail.Vydaje.Sum(v => v.Castka);
                CelkoveVydaje = $"Celkem: {celkoveVydaje} Kč";

                var vsechnyDluhy = skupinaDetail.ZiskejDluhy();
                int celkemDluhu = vsechnyDluhy.Count;
                int splacenoDluhu = vsechnyDluhy.Count(d => d.JeSplaceno);
                double procentoSplaceni = celkemDluhu > 0 ? (splacenoDluhu / (double)celkemDluhu) * 100 : 100;
                StavVyrovnani = $"Vyrovnáno z {procentoSplaceni:F0}%";

                var vydajeList = skupinaDetail.Vydaje.OrderByDescending(v => v.Datum).ToList();
                var dluhyList = skupinaDetail.ZiskejDluhy()
                    .Where(d => !d.JeSplaceno && (d.DluznikId == App.CurrentUserId || d.VeritelId == App.CurrentUserId))
                    .Select(d => new DluhWrapper(d, App.CurrentUserId))
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() => {
                    Vydaje.Clear();
                    foreach (var vydaj in vydajeList) {
                        Vydaje.Add(vydaj);
                    }

                    Dluhy.Clear();
                    foreach (var dluh in dluhyList) {
                        Dluhy.Add(dluh);
                    }

                    ZadneDluhy = Dluhy.Count == 0;
                });

                ZadneDluhy = Dluhy.Count == 0;
                IsDetailVydajeVisible = false;
            } catch (Exception ex) {
                SetError($"Chyba při načítání skupiny: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        private void OnVydajSelected(Vydaj vydaj) {
            if (vydaj == null) return;

            IsDetailVydajeVisible = true;
            NazevVydaje = vydaj.Popis;
            CastkaVydaje = $"{vydaj.Castka} Kč";
            DatumVydaje = vydaj.Datum.ToString("dd.MM.yyyy");
            PlatilVydaj = vydaj.Platil?.Jmeno ?? $"ID:{vydaj.PlatilId}";

            var ucastnici = new HashSet<int> { vydaj.PlatilId };
            foreach (var dluh in vydaj.Dluhy) {
                ucastnici.Add(dluh.DluznikId);
            }
            PocetUcastniku = $"{ucastnici.Count}";
        }

        private async Task OznacitDluhJakoSplacenyAsync(DluhWrapper dluhWrapper) {
            if (dluhWrapper?.Dluh == null) return;
    
            var dluh = dluhWrapper.Dluh;

            if (dluh.VeritelId != App.CurrentUserId) {
                await Application.Current.MainPage.DisplayAlert("Chyba", "Pouze věřitel může potvrdit splacení dluhu.", "OK");
                return;
            }

            bool potvrdit = await Application.Current.MainPage.DisplayAlert(
                "Potvrdit splacení?",
                $"{dluh.Dluznik?.Jmeno ?? "Neznámý"} ti zaplatil {dluh.Castka} Kč?\n\nPotvrzením označíš dluh jako splacený.",
                "Ano, zaplatil", "Ne");

            if (!potvrdit) return;

            try {
                var factory = new NotifierFactory();
                var emailNotifier = factory.CreateNotifier("email");
                var inAppNotifier = factory.CreateNotifier("inapp");

                dluh.RegisterObserver(emailNotifier);
                dluh.RegisterObserver(inAppNotifier);
                dluh.RegisterObserver(this);

                dluh.OznacitJakoSplacene();

                await _db.SaveChangesAsync();

                Dluhy.Remove(dluhWrapper);
                ZadneDluhy = Dluhy.Count == 0;

                await Application.Current.MainPage.DisplayAlert("Úspěch", "Dluh byl označen jako splacený.", "OK");
            } catch (Exception ex) {
                SetError($"Chyba při označování dluhu: {ex.Message}");
            }
        }

        private async Task AddExpenseAsync() {
            if (VybranaSkupina != null) {
                await Shell.Current.GoToAsync($"addexpense?skupinaId={VybranaSkupina.Id}");
            }
        }

        private async Task AddGroupAsync() {
            try {
                var createGroupPage = new CreateGroupPage(
                    _db.GetService<AppDbContext>()
                );
        
                await Application.Current.MainPage.Navigation.PushAsync(createGroupPage);
            } catch (Exception ex) {
                SetError($"Chyba při navigaci: {ex.Message}");
            }
        }

        private async Task ShowSkupinaMenuAsync() {
            if (VybranaSkupina == null) return;

            var clenstvi = VybranaSkupina.Clenstvi.FirstOrDefault(c => c.UzivatelId == App.CurrentUserId);
            if (clenstvi == null) {
                await Application.Current.MainPage.DisplayAlert("Chyba", "Nejste členem této skupiny!", "OK");
                return;
            }

            string akce;

            if (clenstvi.JeAdmin) {
                akce = await Application.Current.MainPage.DisplayActionSheet(
                    $"Možnosti pro skupinu '{VybranaSkupina.Nazev}'",
                    "Zrušit",
                    "Odstranit skupinu",
                    "Upravit název",
                    "Zobrazit členy");
            } else {
                akce = await Application.Current.MainPage.DisplayActionSheet(
                    $"Možnosti pro skupinu '{VybranaSkupina.Nazev}'",
                    "Zrušit",
                    null,
                    "Zobrazit členy");
            }

            switch (akce) {
                case "Upravit název":
                    await UpravitNazevSkupinyAsync();
                    break;
                case "Odstranit skupinu":
                    await OdstranitSkupinuAsync();
                    break;
                case "Zobrazit členy":
                    await ZobrazitClenyAsync();
                    break;
            }
        }

        private async Task UpravitNazevSkupinyAsync() {
            if (VybranaSkupina == null) return;

            string novyNazev = await Application.Current.MainPage.DisplayPromptAsync(
                "Upravit skupinu",
                "Zadejte nový název skupiny:",
                initialValue: VybranaSkupina.Nazev,
                maxLength: 100);

            if (!string.IsNullOrWhiteSpace(novyNazev) && novyNazev != VybranaSkupina.Nazev) {
                var (uspech, chyba) = await _skupinaService.UpravitNazevSkupinyAsync(
                    VybranaSkupina.Id,
                    App.CurrentUserId,
                    novyNazev);

                if (uspech) {
                    NazevSkupiny = novyNazev;
                    VybranaSkupina.Nazev = novyNazev;
                    await Application.Current.MainPage.DisplayAlert("Úspěch", "Název skupiny byl upraven.", "OK");
                } else {
                    await Application.Current.MainPage.DisplayAlert("Chyba", chyba, "OK");
                }
            }
        }

        private async Task OdstranitSkupinuAsync() {
            if (VybranaSkupina == null) return;

            var (celkem, splacene, nezaplacene, celkovaCastka, nezaplacenaCastka) = VybranaSkupina.ZiskejStatistikyDluhu();
            var pocetClenu = VybranaSkupina.Clenstvi?.Count ?? 0;
            var pocetVydaju = VybranaSkupina.Vydaje?.Count ?? 0;

            string varovani = "";
            if (nezaplacene > 0) {
                varovani = $"\n\n⚠️ POZOR: Ve skupině jsou {nezaplacene} nezaplacené dluhy v hodnotě {nezaplacenaCastka} Kč!";
            }

            bool potvrzeni = await Application.Current.MainPage.DisplayAlert(
                "Odstranit skupinu",
                $"Opravdu chcete odstranit skupinu '{VybranaSkupina.Nazev}'?\n\n" +
                $"Skupina obsahuje:\n" +
                $"• {pocetClenu} členů\n" +
                $"• {pocetVydaju} výdajů\n" +
                $"• {celkem} dluhů (z toho {nezaplacene} nezaplacených){varovani}\n\n" +
                "Tato akce odstraní všechny výdaje a dluhy v této skupině a nelze ji vrátit zpět.",
                "Ano, odstranit", "Zrušit");

            if (potvrzeni) {
                var (uspech, chyba) = await _skupinaService.OdstranitSkupinuAsync(VybranaSkupina.Id, App.CurrentUserId);

                if (uspech) {
                    Skupiny.Remove(VybranaSkupina);

                    if (Skupiny.Count > 0) {
                        await OnSkupinaSelectedAsync(Skupiny[0]);
                    } else {
                        VybranaSkupina = null;
                        NazevSkupiny = "Žádné skupiny";
                        PocetClenu = "0 členů";
                        CelkoveVydaje = "0 Kč";
                        StavVyrovnani = "Vyrovnáno z 0%";
                        Vydaje.Clear();
                        Dluhy.Clear();
                        ZadneDluhy = true;
                        IsDetailVydajeVisible = false;
                    }

                    await Application.Current.MainPage.DisplayAlert("Úspěch", "Skupina byla odstraněna.", "OK");
                } else {
                    await Application.Current.MainPage.DisplayAlert("Chyba", chyba, "OK");
                }
            }
        }

        private async Task ZobrazitClenyAsync() {
            if (VybranaSkupina == null) return;

            try {
                var detailyClenu = VybranaSkupina.ZiskejDetailyClenu();

                if (!detailyClenu.Any()) {
                    await Application.Current.MainPage.DisplayAlert("Členové skupiny",
                        $"Skupina '{VybranaSkupina.Nazev}' nemá žádné členy.", "OK");
                    return;
                }

                var seznamClenu = new List<string>();

                foreach (var (uzivatel, jeAdmin, datumPridani) in detailyClenu) {
                    string status = jeAdmin ? " (Admin)" : "";
                    status += $" - přidán {datumPridani:dd.MM.yyyy}";

                    var bilance = VybranaSkupina.ZiskejBilanciUzivatele(uzivatel.Id);
                    string bilanceText = "";
                    if (bilance > 0) {
                        bilanceText = $" [+{bilance} Kč]";
                    } else if (bilance < 0) {
                        bilanceText = $" [{bilance} Kč]";
                    }

                    seznamClenu.Add($"• {uzivatel.Jmeno}{status}{bilanceText}");
                }

                string zprava = $"Skupina '{VybranaSkupina.Nazev}' má {detailyClenu.Count} členů:\n\n" +
                               string.Join("\n", seznamClenu) +
                               "\n\n[+X Kč] = má dostat, [-X Kč] = dluží";

                await Application.Current.MainPage.DisplayAlert("Členové skupiny", zprava, "OK");
            } catch (Exception ex) {
                SetError($"Chyba při zobrazování členů: {ex.Message}");
            }
        }

        #endregion

        #region IObserver Implementation

        public void Update(object data) {
            if (data is Dluh dluh) {
                MainThread.BeginInvokeOnMainThread(() => {
                    var dluhKOdstraneni = Dluhy.FirstOrDefault(d => d.Dluh.Id == dluh.Id);
                    if (dluhKOdstraneni != null) {
                        Dluhy.Remove(dluhKOdstraneni);
                        ZadneDluhy = Dluhy.Count == 0;
                    }
                });
            }
        }

        #endregion
    }
}