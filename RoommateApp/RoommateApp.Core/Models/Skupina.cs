using System.ComponentModel.DataAnnotations;

namespace RoommateApp.Core.Models {
    public class Skupina {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nazev { get; set; }
        
        public List<Clenstvi> Clenstvi { get; set; } = new();
        public List<Vydaj> Vydaje { get; set; } = new();
        
        public Skupina() {}
        
        public Skupina(string nazev) {
            Nazev = nazev;
        }
        
        /// <summary>
        /// Přidá člena do skupiny
        /// </summary>
        /// <param name="uzivatel">Uživatel k přidání</param>
        /// <param name="jeAdmin">Zda má být admin</param>
        public void PridatClena(Uzivatel uzivatel, bool jeAdmin = false) {
            if (Clenstvi.Any(c => c.UzivatelId == uzivatel.Id))
                throw new InvalidOperationException("Uživatel je už členem této skupiny.");

            Clenstvi.Add(new Clenstvi(uzivatel.Id, Id) {
                UzivatelId = uzivatel.Id,
                Uzivatel = uzivatel,
                SkupinaId = this.Id,
                Skupina = this,
                JeAdmin = jeAdmin,
                DatumPridani = DateTime.Now
            });
        }

        /// <summary>
        /// Odebere člena ze skupiny
        /// </summary>
        public void OdebratClena(Uzivatel uzivatel) {
            var clen = Clenstvi.FirstOrDefault(c => c.UzivatelId == uzivatel.Id);
            if (clen != null) {
                Clenstvi.Remove(clen);
            }
        }

        /// <summary>
        /// Změní název skupiny
        /// </summary>
        public void ZmenNazev(string novyNazev) {
            Nazev = novyNazev;
        }

        /// <summary>
        /// Vrací všechny dluhy v této skupině
        /// </summary>
        public List<Dluh> ZiskejDluhy() {
            var dluhy = new List<Dluh>();

            foreach (var vydaj in Vydaje) {
                if (vydaj.Dluhy != null && vydaj.Dluhy.Count > 0) {
                    dluhy.AddRange(vydaj.Dluhy);
                }
            }

            return dluhy;
        }
        
        /// <summary>
        /// Vrátí všechny členy skupiny
        /// </summary>
        public List<Uzivatel> ZiskejCleny() {
            return Clenstvi.Select(c => c.Uzivatel).ToList();
        }
        
        /// <summary>
        /// Kontroluje, zda je uživatel administrátorem skupiny
        /// </summary>
        public bool JeAdmin(int uzivatelId) {
            var clenstvi = Clenstvi.FirstOrDefault(c => c.UzivatelId == uzivatelId);
            return clenstvi?.JeAdmin ?? false;
        }

        /// <summary>
        /// Kontroluje, zda je uživatel členem skupiny
        /// </summary>
        public bool JeClen(int uzivatelId) {
            return Clenstvi.Any(c => c.UzivatelId == uzivatelId);
        }

        /// <summary>
        /// Vrací informace o členství daného uživatele
        /// </summary>
        public Clenstvi? ZiskejClenstvi(int uzivatelId) {
            return Clenstvi.FirstOrDefault(c => c.UzivatelId == uzivatelId);
        }

        /// <summary>
        /// Vrací statistiky o dluzích ve skupině
        /// </summary>
        public (int celkem, int splacene, int nezaplacene, decimal celkovaCastka, decimal nezaplacenaCastka) ZiskejStatistikyDluhu() {
            var dluhy = ZiskejDluhy();
            
            var celkem = dluhy.Count;
            var splacene = dluhy.Count(d => d.JeSplaceno);
            var nezaplacene = dluhy.Count(d => !d.JeSplaceno);
            var celkovaCastka = dluhy.Sum(d => d.Castka);
            var nezaplacenaCastka = dluhy.Where(d => !d.JeSplaceno).Sum(d => d.Castka);
            
            return (celkem, splacene, nezaplacene, celkovaCastka, nezaplacenaCastka);
        }

        /// <summary>
        /// Bilance uživatele ve skupině (kladná = má dostat, záporná = dluží)
        /// </summary>
        public decimal ZiskejBilanciUzivatele(int uzivatelId) {
            var dluhy = ZiskejDluhy().Where(d => !d.JeSplaceno);
            decimal bilance = 0;
            
            foreach (var dluh in dluhy) {
                if (dluh.VeritelId == uzivatelId) {
                    bilance += dluh.Castka;
                } else if (dluh.DluznikId == uzivatelId) {
                    bilance -= dluh.Castka;
                }
            }
            
            return bilance;
        }

        /// <summary>
        /// Ověří, zda může uživatel provést admin akci
        /// </summary>
        public (bool uspech, string chyba) MuzeProvestAdminAkci(int uzivatelId, string akce) {
            if (!JeClen(uzivatelId)) {
                return (false, "Nejste členem této skupiny.");
            }
            
            if (!JeAdmin(uzivatelId)) {
                return (false, $"Pouze administrátor skupiny může {akce}.");
            }
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Vrací detailní informace o členech skupiny
        /// </summary>
        public List<(Uzivatel uzivatel, bool jeAdmin, DateTime datumPridani)> ZiskejDetailyClenu() {
            return Clenstvi.Select(c => (
                uzivatel: c.Uzivatel,
                jeAdmin: c.JeAdmin,
                datumPridani: c.DatumPridani
            )).ToList();
        }

        public override string ToString() => $"Skupina {Id}: {Nazev} (Výdajů: {Vydaje?.Count ?? 0}, Členů: {Clenstvi?.Count ?? 0})";
    }
}