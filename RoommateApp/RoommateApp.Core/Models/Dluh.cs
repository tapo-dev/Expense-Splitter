using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RoommateApp.Core.Observers;

namespace RoommateApp.Core.Models {
    public class Dluh : ISubject {
        [Key]
        public int Id { get; set; }

        private decimal _castka;
        [Required]
        public decimal Castka { 
            get => _castka; 
            set => _castka = Math.Round(value, 2); 
        }

        [Required]
        public int DluznikId { get; set; }
        public Uzivatel Dluznik { get; set; }

        [Required]
        public int VeritelId { get; set; }
        public Uzivatel Veritel { get; set; }

        public bool JeSplaceno { get; set; }

        [NotMapped]
        private List<IObserver> _observers = new List<IObserver>();

        public Dluh() {}
        
        public Dluh(int dluznikId, int veritelId, decimal castka) {
            DluznikId = dluznikId;
            VeritelId = veritelId;
            Castka = castka;
            JeSplaceno = false;
        }
        
        /// <summary>
        /// Označí dluh jako splacený a upozorní observery
        /// </summary>
        public void OznacitJakoSplacene() {
            JeSplaceno = true;
            NotifyObservers();
        }
        
        public void RegisterObserver(IObserver observer) {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }
        
        public void RemoveObserver(IObserver observer) {
            _observers.Remove(observer);
        }
        
        public void NotifyObservers() {
            foreach (var observer in _observers) {
                observer.Update(this);
            }
        }
        
        public override string ToString() =>
            $"Dluh {Id}: {Dluznik?.Jmeno} dluží {Veritel?.Jmeno} {Castka} Kč (Splaceno: {JeSplaceno})";
    }
}