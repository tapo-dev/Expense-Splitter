using RoommateApp.Core.Models;

namespace RoommateApp.Core.Observers {
    /// <summary>
    /// Konzolová notifikační služba pro debug účely
    /// </summary>
    public class ConsoleNotifier : IObserver {
        private readonly string _prefix;
        
        public ConsoleNotifier(string prefix = "[CONSOLE]") {
            _prefix = prefix;
        }
        
        public void Update(object data) {
            if (data is Dluh dluh) {
                if (dluh.JeSplaceno) {
                    Console.WriteLine($"{_prefix} ===================");
                    Console.WriteLine($"{_prefix} DLUH SPLACEN!");
                    Console.WriteLine($"{_prefix} Dlužník: {dluh.Dluznik?.Jmeno ?? "Neznámý"}");
                    Console.WriteLine($"{_prefix} Věřitel: {dluh.Veritel?.Jmeno ?? "Neznámý"}");
                    Console.WriteLine($"{_prefix} Částka: {dluh.Castka} Kč");
                    Console.WriteLine($"{_prefix} Čas: {DateTime.Now:HH:mm:ss}");
                    Console.WriteLine($"{_prefix} ===================");
                }
            }
        }
    }
}