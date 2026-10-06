using Myst3ry.ViewModels;

namespace Myst3ry.Views;

/// <summary>
/// The Stats screen: totals, win rate and the most recent games.
/// </summary>
public partial class StatsPage : ContentPage
{
    private readonly StatsViewModel _viewModel;

    public StatsPage(StatsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Reload every time the tab is opened so newly finished games show up.
        _viewModel.Refresh();
    }

    private async void OnClearClicked(object? sender, EventArgs e)
    {
        bool clear = await DisplayAlert("Clear history?",
            "This permanently deletes all saved games and statistics.",
            "Clear", "Cancel");

        if (clear)
            _viewModel.ClearStats();
    }
}
