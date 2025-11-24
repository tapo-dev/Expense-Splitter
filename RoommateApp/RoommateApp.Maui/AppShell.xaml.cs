using RoommateApp.Maui.Views;

namespace RoommateApp.Maui;

public partial class AppShell : Shell {
    public AppShell() {
        InitializeComponent();
        Routing.RegisterRoute("groups", typeof(GroupsPage));
        Routing.RegisterRoute("profile", typeof(ProfilePage));
        Routing.RegisterRoute("addexpense", typeof(AddExpensePage));
    }
}