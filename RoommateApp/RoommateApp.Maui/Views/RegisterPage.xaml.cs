using RoommateApp.Maui.ViewModels;

namespace RoommateApp.Maui.Views {
    public partial class RegisterPage : ContentPage {
        public RegisterPage(RegisterPageViewModel viewModel) {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
        }
    }
}