/*
 * Myst3ry - a code-breaking game built with .NET MAUI
 *
 * Group number: [GROUP NUMBER]
 *
 * Group members (surname, first name, student number):
 *   [Surname], [First name], [Student number]
 *   [Surname], [First name], [Student number]
 *   [Surname], [First name], [Student number]
 *   [Surname], [First name], [Student number]
 */

namespace Myst3ry
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell()) { Title = "Myst3ry" };
        }
    }
}
