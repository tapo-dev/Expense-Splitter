using Microsoft.EntityFrameworkCore;
using RoommateApp.Core.Data;
using RoommateApp.Core.Models;
using RoommateApp.Core.Strategies;
using BCrypt.Net;

namespace RoommateApp.Core.Services {
    public class DataSeeder {
        private readonly AppDbContext _db;

        public DataSeeder(AppDbContext db) {
            _db = db;
        }

        public async Task SeedAsync() {
            await _db.Database.EnsureCreatedAsync();

            bool maTestovaciData = await _db.Skupiny.AnyAsync(s => s.Nazev == "Skupina 1 - Testovaci skupina") && 
                                  await _db.Vydaje.AnyAsync();

            if (!maTestovaciData) {
                try {
                    // Vytvoření uživatelů
                    var uzivatel1 = new Uzivatel("Petr", "petr@example.com", BCrypt.Net.BCrypt.HashPassword("123"));
                    var uzivatel2 = new Uzivatel("Eva", "eva@example.com", BCrypt.Net.BCrypt.HashPassword("123"));
                    var uzivatel3 = new Uzivatel("Jan", "jan@example.com", BCrypt.Net.BCrypt.HashPassword("123"));
                    var uzivatel4 = new Uzivatel("Lucie", "lucie@example.com", BCrypt.Net.BCrypt.HashPassword("123"));
                    var uzivatel5 = new Uzivatel("Martin", "martin@example.com", BCrypt.Net.BCrypt.HashPassword("123"));
                    
                    _db.Uzivatele.AddRange(uzivatel1, uzivatel2, uzivatel3, uzivatel4, uzivatel5);
                    await _db.SaveChangesAsync();
                    
                    // Vytvoření skupin
                    var skupina1 = new Skupina("Skupina 1 - Testovaci skupina");
                    var skupina2 = new Skupina("Skupina 2 - Dovolená");
                    var skupina3 = new Skupina("Skupina 3 - Byt");
                    
                    _db.Skupiny.AddRange(skupina1, skupina2, skupina3);
                    await _db.SaveChangesAsync();
                    
                    // Přidání členů do skupin
                    skupina1.PridatClena(uzivatel1, jeAdmin: true);
                    skupina1.PridatClena(uzivatel2);
                    skupina1.PridatClena(uzivatel3);
                    
                    skupina2.PridatClena(uzivatel1, jeAdmin: true);
                    skupina2.PridatClena(uzivatel2);
                    skupina2.PridatClena(uzivatel4);
                    skupina2.PridatClena(uzivatel5);
                    
                    skupina3.PridatClena(uzivatel3, jeAdmin: true);
                    skupina3.PridatClena(uzivatel4);
                    skupina3.PridatClena(uzivatel5);
                    
                    await _db.SaveChangesAsync();
                    
                    // Výdaje pro skupinu 1
                    var vydaj1_1 = new Vydaj("Nákup potravin", 900, uzivatel1.Id, skupina1.Id) {
                        Platil = uzivatel1,
                        Skupina = skupina1
                    };
                    _db.Vydaje.Add(vydaj1_1);
                    await _db.SaveChangesAsync();
                    
                    var vydaj1_2 = new Vydaj("Elektřina", 2000, uzivatel2.Id, skupina1.Id) {
                        Platil = uzivatel2,
                        Skupina = skupina1
                    };
                    _db.Vydaje.Add(vydaj1_2);
                    await _db.SaveChangesAsync();
                    
                    // Výpočet dluhů
                    var spravceUctu = new SpravceUctu(new RovnomerneRozdeleniStrategy());
                    
                    var dluhy1_1 = spravceUctu.VypocitatDluhy(skupina1, vydaj1_1);
                    foreach (var dluh in dluhy1_1) {
                        dluh.Dluznik = _db.Uzivatele.Find(dluh.DluznikId);
                        dluh.Veritel = _db.Uzivatele.Find(dluh.VeritelId);
                        vydaj1_1.Dluhy.Add(dluh);
                    }
                    _db.Dluhy.AddRange(dluhy1_1);
                    await _db.SaveChangesAsync();
                    
                    var dluhy1_2 = spravceUctu.VypocitatDluhy(skupina1, vydaj1_2);
                    foreach (var dluh in dluhy1_2) {
                        dluh.Dluznik = _db.Uzivatele.Find(dluh.DluznikId);
                        dluh.Veritel = _db.Uzivatele.Find(dluh.VeritelId);
                        vydaj1_2.Dluhy.Add(dluh);
                    }
                    _db.Dluhy.AddRange(dluhy1_2);
                    await _db.SaveChangesAsync();
                    
                    // Výdaje pro skupinu 2
                    var vydaj2_1 = new Vydaj("Ubytování", 8000, uzivatel1.Id, skupina2.Id) {
                        Platil = uzivatel1,
                        Skupina = skupina2
                    };
                    _db.Vydaje.Add(vydaj2_1);
                    await _db.SaveChangesAsync();
                    
                    var vydaj2_2 = new Vydaj("Doprava", 2400, uzivatel5.Id, skupina2.Id) {
                        Platil = uzivatel5,
                        Skupina = skupina2
                    };
                    _db.Vydaje.Add(vydaj2_2);
                    await _db.SaveChangesAsync();
                    
                    var dluhy2_1 = spravceUctu.VypocitatDluhy(skupina2, vydaj2_1);
                    foreach (var dluh in dluhy2_1) {
                        dluh.Dluznik = _db.Uzivatele.Find(dluh.DluznikId);
                        dluh.Veritel = _db.Uzivatele.Find(dluh.VeritelId);
                        vydaj2_1.Dluhy.Add(dluh);
                    }
                    _db.Dluhy.AddRange(dluhy2_1);
                    await _db.SaveChangesAsync();
                    
                    var dluhy2_2 = spravceUctu.VypocitatDluhy(skupina2, vydaj2_2);
                    foreach (var dluh in dluhy2_2) {
                        dluh.Dluznik = _db.Uzivatele.Find(dluh.DluznikId);
                        dluh.Veritel = _db.Uzivatele.Find(dluh.VeritelId);
                        vydaj2_2.Dluhy.Add(dluh);
                    }
                    _db.Dluhy.AddRange(dluhy2_2);
                    await _db.SaveChangesAsync();
                    
                    // Výdaje pro skupinu 3
                    var vydaj3_1 = new Vydaj("Nájem", 10000, uzivatel3.Id, skupina3.Id) {
                        Platil = uzivatel3,
                        Skupina = skupina3
                    };
                    _db.Vydaje.Add(vydaj3_1);
                    await _db.SaveChangesAsync();
                    
                    var vydaj3_2 = new Vydaj("Internet", 800, uzivatel4.Id, skupina3.Id) {
                        Platil = uzivatel4,
                        Skupina = skupina3
                    };
                    _db.Vydaje.Add(vydaj3_2);
                    await _db.SaveChangesAsync();
                    
                    var dluhy3_1 = spravceUctu.VypocitatDluhy(skupina3, vydaj3_1);
                    foreach (var dluh in dluhy3_1) {
                        dluh.Dluznik = _db.Uzivatele.Find(dluh.DluznikId);
                        dluh.Veritel = _db.Uzivatele.Find(dluh.VeritelId);
                        vydaj3_1.Dluhy.Add(dluh);
                    }
                    _db.Dluhy.AddRange(dluhy3_1);
                    await _db.SaveChangesAsync();
                    
                    var dluhy3_2 = spravceUctu.VypocitatDluhy(skupina3, vydaj3_2);
                    foreach (var dluh in dluhy3_2) {
                        dluh.Dluznik = _db.Uzivatele.Find(dluh.DluznikId);
                        dluh.Veritel = _db.Uzivatele.Find(dluh.VeritelId);
                        vydaj3_2.Dluhy.Add(dluh);
                    }
                    _db.Dluhy.AddRange(dluhy3_2);
                    await _db.SaveChangesAsync();
                    
                    // Označení některých dluhů jako splacených
                    if (dluhy1_2.Count > 0) {
                        var dluhKeSplaceni = dluhy1_2[0];
                        dluhKeSplaceni.OznacitJakoSplacene();
                        await _db.SaveChangesAsync();
                    }
                    
                    if (dluhy2_2.Count > 0) {
                        var dluhKeSplaceni = dluhy2_2[0];
                        dluhKeSplaceni.OznacitJakoSplacene();
                        await _db.SaveChangesAsync();
                    }
                } catch (Exception ex) {
                    throw new Exception($"Chyba při tvorbě testovacích dat: {ex.Message}", ex);
                }
            }
        }
        
        public async Task ResetDatabaseAsync() {
            try {
                var dluhy = await _db.Dluhy.ToListAsync();
                _db.Dluhy.RemoveRange(dluhy);
                await _db.SaveChangesAsync();
            
                var vydaje = await _db.Vydaje.ToListAsync();
                _db.Vydaje.RemoveRange(vydaje);
                await _db.SaveChangesAsync();
            
                var clenstvi = await _db.Clenstvi.ToListAsync();
                _db.Clenstvi.RemoveRange(clenstvi);
                await _db.SaveChangesAsync();
            
                var skupiny = await _db.Skupiny.ToListAsync();
                _db.Skupiny.RemoveRange(skupiny);
                await _db.SaveChangesAsync();
            
                var uzivatele = await _db.Uzivatele.ToListAsync();
                _db.Uzivatele.RemoveRange(uzivatele);
                await _db.SaveChangesAsync();
            
                await SeedAsync();
            } catch (Exception ex) {
                throw new Exception($"Chyba při resetu databáze: {ex.Message}", ex);
            }
        }
    }
}