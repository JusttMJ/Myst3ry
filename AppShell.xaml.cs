using Myst3ry.Views;

namespace Myst3ry
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Lets pages open the help screen with Shell.Current.GoToAsync(nameof(HelpPage)).
            Routing.RegisterRoute(nameof(HelpPage), typeof(HelpPage));
        }
    }
}
