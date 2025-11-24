using RoommateApp.Core.Models;

namespace RoommateApp.Core.Observers {
    public class InAppNotifier : IObserver {
        private readonly List<string> _notifikace = new List<string>();
            
        public void Update(object data) {
            if (data is Dluh dluh) {
                if (dluh.JeSplaceno) {
                    _notifikace.Add($"Dluh ve výši {dluh.Castka} Kč byl označen jako splacený.");
                }
            }
        }
            
        /// <summary>
        /// Vrací seznam všech notifikací
        /// </summary>
        public List<string> GetNotifikace() {
            return _notifikace;
        }
    }
}