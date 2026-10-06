namespace Myst3ry.Views;

public partial class HelpPage : ContentPage
{
    public HelpPage()
    {
        InitializeComponent();
    }

    private async void OnGotItClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}