using Myst3ry.Services;
using Myst3ry.ViewModels;

namespace Myst3ry.Views;

public partial class GamePage : ContentPage
{
    public GamePage()
    {
        InitializeComponent();
        BindingContext = new GameViewModel(new ScoreService());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}