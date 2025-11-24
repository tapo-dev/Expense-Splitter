using System.ComponentModel.DataAnnotations;

namespace RoommateApp.Core.Models {
    public class Clenstvi {
        public int UzivatelId { get; set; }
        public int SkupinaId { get; set; }
        public bool JeAdmin { get; set; } = false;
        public DateTime DatumPridani { get; set; }

        public Uzivatel Uzivatel { get; set; }
        public Skupina Skupina { get; set; }

        public Clenstvi() { }

        public Clenstvi(int uzivatelId, int skupinaId, bool jeAdmin = false) {
            UzivatelId = uzivatelId;
            SkupinaId = skupinaId;
            JeAdmin = jeAdmin;
            DatumPridani = DateTime.UtcNow;
        }

        public override string ToString() =>
            $"{Uzivatel?.Jmeno} ve skupině {Skupina?.Nazev} (Admin: {JeAdmin})";
    }
}