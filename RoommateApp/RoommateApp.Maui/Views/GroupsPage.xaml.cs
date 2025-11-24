using RoommateApp.Maui.ViewModels;

namespace RoommateApp.Maui.Views {
    [QueryProperty(nameof(SkupinaId), "skupinaId")]
    public partial class GroupsPage : ContentPage {
        private readonly GroupsPageViewModel _viewModel;
        
        private string _skupinaId;
        public string SkupinaId {
            get => _skupinaId;
            set {
                _skupinaId = value;
                if (_viewModel != null && int.TryParse(value, out int id)) {
                    Task.Run(async () => await _viewModel.LoadSkupinaByIdAsync(id));
                }
            }
        }

        public GroupsPage(GroupsPageViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
    
            if (!string.IsNullOrEmpty(SkupinaId) && int.TryParse(SkupinaId, out int id)) {
                await _viewModel.LoadSkupinaByIdAsync(id);
            } else {
                await _viewModel.LoadDataAsync();
            }
        }
    }
}