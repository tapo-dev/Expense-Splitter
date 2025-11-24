using RoommateApp.Core.Observers;

namespace RoommateApp.Core.Factories {
    /// <summary>
    /// Factory pro vytváření notifikačních služeb
    /// </summary>
    public interface INotifierFactory {
        /// <summary>
        /// Vytvoří notifikační službu podle typu
        /// </summary>
        /// <param name="type">Typ notifikace ("email", "inapp", "sms", "console")</param>
        /// <returns>Instance notifikační služby</returns>
        IObserver CreateNotifier(string type);
        
        /// <summary>
        /// Vytvoří všechny dostupné notifikační služby
        /// </summary>
        /// <returns>Seznam všech notifikačních služeb</returns>
        List<IObserver> CreateAllNotifiers();
        
        /// <summary>
        /// Vrací seznam dostupných typů notifikací
        /// </summary>
        /// <returns>Seznam dostupných typů</returns>
        List<string> GetAvailableTypes();
    }
}