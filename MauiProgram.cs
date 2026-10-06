using Microsoft.Extensions.Logging;
using Myst3ry.Services;
using Myst3ry.ViewModels;
using Myst3ry.Views;

namespace Myst3ry
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Dependency injection: Shell creates the pages and passes in what their constructors ask for.
            builder.Services.AddSingleton<IPreferences>(Preferences.Default);
            builder.Services.AddSingleton<ScoreService>();

            builder.Services.AddSingleton<GameViewModel>();
            builder.Services.AddSingleton<StatsViewModel>();

            builder.Services.AddSingleton<GamePage>();
            builder.Services.AddSingleton<StatsPage>();
            builder.Services.AddTransient<HelpPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
