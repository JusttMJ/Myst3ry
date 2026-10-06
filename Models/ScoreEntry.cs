using System.Text.Json.Serialization;

namespace Myst3ry.Models
{
    /// <summary>
    /// One finished game, saved on the device so the Stats page can show it.
    /// </summary>
    public class ScoreEntry
    {
        public int GuessCount { get; set; }
        public int ElapsedSeconds { get; set; }
        public bool Won { get; set; }
        public bool GaveUp { get; set; }
        public string Difficulty { get; set; } = "Normal";
        public DateTime DatePlayed { get; set; } = DateTime.Now;

        // The properties below are only for display, so they are not saved.

        [JsonIgnore]
        public string ResultIcon => Won ? "✅" : GaveUp ? "🏳️" : "⏰";

        [JsonIgnore]
        public string Title => Won
            ? $"Cracked in {GuessCount} guess{(GuessCount != 1 ? "es" : "")}"
            : GaveUp ? "Gave up" : "Ran out of time";

        [JsonIgnore]
        public string Subtitle => $"{Difficulty} • {FormatTime(ElapsedSeconds)} • {DatePlayed:d MMM, HH:mm}";

        [JsonIgnore]
        public string Display => $"{Title}. {Subtitle}";

        private static string FormatTime(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return ts.ToString(@"mm\:ss");
        }
    }
}
