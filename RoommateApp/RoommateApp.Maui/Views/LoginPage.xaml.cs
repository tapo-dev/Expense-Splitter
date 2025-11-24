using RoommateApp.Maui.ViewModels;

namespace RoommateApp.Maui.Views {
    public partial class LoginPage : ContentPage {
        public LoginPage(LoginPageViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            
            if (BindingContext is LoginPageViewModel viewModel) {
                viewModel.LoadSavedCredentials();
            }
        }
    }
}