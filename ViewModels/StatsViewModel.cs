using Myst3ry.Models;
using Myst3ry.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Myst3ry.ViewModels
{
    public class StatsViewModel : ViewModelBase
    {
        private readonly ScoreService _scoreService;

        private int _gamesPlayed;
        private int _gamesWon;
        private string _winRate = "0%";
        private string _bestScore = "-";
        private string _avgGuesses = "-";
        private string _fastestWin = "-";

        public StatsViewModel(ScoreService scoreService)
        {
            _scoreService = scoreService;
            ClearCommand = new Command(ClearStats);
        }

        public ObservableCollection<ScoreEntry> RecentGames { get; } = new();

        public ICommand ClearCommand { get; }

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

        public string WinRate
        {
            get => _winRate;
            private set => SetProperty(ref _winRate, value);
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

        public void Refresh()
        {
            var scores = _scoreService.GetScores();
            Debug.WriteLine($"[StatsViewModel] {scores.Count} scores loaded.");

            GamesPlayed = scores.Count;
            GamesWon = scores.Count(s => s.Won);
            WinRate = GamesPlayed == 0 ? "0%" : $"{(100.0 * GamesWon / GamesPlayed):F1}%";

            var wonGames = scores.Where(s => s.Won).ToList();
            if (wonGames.Count > 0)
            {
                BestScore = $"{wonGames.Min(s => s.GuessCount)} guesses";
                AvgGuesses = $"{wonGames.Average(s => s.GuessCount):F1}";
                int fastest = wonGames.Min(s => s.ElapsedSeconds);
                FastestWin = TimeSpan.FromSeconds(fastest).ToString(@"mm\:ss");
            }
            else
            {
                BestScore = "-";
                AvgGuesses = "-";
                FastestWin = "-";
            }

            RecentGames.Clear();
            foreach (var entry in scores.Take(10))
            {
                RecentGames.Add(entry);
                Debug.WriteLine($"[StatsViewModel] Added: {entry.Display}");
            }
        }

        private void ClearStats()
        {
            _scoreService.ClearAll();
            Refresh();
        }
    }
}