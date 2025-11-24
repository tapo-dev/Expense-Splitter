using RoommateApp.Core.Observers;

namespace RoommateApp.Core.Factories {
    /// <summary>
    /// Factory pro vytváření notifikačních služeb
    /// </summary>
    public class NotifierFactory : INotifierFactory {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _emailUsername;
        private readonly string _emailPassword;
        
        public NotifierFactory(string smtpServer = "smtp.gmail.com", int smtpPort = 587, 
                              string emailUsername = "default@email.com", string emailPassword = "defaultpass") {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _emailUsername = emailUsername;
            _emailPassword = emailPassword;
        }
        
        public IObserver CreateNotifier(string type) {
            return type.ToLower() switch {
                "email" => new EmailNotifier(_smtpServer, _smtpPort, _emailUsername, _emailPassword),
                "inapp" => new InAppNotifier(),
                "sms" => new SmsNotifier(),
                "console" => new ConsoleNotifier(),
                _ => throw new ArgumentException($"Neznámý typ notifikace: {type}")
            };
        }
        
        /// <summary>
        /// Vytvoří všechny dostupné notifikační služby
        /// </summary>
        /// <returns>Seznam všech notifikačních služeb</returns>
        public List<IObserver> CreateAllNotifiers() {
            var notifiers = new List<IObserver>();
            
            foreach (var type in GetAvailableTypes()) {
                try {
                    notifiers.Add(CreateNotifier(type));
                } catch (Exception) {
                    // Pokračujeme i když se nějaký notifier nepodaří vytvořit
                }
            }
            
            return notifiers;
        }
        
        /// <summary>
        /// Vrací seznam dostupných typů notifikací
        /// </summary>
        /// <returns>Seznam dostupných typů</returns>
        public List<string> GetAvailableTypes() {
            return new List<string> { "email", "inapp", "sms", "console" };
        }
    }
}