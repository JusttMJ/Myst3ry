using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using Myst3ry.Models;

namespace Myst3ry.Services
{
    public class ScoreService
    {
        private const string ScoresKey = "myst3ry_scores";

        public List<ScoreEntry> GetScores()
        {
            if (!Preferences.ContainsKey(ScoresKey))
            {
                Debug.WriteLine("[ScoreService] No saved scores found.");
                return new List<ScoreEntry>();
            }

            string json = Preferences.Get(ScoresKey, "[]");
            Debug.WriteLine($"[ScoreService] Retrieved JSON: {json}");

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
            if (scores.Count > 20)
                scores = scores.GetRange(0, 20);

            string json = JsonSerializer.Serialize(scores);
            Preferences.Set(ScoresKey, json);
            Debug.WriteLine($"[ScoreService] Saved JSON: {json}");
        }

        public void ClearAll() => Preferences.Remove(ScoresKey);
    }
}