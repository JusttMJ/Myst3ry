using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myst3ry.Models
{
    public class GuessResult
    {
        public string Guess { get; set; } = string.Empty;
        public int Hits { get; set; }
        public int Matches { get; set; }
        public bool IsCorrect => Hits == 3;

        public string Display =>
            $"{Guess}  →  {Hits} hit{(Hits != 1 ? "s" : "")}, " +
            $"{Matches} match{(Matches != 1 ? "es" : "")}";
    }
}