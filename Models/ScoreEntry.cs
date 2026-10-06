using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myst3ry.Models
{
    public class ScoreEntry
    {
        public int GuessCount { get; set; }
        public int ElapsedSeconds { get; set; }
        public bool Won { get; set; }
        public string Difficulty { get; set; } = "Normal";
        public DateTime DatePlayed { get; set; } = DateTime.Now;

        public string Display =>
                    $"{(Won ? "✅" : "❌")} {GuessCount} guesses • {FormatTime(ElapsedSeconds)}";

        private static string FormatTime(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return ts.ToString(@"mm\:ss");
        }
    }
}