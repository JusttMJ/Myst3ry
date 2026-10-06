using System.ComponentModel;
using Myst3ry.Models;
using Myst3ry.ViewModels;

namespace Myst3ry.Views;

/// <summary>
/// The Play screen. The layout is in GamePage.xaml and the game logic is in GameViewModel;
/// this code-behind only handles page events, confirmation dialogs and animations.
/// </summary>
public partial class GamePage : ContentPage
{
    private const string SeenHelpKey = "myst3ry_seen_help";

    private readonly GameViewModel _viewModel;

    public GamePage(GameViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.GuessRejected += OnGuessRejected;
        _viewModel.GuessScored += OnGuessScored;
        _viewModel.GameEnded += OnGameEnded;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshBest();
        _viewModel.ResumeClock();

        // Show the rules automatically the first time the app is opened.
        if (!Preferences.Default.Get(SeenHelpKey, false))
        {
            Preferences.Default.Set(SeenHelpKey, true);
            Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync(nameof(HelpPage)));
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Don't count time while the player is on another screen.
        _viewModel.PauseClock();
    }

    private async void OnHelpClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(HelpPage));
    }

    private async void OnDifficultyClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: Difficulty chosen } || chosen == _viewModel.Difficulty)
            return;

        if (_viewModel.HasProgress)
        {
            bool switchNow = await DisplayAlert("Change difficulty?",
                $"Switching to {chosen} starts a new game, so your current game will be lost.",
                "Switch", "Keep playing");
            if (!switchNow)
                return;
        }

        _viewModel.Difficulty = chosen;
    }

    private async void OnNewGameClicked(object? sender, EventArgs e)
    {
        if (_viewModel.HasProgress)
        {
            bool restart = await DisplayAlert("Start a new game?",
                "Your current game will be lost.",
                "New game", "Cancel");
            if (!restart)
                return;
        }

        _viewModel.StartNewGame();
    }

    private async void OnGiveUpClicked(object? sender, EventArgs e)
    {
        bool giveUp = await DisplayAlert("Give up?",
            "The secret code will be revealed and this game will count as a loss.",
            "Give up", "Keep playing");

        if (giveUp)
            _viewModel.GiveUp();
    }

    private async void OnGuessRejected(object? sender, EventArgs e)
    {
        SemanticScreenReader.Announce(_viewModel.StatusMessage);
        await ShakeAsync(SlotsGrid);
    }

    private async void OnGuessScored(object? sender, EventArgs e)
    {
        SemanticScreenReader.Announce(_viewModel.StatusMessage);

        // Newest guess is at the top of the list, so make sure it's visible.
        HistoryList.ScrollTo(0, position: ScrollToPosition.Start, animate: true);

        await StatusLabel.ScaleTo(1.06, 100, Easing.CubicOut);
        await StatusLabel.ScaleTo(1.0, 100, Easing.CubicIn);
    }

    private async void OnGameEnded(object? sender, EventArgs e)
    {
        SemanticScreenReader.Announce(_viewModel.StatusMessage);

        // Pop the result card in.
        ResultCard.Opacity = 0;
        ResultCard.Scale = 0.85;
        await Task.WhenAll(
            ResultCard.FadeTo(1, 250),
            ResultCard.ScaleTo(1, 450, Easing.SpringOut));
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // In Hard mode, pulse the clock every second once only a few seconds are left.
        if (e.PropertyName == nameof(GameViewModel.TimerDisplay) && _viewModel.IsTimeLow)
        {
            await TimerChip.ScaleTo(1.08, 120, Easing.CubicOut);
            await TimerChip.ScaleTo(1.0, 120, Easing.CubicIn);
        }
    }

    private static async Task ShakeAsync(VisualElement element)
    {
        const uint step = 50;
        await element.TranslateTo(-12, 0, step);
        await element.TranslateTo(12, 0, step);
        await element.TranslateTo(-8, 0, step);
        await element.TranslateTo(8, 0, step);
        await element.TranslateTo(0, 0, step);
    }
}
