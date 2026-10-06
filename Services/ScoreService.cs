using System.Diagnostics;
using System.Text.Json;
using Myst3ry.Models;

namespace Myst3ry.Services
{
    /// <summary>
    /// Saves finished games on the device (Preferences + JSON) and reads them back.
    /// </summary>
    public class ScoreService
    {
        private const string ScoresKey = "myst3ry_scores";

        private readonly IPreferences _preferences;

        // Keeps the saved JSON small enough for every platform's preference storage.
        public const int MaxStoredGames = 50;

        public ScoreService(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public List<ScoreEntry> GetScores()
        {
            if (!_preferences.ContainsKey(ScoresKey))
            {
                return new List<ScoreEntry>();
            }

            string json = _preferences.Get(ScoresKey, "[]");

            try
            {
                return JsonSerializer.Deserialize<List<ScoreEntry>>(json)
                       ?? new List<ScoreEntry>();
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"[ScoreService] Deserialize error: {ex.Message}");
                return new List<ScoreEntry>();
            }
        }

        public void SaveGame(ScoreEntry entry)
        {
            var scores = GetScores();
            scores.Insert(0, entry);
            if (scores.Count > MaxStoredGames)
                scores = scores.GetRange(0, MaxStoredGames);

            string json = JsonSerializer.Serialize(scores);
            _preferences.Set(ScoresKey, json);
            Debug.WriteLine($"[ScoreService] Saved game. {scores.Count} games stored.");
        }

        /// <summary>
        /// The fewest guesses needed to win on a difficulty, or null if the player has not won on it yet.
        /// </summary>
        public int? GetBestGuessCount(Difficulty difficulty)
        {
            var wins = GetScores()
                .Where(s => s.Won && s.Difficulty == difficulty.ToString())
                .ToList();

            return wins.Count == 0 ? null : wins.Min(s => s.GuessCount);
        }

        public void ClearAll() => _preferences.Remove(ScoresKey);
    }
}
