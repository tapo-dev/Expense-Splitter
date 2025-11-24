using RoommateApp.Core.Factories;
using RoommateApp.Core.Models;
using RoommateApp.Core.Observers;

namespace RoommateApp.Core.Services {
    /// <summary>
    /// Služba pro správu notifikací používající Factory Pattern
    /// </summary>
    public class NotificationService {
        private readonly INotifierFactory _notifierFactory;
        private readonly List<IObserver> _activeNotifiers;
        
        public NotificationService(INotifierFactory notifierFactory) {
            _notifierFactory = notifierFactory;
            _activeNotifiers = new List<IObserver>();
        }
        
        /// <summary>
        /// Aktivuje notifikační službu podle typu
        /// </summary>
        public void ActivateNotifier(string type) {
            try {
                var notifier = _notifierFactory.CreateNotifier(type);
                if (!_activeNotifiers.Contains(notifier)) {
                    _activeNotifiers.Add(notifier);
                }
            } catch (Exception) {
                // Pokračujeme i když se nepodaří aktivovat notifier
            }
        }
        
        /// <summary>
        /// Aktivuje všechny dostupné notifikační služby
        /// </summary>
        public void ActivateAllNotifiers() {
            var allNotifiers = _notifierFactory.CreateAllNotifiers();
            _activeNotifiers.Clear();
            _activeNotifiers.AddRange(allNotifiers);
        }
        
        /// <summary>
        /// Zaregistruje všechny aktivní notifikace k dluhu
        /// </summary>
        public void RegisterNotifiersToDebt(Dluh dluh) {
            foreach (var notifier in _activeNotifiers) {
                dluh.RegisterObserver(notifier);
            }
        }
        
        /// <summary>
        /// Vrací seznam aktivních notifikačních služeb
        /// </summary>
        public List<string> GetActiveNotifierTypes() {
            return _activeNotifiers.Select(n => n.GetType().Name).ToList();
        }
        
        /// <summary>
        /// Vrací seznam všech dostupných typů notifikací
        /// </summary>
        public List<string> GetAvailableNotifierTypes() {
            return _notifierFactory.GetAvailableTypes();
        }
        
        /// <summary>
        /// Deaktivuje všechny notifikace
        /// </summary>
        public void DeactivateAllNotifiers() {
            _activeNotifiers.Clear();
        }
    }
}