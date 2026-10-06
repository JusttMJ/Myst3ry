using Myst3ry.Views;

namespace Myst3ry
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(HelpPage), typeof(HelpPage));
        }
    }
}
