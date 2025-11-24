using RoommateApp.Core.Models;
using RoommateApp.Core.Strategies;

namespace RoommateApp.Core.Services {
    public class SpravceUctu {
        private IVypocetDluhuStrategy _strategy;
        
        public SpravceUctu(IVypocetDluhuStrategy strategy = null) {
            _strategy = strategy ?? new RovnomerneRozdeleniStrategy();
        }
        
        public void ZmenitStrategii(IVypocetDluhuStrategy strategy) {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }
        
        public List<Dluh> VypocitatDluhy(Skupina skupina, Vydaj vydaj) {
            if (skupina == null || vydaj == null)
                throw new ArgumentNullException("Skupina nebo výdaj je null");
                
            return _strategy.VypocitatDluhy(vydaj, skupina.Clenstvi);
        }

        /// <summary>
        /// Exportuje výdaje skupiny do CSV
        /// </summary>
        public void ExportovatVydajeDoCsv(Skupina skupina, string cesta) {
            using var writer = new StreamWriter(cesta, append: false);
            writer.WriteLine("Popis,Castka,Datum,Platil");

            foreach (var vydaj in skupina.Vydaje) {
                var radek = $"{vydaj.Popis},{vydaj.Castka},{vydaj.Datum:yyyy-MM-dd},{vydaj.Platil?.Jmeno ?? vydaj.PlatilId.ToString()}";
                writer.WriteLine(radek);
            }

            Console.WriteLine($"Výdaje byly exportovány do souboru: {cesta}");
        }

        /// <summary>
        /// Notifikuje uživatele o jejich dluzích (do konzole)
        /// </summary>
        public void NotifikovatUzivateleODluhu(Skupina skupina) {
            foreach (var clen in skupina.Clenstvi) {
                var uzivatel = clen.Uzivatel;
                var dluhy = uzivatel.Dluhy.Where(d => !d.JeSplaceno).ToList();

                if (dluhy.Any()) {
                    Console.WriteLine($"Notifikace pro {uzivatel.Email}:");
                    foreach (var dluh in dluhy) {
                        Console.WriteLine($"Dlužíš {dluh.Castka} Kč uživateli {dluh.Veritel?.Jmeno ?? dluh.VeritelId.ToString()}");
                    }
                }
            }
        }
    }
}