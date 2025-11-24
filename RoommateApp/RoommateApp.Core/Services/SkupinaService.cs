using Microsoft.EntityFrameworkCore;
using RoommateApp.Core.Data;
using RoommateApp.Core.Models;

namespace RoommateApp.Core.Services {
    public class SkupinaService {
        private readonly AppDbContext _db;
        
        public SkupinaService(AppDbContext db) {
            _db = db;
        }
        
        /// <summary>
        /// Bezpečně odstraní skupinu s kontrolou oprávnění
        /// </summary>
        public async Task<(bool uspech, string chyba)> OdstranitSkupinuAsync(int skupinaId, int uzivatelId) {
            try {
                var skupina = await _db.Skupiny
                    .Include(s => s.Clenstvi)
                    .Include(s => s.Vydaje)
                    .ThenInclude(v => v.Dluhy)
                    .FirstOrDefaultAsync(s => s.Id == skupinaId);
                    
                if (skupina == null) {
                    return (false, "Skupina nebyla nalezena.");
                }
                
                var (muze, chybaOpravneni) = skupina.MuzeProvestAdminAkci(uzivatelId, "odstranit skupinu");
                if (!muze) {
                    return (false, chybaOpravneni);
                }
                
                // Odstranění v správném pořadí kvůli foreign key constraints
                var vsechnyDluhy = skupina.ZiskejDluhy();
                if (vsechnyDluhy.Any()) {
                    _db.Dluhy.RemoveRange(vsechnyDluhy);
                }
                
                if (skupina.Vydaje.Any()) {
                    _db.Vydaje.RemoveRange(skupina.Vydaje);
                }
                
                if (skupina.Clenstvi.Any()) {
                    _db.Clenstvi.RemoveRange(skupina.Clenstvi);
                }
                
                _db.Skupiny.Remove(skupina);
                await _db.SaveChangesAsync();
                
                return (true, string.Empty);
            } catch (Exception ex) {
                return (false, $"Chyba při mazání skupiny: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Upraví název skupiny s kontrolou oprávnění
        /// </summary>
        public async Task<(bool uspech, string chyba)> UpravitNazevSkupinyAsync(int skupinaId, int uzivatelId, string novyNazev) {
            try {
                if (string.IsNullOrWhiteSpace(novyNazev)) {
                    return (false, "Název skupiny nemůže být prázdný.");
                }
                
                var skupina = await _db.Skupiny
                    .Include(s => s.Clenstvi)
                    .FirstOrDefaultAsync(s => s.Id == skupinaId);
                    
                if (skupina == null) {
                    return (false, "Skupina nebyla nalezena.");
                }
                
                var (muze, chybaOpravneni) = skupina.MuzeProvestAdminAkci(uzivatelId, "upravit název skupiny");
                if (!muze) {
                    return (false, chybaOpravneni);
                }
                
                skupina.ZmenNazev(novyNazev);
                await _db.SaveChangesAsync();
                
                return (true, string.Empty);
            } catch (Exception ex) {
                return (false, $"Chyba při úpravě názvu: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Vrací detailní informace o skupině
        /// </summary>
        public async Task<Skupina?> ZiskejDetailSkupinyAsync(int skupinaId) {
            return await _db.Skupiny
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
                .FirstOrDefaultAsync(s => s.Id == skupinaId);
        }
    }
}