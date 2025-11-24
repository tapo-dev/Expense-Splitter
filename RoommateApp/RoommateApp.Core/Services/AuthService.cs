using RoommateApp.Core.Data;
using RoommateApp.Core.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace RoommateApp.Core.Services {
    public class AuthService {
        private readonly AppDbContext _db;
        private const int BCRYPT_WORK_FACTOR = 10;

        public AuthService(AppDbContext db) {
            _db = db;
        }

        /// <summary>
        /// Přihlášení uživatele s bezpečným ověřením hesla
        /// </summary>
        public async Task<Uzivatel> PrihlasitAsync(string email, string heslo) {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(heslo))
                return null;

            try {
                var uzivatel = await _db.Uzivatele
                    .FirstOrDefaultAsync(u => u.Email == email);
                
                if (uzivatel == null)
                    return null;
                
                // Migrace starých plain text hesel
                if (!uzivatel.Heslo.StartsWith("$2")) {
                    if (uzivatel.Heslo == heslo) {
                        uzivatel.Heslo = HashHeslo(heslo);
                        await _db.SaveChangesAsync();
                        return uzivatel;
                    }
                    return null;
                }
                
                bool hesloJeSpravne = BCrypt.Net.BCrypt.Verify(heslo, uzivatel.Heslo);
                
                if (!hesloJeSpravne)
                    return null;

                return uzivatel;
            } catch (Exception) {
                return null;
            }
        }

        /// <summary>
        /// Registrace nového uživatele s hashováním hesla
        /// </summary>
        public async Task<(Uzivatel uzivatel, string chyba)> RegistrovatAsync(string jmeno, string email, string heslo) {
            if (string.IsNullOrWhiteSpace(jmeno) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(heslo))
                return (null, "Všechna pole musí být vyplněna");

            if (heslo.Length < 3)
                return (null, "Heslo musí mít alespoň 3 znaky");

            try {
                bool emailExists = await _db.Uzivatele.AnyAsync(u => u.Email == email);
                if (emailExists)
                    return (null, "Tento email je již registrován");

                string hashovaneHeslo = HashHeslo(heslo);
                var novyUzivatel = new Uzivatel(jmeno, email, hashovaneHeslo);

                _db.Uzivatele.Add(novyUzivatel);
                await _db.SaveChangesAsync();

                return (novyUzivatel, null);
            } catch (Exception ex) {
                return (null, $"Registrace selhala: {ex.Message}");
            }
        }

        /// <summary>
        /// Načte uživatele podle ID
        /// </summary>
        public async Task<Uzivatel> NacistUzivateleAsync(int userId) {
            try {
                return await _db.Uzivatele
                    .Include(u => u.Clenstvi)
                    .ThenInclude(c => c.Skupina)
                    .Include(u => u.Dluhy)
                    .FirstOrDefaultAsync(u => u.Id == userId);
            } catch (Exception) {
                return null;
            }
        }
        
        /// <summary>
        /// Aktualizuje údaje uživatele včetně bezpečného hashování hesla
        /// </summary>
        public async Task<bool> AktualizovatUzivateleAsync(Uzivatel uzivatel) {
            try {
                var existujiciUzivatel = await _db.Uzivatele.FindAsync(uzivatel.Id);
                if (existujiciUzivatel == null)
                    return false;

                existujiciUzivatel.Jmeno = uzivatel.Jmeno;
                
                if (!string.IsNullOrEmpty(uzivatel.Heslo)) {
                    existujiciUzivatel.Heslo = HashHeslo(uzivatel.Heslo);
                }

                await _db.SaveChangesAsync();
                return true;
            } catch (Exception) {
                return false;
            }
        }
        
        /// <summary>
        /// Změní heslo uživatele
        /// </summary>
        public async Task<(bool uspech, string chyba)> ZmenitHesloAsync(int uzivatelId, string stareHeslo, string noveHeslo) {
            try {
                var uzivatel = await _db.Uzivatele.FindAsync(uzivatelId);
                if (uzivatel == null)
                    return (false, "Uživatel nenalezen");
                    
                bool stareHesloJeSpravne = BCrypt.Net.BCrypt.Verify(stareHeslo, uzivatel.Heslo);
                
                // Migrace plain text hesel
                if (!stareHesloJeSpravne && !uzivatel.Heslo.StartsWith("$2") && uzivatel.Heslo == stareHeslo) {
                    stareHesloJeSpravne = true;
                }
                
                if (!stareHesloJeSpravne)
                    return (false, "Staré heslo není správné");
                    
                if (noveHeslo.Length < 3)
                    return (false, "Nové heslo musí mít alespoň 3 znaky");
                    
                uzivatel.Heslo = HashHeslo(noveHeslo);
                await _db.SaveChangesAsync();
                
                return (true, null);
            } catch (Exception) {
                return (false, "Změna hesla selhala");
            }
        }
        
        private string HashHeslo(string heslo) {
            return BCrypt.Net.BCrypt.HashPassword(heslo, BCRYPT_WORK_FACTOR);
        }
        
        /// <summary>
        /// Migruje všechna plain text hesla na hashovaná
        /// </summary>
        public async Task<int> MigrovatHeslaAsync() {
            try {
                var uzivatele = await _db.Uzivatele
                    .Where(u => !u.Heslo.StartsWith("$2"))
                    .ToListAsync();
                    
                int pocetMigrovanych = 0;
                
                foreach (var uzivatel in uzivatele) {
                    uzivatel.Heslo = HashHeslo(uzivatel.Heslo);
                    pocetMigrovanych++;
                }
                
                if (pocetMigrovanych > 0) {
                    await _db.SaveChangesAsync();
                }
                
                return pocetMigrovanych;
            } catch (Exception) {
                return 0;
            }
        }
    }
}