namespace Myst3ry.Models
{
    /// <summary>
    /// The result of one guess, shown as a row in the guess history.
    /// </summary>
    public class GuessResult
    {
        public int Number { get; set; }
        public string Guess { get; set; } = string.Empty;
        public int Hits { get; set; }
        public int Matches { get; set; }
        public IReadOnlyList<DigitFeedback> Digits { get; set; } = new List<DigitFeedback>();

        public bool IsCorrect => Hits == GameModel.CodeLength;
        public bool HasHits => Hits > 0;
        public bool HasMatches => Matches > 0;

        public string NumberText => $"#{Number}";
        public string HitsText => $"{Hits} hit{(Hits != 1 ? "s" : "")}";
        public string MatchesText => $"{Matches} match{(Matches != 1 ? "es" : "")}";

        // Read out by screen readers for the whole history row.
        public string Display => $"Guess {Number}: {Guess}. {HitsText}, {MatchesText}.";
    }
}
