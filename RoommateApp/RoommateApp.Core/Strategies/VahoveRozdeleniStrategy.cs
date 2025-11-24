using RoommateApp.Core.Models;

namespace RoommateApp.Core.Strategies {
    public class VahoveRozdeleniStrategy : IVypocetDluhuStrategy {
        private readonly Dictionary<int, decimal> _vahy;
            
        public VahoveRozdeleniStrategy(Dictionary<int, decimal> vahy) {
            _vahy = vahy;
        }
            
        public List<Dluh> VypocitatDluhy(Vydaj vydaj, List<Clenstvi> clenove) {
            var dluhy = new List<Dluh>();
                
            if (clenove.Count <= 1)
                return dluhy;
                    
            // Spočítej celkovou váhu všech členů
            decimal sumVah = 0;
            foreach (var clen in clenove) {
                if (_vahy.ContainsKey(clen.UzivatelId))
                    sumVah += _vahy[clen.UzivatelId];
                else
                    sumVah += 1.0m; // Výchozí váha
            }
                
            // Vytvoř dluhy podle poměru vah
            foreach (var clen in clenove) {
                if (clen.UzivatelId == vydaj.PlatilId)
                    continue;
                        
                decimal vaha = _vahy.ContainsKey(clen.UzivatelId) ? _vahy[clen.UzivatelId] : 1.0m;
                decimal castkaDluh = vydaj.Castka * (vaha / sumVah);
                    
                var dluh = new Dluh(clen.UzivatelId, vydaj.PlatilId, castkaDluh) {
                    Dluznik = clen.Uzivatel,
                    Veritel = vydaj.Platil,
                    JeSplaceno = false
                };
                    
                dluhy.Add(dluh);
            }
                
            return dluhy;
        }
    }
}