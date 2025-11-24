using RoommateApp.Core.Models;

namespace RoommateApp.Core.Observers {
    public class EmailNotifier : IObserver {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
            
        public EmailNotifier(string smtpServer, int port, string username, string password) {
            _smtpServer = smtpServer;
            _port = port;
            _username = username;
            _password = password;
        }
            
        public void Update(object data) {
            if (data is Dluh dluh) {
                if (dluh.JeSplaceno) {
                    OdeslatEmail(dluh.Veritel.Email, 
                        $"Dluh splacen", 
                        $"Uživatel {dluh.Dluznik.Jmeno} splatil dluh {dluh.Castka} Kč.");
                }
            }
        }
            
        private void OdeslatEmail(string adresa, string predmet, string zprava) {
            // TODO: Implementovat skutečné odesílání e-mailů
            Console.WriteLine($"Odesílám e-mail na adresu {adresa}");
            Console.WriteLine($"Předmět: {predmet}");
            Console.WriteLine($"Zpráva: {zprava}");
        }
    }
}