using RoommateApp.Core.Models;

namespace RoommateApp.Core.Observers {
    /// <summary>
    /// SMS notifikační služba (simulovaná)
    /// </summary>
    public class SmsNotifier : IObserver {
        private readonly string _phoneNumber;
        
        public SmsNotifier(string phoneNumber = "+420123456789") {
            _phoneNumber = phoneNumber;
        }
        
        public void Update(object data) {
            if (data is Dluh dluh) {
                if (dluh.JeSplaceno) {
                    OdeslatSms(_phoneNumber, 
                        $"Dluh splacen", 
                        $"Uživatel {dluh.Dluznik?.Jmeno ?? "Neznámý"} splatil dluh {dluh.Castka} Kč.");
                }
            }
        }
        
        private void OdeslatSms(string cislo, string predmet, string zprava) {
            // TODO: Implementovat real SMS
            Console.WriteLine($"📱 SMS na číslo {cislo}");
            Console.WriteLine($"Předmět: {predmet}");
            Console.WriteLine($"Zpráva: {zprava}");
            Console.WriteLine("--- SMS odeslána ---");
        }
    }
}