using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Myst3ry.Models;
using Myst3ry.Services;
using Microsoft.Maui.Controls;

namespace Myst3ry.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly GameModel _game = new GameModel();
        private readonly ScoreService _scoreService;
        private IDispatcherTimer? _timer;

        private string _currentGuess = string.Empty;
        private int _guessCount = 0;
        private int _elapsedSeconds = 0;
        private string _timerDisplay = "00:00";
        private string _statusMessage = "Enter a 3-digit code";
        private bool _isGameOver = false;
        private bool _isWon = false;
        private Difficulty _difficulty = Difficulty.Normal;

        public GameViewModel(ScoreService scoreService)
        {
            _scoreService = scoreService;

            DigitCommand = new Command<string>(OnDigitPressed);
            BackspaceCommand = new Command(OnBackspace, () => CurrentGuess.Length > 0 && !IsGameOver);
            SubmitCommand = new Command(OnSubmit, () => CurrentGuess.Length == 3 && !IsGameOver);
            NewGameCommand = new Command(StartNewGame);
            ShowHelpCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.HelpPage)));

            StartNewGame();
        }

        public IReadOnlyList<Difficulty> Difficulties { get; } = Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>().ToList();

        public ObservableCollection<GuessResult> GuessHistory { get; } = new();

        public string CurrentGuess
        {
            get => _currentGuess;
            private set
            {
                if (SetProperty(ref _currentGuess, value))
                {
                    OnPropertyChanged(nameof(Slot1));
                    OnPropertyChanged(nameof(Slot2));
                    OnPropertyChanged(nameof(Slot3));
                    ((Command)BackspaceCommand).ChangeCanExecute();
                    ((Command)SubmitCommand).ChangeCanExecute();
                }
            }
        }

        public string Slot1 => CurrentGuess.Length > 0 ? CurrentGuess[0].ToString() : "_";
        public string Slot2 => CurrentGuess.Length > 1 ? CurrentGuess[1].ToString() : "_";
        public string Slot3 => CurrentGuess.Length > 2 ? CurrentGuess[2].ToString() : "_";

        public int GuessCount
        {
            get => _guessCount;
            private set => SetProperty(ref _guessCount, value);
        }

        public string TimerDisplay
        {
            get => _timerDisplay;
            private set => SetProperty(ref _timerDisplay, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public string SecretCodeDisplay => _game.SecretCode;

        public bool IsGameOver
        {
            get => _isGameOver;
            private set => SetProperty(ref _isGameOver, value);
        }

        public bool IsWon
        {
            get => _isWon;
            private set => SetProperty(ref _isWon, value);
        }

        public Difficulty Difficulty
        {
            get => _difficulty;
            set
            {
                if (SetProperty(ref _difficulty, value))
                    StartNewGame();
            }
        }

        public int ElapsedSeconds => _elapsedSeconds;

        public ICommand DigitCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand NewGameCommand { get; }
        public ICommand ShowHelpCommand { get; }

        public void StartNewGame()
        {
            _game.GenerateNewCode(Difficulty);
            GuessHistory.Clear();
            GuessCount = 0;
            CurrentGuess = string.Empty;
            StatusMessage = "Enter a 3-digit code";
            IsGameOver = false;
            IsWon = false;

            _elapsedSeconds = 0;
            TimerDisplay = "00:00";

            _timer?.Stop();
            StartTimer();
        }

        private void OnDigitPressed(string digit)
        {
            if (CurrentGuess.Length < 3 && !IsGameOver)
            {
                CurrentGuess += digit;
            }
        }

        private void OnBackspace()
        {
            if (CurrentGuess.Length > 0)
            {
                CurrentGuess = CurrentGuess.Substring(0, CurrentGuess.Length - 1);
            }
        }

        private void OnSubmit()
        {
            if (CurrentGuess.Length != 3 || IsGameOver) return;

            var result = _game.EvaluateGuess(CurrentGuess);
            GuessHistory.Insert(0, result);
            GuessCount++;

            if (result.IsCorrect)
            {
                IsWon = true;
                IsGameOver = true;
                _timer?.Stop();
                StatusMessage = $"🎉 Cracked in {GuessCount} guesses!";
                OnPropertyChanged(nameof(SecretCodeDisplay));

                _scoreService.SaveGame(new ScoreEntry
                {
                    GuessCount = GuessCount,
                    ElapsedSeconds = _elapsedSeconds,
                    Won = true,
                    Difficulty = Difficulty.ToString()
                });
            }
            else
            {
                StatusMessage = $"Try again! {result.Hits} hit{(result.Hits != 1 ? "s" : "")}, " +
                                $"{result.Matches} match{(result.Matches != 1 ? "es" : "")}";
            }

            CurrentGuess = string.Empty;
        }

        private void StartTimer()
        {
            _timer = Application.Current!.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (sender, e) =>
            {
                _elapsedSeconds++;
                TimerDisplay = TimeSpan.FromSeconds(_elapsedSeconds).ToString(@"mm\:ss");

                if (Difficulty == Difficulty.Hard && _elapsedSeconds >= 60 && !IsGameOver)
                {
                    EndGameWithLoss();
                }
            };
            _timer.IsRepeating = true;
            _timer.Start();
        }

        private void EndGameWithLoss()
        {
            IsGameOver = true;
            IsWon = false;
            _timer?.Stop();
            StatusMessage = $"⏰ Time's up! The code was {_game.SecretCode}";

            _scoreService.SaveGame(new ScoreEntry
            {
                GuessCount = GuessCount,
                ElapsedSeconds = _elapsedSeconds,
                Won = false,
                Difficulty = Difficulty.ToString()
            });
        }
    }
}