using System.Collections.ObjectModel;
using Myst3ry.Models;
using Myst3ry.Services;

namespace Myst3ry.ViewModels
{
    /// <summary>
    /// Works out the numbers shown on the Stats screen from the saved games.
    /// </summary>
    public class StatsViewModel : ViewModelBase
    {
        private const int RecentGamesShown = 10;

        private readonly ScoreService _scoreService;

        private int _gamesPlayed;
        private int _gamesWon;
        private int _gamesLost;
        private string _winRate = "0%";
        private double _winRateProgress;
        private string _bestScore = "–";
        private string _avgGuesses = "–";
        private string _fastestWin = "–";
        private string _summary = string.Empty;
        private bool _hasGames;

        public StatsViewModel(ScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        public ObservableCollection<ScoreEntry> RecentGames { get; } = new();

        public int GamesPlayed
        {
            get => _gamesPlayed;
            private set => SetProperty(ref _gamesPlayed, value);
        }

        public int GamesWon
        {
            get => _gamesWon;
            private set => SetProperty(ref _gamesWon, value);
        }

        public int GamesLost
        {
            get => _gamesLost;
            private set => SetProperty(ref _gamesLost, value);
        }

        public string WinRate
        {
            get => _winRate;
            private set => SetProperty(ref _winRate, value);
        }

        // 0.0 to 1.0, for the progress bar.
        public double WinRateProgress
        {
            get => _winRateProgress;
            private set => SetProperty(ref _winRateProgress, value);
        }

        public string BestScore
        {
            get => _bestScore;
            private set => SetProperty(ref _bestScore, value);
        }

        public string AvgGuesses
        {
            get => _avgGuesses;
            private set => SetProperty(ref _avgGuesses, value);
        }

        public string FastestWin
        {
            get => _fastestWin;
            private set => SetProperty(ref _fastestWin, value);
        }

        public string Summary
        {
            get => _summary;
            private set => SetProperty(ref _summary, value);
        }

        public bool HasGames
        {
            get => _hasGames;
            private set => SetProperty(ref _hasGames, value);
        }

        public void Refresh()
        {
            var scores = _scoreService.GetScores();

            GamesPlayed = scores.Count;
            GamesWon = scores.Count(s => s.Won);
            GamesLost = GamesPlayed - GamesWon;
            HasGames = GamesPlayed > 0;

            WinRateProgress = GamesPlayed == 0 ? 0 : (double)GamesWon / GamesPlayed;
            WinRate = $"{WinRateProgress * 100:F0}%";

            var wonGames = scores.Where(s => s.Won).ToList();
            if (wonGames.Count > 0)
            {
                BestScore = wonGames.Min(s => s.GuessCount).ToString();
                AvgGuesses = $"{wonGames.Average(s => s.GuessCount):F1}";
                int fastest = wonGames.Min(s => s.ElapsedSeconds);
                FastestWin = TimeSpan.FromSeconds(fastest).ToString(@"mm\:ss");
            }
            else
            {
                BestScore = "–";
                AvgGuesses = "–";
                FastestWin = "–";
            }

            if (!HasGames)
                Summary = "Play a game to start tracking your progress.";
            else if (GamesPlayed >= ScoreService.MaxStoredGames)
                Summary = $"Based on your last {GamesPlayed} games.";
            else
                Summary = $"Based on {GamesPlayed} game{(GamesPlayed != 1 ? "s" : "")} played.";

            RecentGames.Clear();
            foreach (var entry in scores.Take(RecentGamesShown))
            {
                RecentGames.Add(entry);
            }
        }

        public void ClearStats()
        {
            _scoreService.ClearAll();
            Refresh();
        }
    }
}
