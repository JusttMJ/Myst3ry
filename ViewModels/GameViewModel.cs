using System.Collections.ObjectModel;
using System.Windows.Input;
using Myst3ry.Models;
using Myst3ry.Services;

namespace Myst3ry.ViewModels
{
    /// <summary>
    /// State and actions for the Play screen. GamePage.xaml binds to the properties and commands here.
    /// </summary>
    public class GameViewModel : ViewModelBase
    {
        public const int HardTimeLimitSeconds = 60;
        private const int LowTimeWarningSeconds = 10;

        private readonly GameModel _game = new GameModel();
        private readonly ScoreService _scoreService;
        private readonly IDispatcherTimer _timer;

        private string _currentGuess = string.Empty;
        private int _guessCount = 0;
        private int _elapsedSeconds = 0;
        private bool _clockStarted = false;
        private string _timerDisplay = "00:00";
        private bool _isTimeLow = false;
        private string _statusMessage = string.Empty;
        private bool _isGameOver = false;
        private GameOutcome _outcome = GameOutcome.InProgress;
        private bool _isNewBest = false;
        private string _bestMessage = string.Empty;
        private string _bestDisplay = "–";
        private Difficulty _difficulty = Difficulty.Normal;
        private IReadOnlyList<DigitFeedback> _secretDigits = new List<DigitFeedback>();

        public GameViewModel(ScoreService scoreService)
        {
            _scoreService = scoreService;

            // The game clock: a dispatcher timer that ticks once a second on the UI thread.
            _timer = Application.Current!.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.IsRepeating = true;
            _timer.Tick += OnTimerTick;

            DigitCommand = new Command<string>(OnDigitPressed, CanPressDigit);
            BackspaceCommand = new Command(OnBackspace, () => CurrentGuess.Length > 0 && !IsGameOver);
            SubmitCommand = new Command(OnSubmit, () => CurrentGuess.Length == GameModel.CodeLength && !IsGameOver);
            NewGameCommand = new Command(StartNewGame);

            StartNewGame();
        }

        // Raised so the page can play animations. The view model itself never touches the UI.
        public event EventHandler? GuessRejected;
        public event EventHandler? GuessScored;
        public event EventHandler? GameEnded;

        public ObservableCollection<GuessResult> GuessHistory { get; } = new();

