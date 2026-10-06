using System;
using System.Collections.Generic;
using System.Linq;

namespace Myst3ry.Models
{
    public class GameModel
    {
        private string _secretCode = string.Empty;
        private readonly Random _random = new Random();

        public string SecretCode => _secretCode;

        public void GenerateNewCode(Difficulty difficulty)
        {
            bool allowRepeats = difficulty == Difficulty.Easy;

            do
            {
                int a = _random.Next(0, 10);
                int b = _random.Next(0, 10);
                int c = _random.Next(0, 10);
                _secretCode = $"{a}{b}{c}";
            }
            while (!allowRepeats && HasDuplicateDigits(_secretCode));
        }

        public void SetSecretCode(string code)
        {
            _secretCode = code ?? string.Empty;
        }

        private bool HasDuplicateDigits(string code)
        {
            return code[0] == code[1] ||
                   code[1] == code[2] ||
                   code[0] == code[2];
        }

        public GuessResult EvaluateGuess(string guess)
        {
            int hits = 0, matches = 0;
            var secretUnmatched = new List<char>();
            var guessUnmatched = new List<char>();

            for (int i = 0; i < 3; i++)
            {
                if (guess[i] == _secretCode[i])
                {
                    hits++;
                }
                else
                {
                    secretUnmatched.Add(_secretCode[i]);
                    guessUnmatched.Add(guess[i]);
                }
            }

            foreach (char c in guessUnmatched)
            {
                if (secretUnmatched.Contains(c))
                {
                    matches++;
                    secretUnmatched.Remove(c); 
                }
            }

            return new GuessResult
            {
                Guess = guess,
                Hits = hits,
                Matches = matches
            };
        }
    }
}