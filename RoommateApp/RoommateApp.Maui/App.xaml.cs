using RoommateApp.Core.Services;
using RoommateApp.Core.Models;

namespace RoommateApp.Maui;

public partial class App : Application {
    public static Uzivatel CurrentUser { get; set; }
    public static int CurrentUserId { get; set; }
    
    public App(IServiceProvider services) {
        InitializeComponent();
        
        MainPage = new AppShell();
        
        // Spusť DataSeeder asynchronně
        Task.Run(async () => {
            try {
                using (var scope = services.CreateScope()) {
                    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                    await dataSeeder.SeedAsync();
                    System.Diagnostics.Debug.WriteLine("Data byla úspěšně seedována");
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Chyba při seedování dat: {ex.Message}");
            }
        });
        
        // Kontrola, zda je uživatel přihlášen
        if (Preferences.ContainsKey("LoggedInUserId")) {
            int userId = Preferences.Get("LoggedInUserId", 0);
            if (userId > 0) {
                CurrentUserId = userId;
                
                System.Diagnostics.Debug.WriteLine($"Nalezeno ID přihlášeného uživatele: {userId}");
                
                // Načtení uživatele - provedeme to asynchronně
                Task.Run(async () => {
                    try {
                        using (var scope = services.CreateScope()) {
                            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
                            CurrentUser = await authService.NacistUzivateleAsync(userId);
                            
                            // Pokud se nepodařilo načíst uživatele, přesměrujeme na login
                            if (CurrentUser == null) {
                                System.Diagnostics.Debug.WriteLine("Uživatel nenalezen v databázi");
                                MainThread.BeginInvokeOnMainThread(() => {
                                    Shell.Current.GoToAsync("//login");
                                });
                            } else {
                                System.Diagnostics.Debug.WriteLine($"Uživatel načten: {CurrentUser.Jmeno}");
                                // Přesměrování na hlavní stránku
                                MainThread.BeginInvokeOnMainThread(() => {
                                    Shell.Current.GoToAsync("//main");
                                });
                            }
                        }
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"Chyba při načítání uživatele: {ex.Message}");
                        // V případě chyby přesměrujeme na login
                        MainThread.BeginInvokeOnMainThread(() => {
                            Shell.Current.GoToAsync("//login");
                        });
                    }
                });
            } else {
                System.Diagnostics.Debug.WriteLine("ID uživatele je neplatné, přesměrování na login");
                // Přesměrování na login
                Shell.Current.GoToAsync("//login");
            }
        } else {
            System.Diagnostics.Debug.WriteLine("Žádný přihlášený uživatel nenalezen, přesměrování na login");
            // Uživatel není přihlášen
            Shell.Current.GoToAsync("//login");
        }
    }
}