        public ICommand DigitCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand NewGameCommand { get; }

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
                    OnPropertyChanged(nameof(ActiveSlot));
                    RefreshCommands();
                }
            }
        }

        public string Slot1 => CurrentGuess.Length > 0 ? CurrentGuess[0].ToString() : string.Empty;
        public string Slot2 => CurrentGuess.Length > 1 ? CurrentGuess[1].ToString() : string.Empty;
        public string Slot3 => CurrentGuess.Length > 2 ? CurrentGuess[2].ToString() : string.Empty;

        // The slot the next digit will go into (highlighted on screen).
        public int ActiveSlot => IsGameOver ? -1 : CurrentGuess.Length;

        public int GuessCount
        {
            get => _guessCount;
            private set
            {
                if (SetProperty(ref _guessCount, value))
                    OnPropertyChanged(nameof(GuessesCaption));
            }
        }

        public string GuessesCaption => GuessCount == 1 ? "GUESS" : "GUESSES";

        public string TimerDisplay
        {
            get => _timerDisplay;
            private set => SetProperty(ref _timerDisplay, value);
        }

        public string TimerCaption => IsTimed ? "TIME LEFT" : "TIME";

        public bool IsTimed => Difficulty == Difficulty.Hard;

        public bool IsTimeLow
        {
            get => _isTimeLow;
            private set => SetProperty(ref _isTimeLow, value);
        }

        public string ElapsedDisplay => FormatTime(_elapsedSeconds);

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            private set
            {
                if (SetProperty(ref _isGameOver, value))
                {
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(ActiveSlot));
                    RefreshCommands();
                }
            }
        }

        public bool IsPlaying => !IsGameOver;

        // True once the player has made at least one guess in a game that is still running.
        public bool HasProgress => !IsGameOver && GuessCount > 0;

        public bool IsWon => _outcome == GameOutcome.Won;

        public string ResultTitle => _outcome switch
        {
            GameOutcome.Won => "🎉 Code cracked!",
            GameOutcome.TimedOut => "⏰ Time's up!",
            GameOutcome.GaveUp => "🏳️ Game over",
            _ => string.Empty
        };

        public string ResultMessage => _outcome switch
        {
            GameOutcome.Won => $"You found the code in {GuessCount} {GuessesCaption.ToLower()}.",
            GameOutcome.TimedOut => $"The {HardTimeLimitSeconds} seconds ran out. Better luck next time!",
            GameOutcome.GaveUp => "You gave up, but there's always another code to crack.",
            _ => string.Empty
        };

        public bool IsNewBest
        {
            get => _isNewBest;
            private set => SetProperty(ref _isNewBest, value);
        }

        public string BestMessage
        {
            get => _bestMessage;
            private set => SetProperty(ref _bestMessage, value);
        }

        // Fewest guesses ever needed on the current difficulty.
        public string BestDisplay
        {
            get => _bestDisplay;
            private set => SetProperty(ref _bestDisplay, value);
        }

        // The secret code, shown as tiles once the game is over.
        public IReadOnlyList<DigitFeedback> SecretDigits
        {
            get => _secretDigits;
            private set => SetProperty(ref _secretDigits, value);
        }

        public Difficulty Difficulty
        {
            get => _difficulty;
            set
            {
                if (SetProperty(ref _difficulty, value))
                {
                    OnPropertyChanged(nameof(IsTimed));
                    OnPropertyChanged(nameof(TimerCaption));
                    OnPropertyChanged(nameof(DifficultyDescription));
                    StartNewGame();
                }
            }
        }

        public string DifficultyDescription => Difficulty switch
        {
            Difficulty.Easy => "Coloured tiles show which digits are hits and matches.",
            Difficulty.Hard => $"Only hit and match counts, and just {HardTimeLimitSeconds} seconds!",
            _ => "Only the number of hits and matches is shown."
        };

        public void StartNewGame()
        {
            _timer.Stop();
            _clockStarted = false;
            _elapsedSeconds = 0;

            _game.GenerateNewCode();
            GuessHistory.Clear();
            GuessCount = 0;
            CurrentGuess = string.Empty;
            SecretDigits = new List<DigitFeedback>();
            IsNewBest = false;
            BestMessage = string.Empty;
            SetOutcome(GameOutcome.InProgress);
            IsGameOver = false;

            UpdateTimerDisplay();
            RefreshBest();
            StatusMessage = "Tap 3 different digits, then ✓. The clock starts on your first tap.";
            RefreshCommands();
        }

        // Called by the page when the player leaves or returns to the Play screen.
        public void PauseClock() => _timer.Stop();

        public void ResumeClock()
        {
            if (_clockStarted && !IsGameOver)
                _timer.Start();
        }

        public void GiveUp()
        {
            if (!IsGameOver)
                EndGame(GameOutcome.GaveUp);
        }

        private bool CanPressDigit(string? digit)
        {
            if (IsGameOver || string.IsNullOrEmpty(digit) || CurrentGuess.Length >= GameModel.CodeLength)
                return false;

            // The secret code never repeats a digit, so a guess shouldn't either.
            if (CurrentGuess.Contains(digit))
                return false;

            // A three-digit number can't start with 0.
            if (digit == "0" && CurrentGuess.Length == 0)
                return false;

            return true;
        }

        private void OnDigitPressed(string digit)
        {
            if (!CanPressDigit(digit))
                return;

            if (!_clockStarted)
            {
                _clockStarted = true;
                _timer.Start();
            }

            CurrentGuess += digit;
            FeedbackService.Tap();
        }

        private void OnBackspace()
        {
            if (CurrentGuess.Length > 0)
            {
                CurrentGuess = CurrentGuess.Substring(0, CurrentGuess.Length - 1);
                FeedbackService.Tap();
            }
        }

        private void OnSubmit()
        {
            if (CurrentGuess.Length != GameModel.CodeLength || IsGameOver) return;

            string guess = CurrentGuess;
            CurrentGuess = string.Empty;

            if (GuessHistory.Any(g => g.Guess == guess))
            {
                StatusMessage = $"You already tried {guess}. Try a different code.";
                FeedbackService.Buzz();
                GuessRejected?.Invoke(this, EventArgs.Empty);
                return;
            }

            var result = _game.EvaluateGuess(guess, revealDigitStates: Difficulty == Difficulty.Easy);
            GuessCount++;
            result.Number = GuessCount;
            GuessHistory.Insert(0, result);

            if (result.IsCorrect)
            {
                EndGame(GameOutcome.Won);
                return;
            }

            if (result.Hits + result.Matches == 0)
                StatusMessage = $"{guess}: no hits or matches, so none of those digits are in the code.";
            else if (result.Hits + result.Matches == GameModel.CodeLength)
                StatusMessage = $"{guess}: all three digits are right. Now find the right order!";
            else
                StatusMessage = $"{guess}: {result.HitsText}, {result.MatchesText}. Keep going!";

            FeedbackService.Tap();
            GuessScored?.Invoke(this, EventArgs.Empty);
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _elapsedSeconds++;
            UpdateTimerDisplay();

            if (IsTimed && _elapsedSeconds >= HardTimeLimitSeconds && !IsGameOver)
            {
                EndGame(GameOutcome.TimedOut);
            }
        }

        private void EndGame(GameOutcome outcome)
        {
            _timer.Stop();

            if (outcome == GameOutcome.Won)
            {
                // Compare with earlier wins before this one is saved.
                int? previousBest = _scoreService.GetBestGuessCount(Difficulty);
                IsNewBest = previousBest == null || GuessCount < previousBest;
                BestMessage = previousBest == null
                    ? $"🏆 Your first win on {Difficulty}!"
                    : "🏆 New personal best!";
            }

            _scoreService.SaveGame(new ScoreEntry
            {
                GuessCount = GuessCount,
                ElapsedSeconds = _elapsedSeconds,
                Won = outcome == GameOutcome.Won,
                GaveUp = outcome == GameOutcome.GaveUp,
                Difficulty = Difficulty.ToString()
            });

            var revealState = outcome == GameOutcome.Won ? DigitState.Hit : DigitState.Neutral;
            SecretDigits = _game.SecretCode
                .Select(c => new DigitFeedback(c.ToString(), revealState))
                .ToList();

            SetOutcome(outcome);
            IsGameOver = true;
            UpdateTimerDisplay();
            RefreshBest();
            StatusMessage = $"{ResultTitle} The code was {_game.SecretCode}.";

            FeedbackService.Buzz();
            GameEnded?.Invoke(this, EventArgs.Empty);
        }

        private void SetOutcome(GameOutcome outcome)
        {
            _outcome = outcome;
            OnPropertyChanged(nameof(IsWon));
            OnPropertyChanged(nameof(ResultTitle));
            OnPropertyChanged(nameof(ResultMessage));
            OnPropertyChanged(nameof(ElapsedDisplay));
        }

        private void UpdateTimerDisplay()
        {
            int seconds = IsTimed ? Math.Max(0, HardTimeLimitSeconds - _elapsedSeconds) : _elapsedSeconds;
            TimerDisplay = FormatTime(seconds);
            IsTimeLow = IsTimed && _clockStarted && !IsGameOver && seconds <= LowTimeWarningSeconds;
        }

        // Also called by the page when it appears, in case the stats were cleared meanwhile.
        public void RefreshBest()
        {
            int? best = _scoreService.GetBestGuessCount(Difficulty);
            BestDisplay = best?.ToString() ?? "–";
        }

        private void RefreshCommands()
        {
            ((Command)DigitCommand).ChangeCanExecute();
            ((Command)BackspaceCommand).ChangeCanExecute();
            ((Command)SubmitCommand).ChangeCanExecute();
        }

        private static string FormatTime(int seconds) => TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
    }
}
