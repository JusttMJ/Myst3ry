using Myst3ry.Services;
using Myst3ry.ViewModels;

namespace Myst3ry.Views;

    public partial class StatsPage : ContentPage
    {
        private readonly StatsViewModel _viewModel;

        public StatsPage()
        {
            InitializeComponent();
            _viewModel = new StatsViewModel(new ScoreService());
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Refresh();
        }
    }