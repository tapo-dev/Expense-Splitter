using RoommateApp.Core.Models;

namespace RoommateApp.Core.Strategies {
    public interface IVypocetDluhuStrategy {
        List<Dluh> VypocitatDluhy(Vydaj vydaj, List<Clenstvi> clenove);
    }
}