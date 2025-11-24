using RoommateApp.Core.Models;

namespace RoommateApp.Core.Strategies {
    public class RovnomerneRozdeleniStrategy : IVypocetDluhuStrategy {
        public List<Dluh> VypocitatDluhy(Vydaj vydaj, List<Clenstvi> clenove) {
            var dluhy = new List<Dluh>();
            
            if (clenove.Count <= 1)
                return dluhy;
                
            var castkaNaOsobu = vydaj.Castka / clenove.Count;
            
            foreach (var clen in clenove) {
                if (clen.UzivatelId == vydaj.PlatilId) {
                    continue;
                }
                    
                var dluh = new Dluh(clen.UzivatelId, vydaj.PlatilId, castkaNaOsobu) {
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