namespace Myst3ry.Views;

/// <summary>
/// The "How to Play" screen. It is opened from the Play screen's toolbar,
/// and automatically the first time the app runs.
/// </summary>
public partial class HelpPage : ContentPage
{
    public HelpPage()
    {
        InitializeComponent();
    }

    private async void OnGotItClicked(object? sender, EventArgs e)
    {
        // Go back to the page that opened this one.
        await Shell.Current.GoToAsync("..");
    }
}
