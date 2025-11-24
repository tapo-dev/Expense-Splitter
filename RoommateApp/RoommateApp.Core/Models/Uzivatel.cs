using System.ComponentModel.DataAnnotations;
using RoommateApp.Core.Services;

namespace RoommateApp.Core.Models {
    public class Uzivatel {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Jmeno { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Heslo { get; set; }

        public List<Clenstvi> Clenstvi { get; set; } = new();
        public List<Vydaj> Vydaje { get; set; } = new();
        public List<Dluh> Dluhy { get; set; } = new();
        
        public Uzivatel() {}

        public Uzivatel(string jmeno, string email, string heslo) {
            Jmeno = jmeno;
            Email = email;
            Heslo = heslo;
        }
        
        /// <summary>
        /// Přidá výdaj uživateli
        /// </summary>
        public void PridatVydaj(Vydaj vydaj) {
            if (vydaj.PlatilId != this.Id) 
                throw new InvalidOperationException("Uživatel nesouhlasí s tím, kdo zaplatil");
            
            if (vydaj.Datum == default) 
                vydaj.Datum = DateTime.Now;
            
            Vydaje.Add(vydaj);
        }

        /// <summary>
        /// Odebere výdaj uživateli
        /// </summary>
        public void SmazatVydaj(Vydaj vydaj) {
            if (vydaj.PlatilId != this.Id) 
                throw new InvalidOperationException("Uživatel nesouhlasí s tím, kdo zaplatil");

            Vydaje.Remove(vydaj);
        }

        /// <summary>
        /// Zobrazí dluhy uživatele
        /// </summary>
        /// <param name="jenNezaplacene">Pouze nezaplacené dluhy</param>
        public List<Dluh> ZobrazDluhy(bool jenNezaplacene = false) {
            return jenNezaplacene 
                ? Dluhy.Where(d => !d.JeSplaceno).ToList()
                : Dluhy.ToList();
        }

        /// <summary>
        /// Označí dluh jako splacený s možností notifikací
        /// </summary>
        /// <param name="dluhId">ID dluhu</param>
        /// <param name="notificationService">Volitelná služba pro notifikace</param>
        /// <returns>True pokud byl dluh nalezen a označen</returns>
        public bool OznacDluhJakoSplaceny(int dluhId, NotificationService notificationService = null) {
            var dluh = Dluhy.FirstOrDefault(d => d.Id == dluhId);
            if (dluh == null) return false;

            if (notificationService != null) {
                notificationService.RegisterNotifiersToDebt(dluh);
            }

            dluh.OznacitJakoSplacene();
            return true;
        }
        
        public override string ToString() => $"{Jmeno} ({Email})";
    }
}