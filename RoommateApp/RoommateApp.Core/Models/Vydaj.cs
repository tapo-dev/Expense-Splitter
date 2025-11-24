using System.ComponentModel.DataAnnotations;

namespace RoommateApp.Core.Models {
    public class Vydaj {
        public int Id { get; set; }

        [Required]
        public string Popis { get; set; }

        private decimal _castka;
        
        [Required]
        public decimal Castka { 
            get => _castka; 
            set => _castka = Math.Round(value, 2); 
        }

        public DateTime Datum { get; set; }

        [Required]
        public int PlatilId { get; set; }
        public Uzivatel Platil { get; set; }

        [Required]
        public int SkupinaId { get; set; }
        public Skupina Skupina { get; set; }

        public List<Dluh> Dluhy { get; set; } = new();
        
        public Vydaj() {}
        
        public Vydaj(string popis, decimal castka, int platilId, int skupinaId)
            : this(popis, castka, null, platilId, skupinaId) { }

        public Vydaj(string popis, decimal castka, DateTime? datum, int platilId, int skupinaId) {
            Popis = popis;
            Castka = castka;
            Datum = datum ?? DateTime.Now;
            PlatilId = platilId;
            SkupinaId = skupinaId;
        }

        /// <summary>
        /// Starší metoda pro výpočet dluhů - použijte SpravceUctu.VypocitatDluhy()
        /// </summary>
        [Obsolete("Použít metodu VypocitatDluhy ze SpravceUctu")]
        public List<Dluh> VypocitatDluh(List<Clenstvi> clenove) {
            var dluhy = new List<Dluh>();

            if (clenove.Count <= 1)
                return dluhy;

            var castkaNaOsobu = Castka / clenove.Count;

            foreach (var clen in clenove) {
                if (clen.UzivatelId == PlatilId)
                    continue;

                var dluh = new Dluh(clen.UzivatelId, PlatilId, castkaNaOsobu) {
                    Veritel = Platil ?? clen.Uzivatel,
                    Dluznik = clen.Uzivatel,
                    JeSplaceno = false
                };

                dluhy.Add(dluh);
            }

            return dluhy;
        }

        public override string ToString() => 
            $"Vydaj {Id}: {Popis}, {Castka} Kč, zaplatil {Platil?.Jmeno ?? $"ID:{PlatilId}"}, {Datum:d}";
    }
}