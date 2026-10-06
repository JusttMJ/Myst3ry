namespace Myst3ry.Models
{
    /// <summary>
    /// The game rules: creates the secret code and scores each guess.
    /// It contains no user-interface code.
    /// </summary>
    public class GameModel
    {
        public const int CodeLength = 3;

        private string _secretCode = string.Empty;
        private readonly Random _random = new Random();

        public string SecretCode => _secretCode;

        /// <summary>
        /// Picks a random three-digit number (102 to 987) in which every digit is different.
        /// </summary>
        public void GenerateNewCode()
        {
            // The first digit can't be 0, otherwise it would not be a three-digit number.
            var digits = new List<int> { _random.Next(1, 10) };

            while (digits.Count < CodeLength)
            {
                int digit = _random.Next(0, 10);
                if (!digits.Contains(digit))
                    digits.Add(digit);
            }

            _secretCode = string.Concat(digits);
        }

        public void SetSecretCode(string code)
        {
            _secretCode = code ?? string.Empty;
        }

        /// <summary>
        /// Compares a guess with the secret code.
        /// A hit is a correct digit in the correct place; a match is a correct digit in the wrong place.
        /// </summary>
        /// <param name="revealDigitStates">True (Easy mode) to also say which digit is a hit, match or miss.</param>
        public GuessResult EvaluateGuess(string guess, bool revealDigitStates)
        {
            int hits = 0, matches = 0;
            var states = new DigitState[CodeLength];
            var secretUnmatched = new List<char>();

            // First pass: hits (right digit, right place).
            for (int i = 0; i < CodeLength; i++)
            {
                if (guess[i] == _secretCode[i])
                {
                    hits++;
                    states[i] = DigitState.Hit;
                }
                else
                {
                    secretUnmatched.Add(_secretCode[i]);
                }
            }

            // Second pass: matches (right digit, wrong place). Each secret digit is only counted once.
            for (int i = 0; i < CodeLength; i++)
            {
                if (states[i] == DigitState.Hit)
                    continue;

                if (secretUnmatched.Remove(guess[i]))
                {
                    matches++;
                    states[i] = DigitState.Match;
                }
                else
                {
                    states[i] = DigitState.Miss;
                }
            }

            var digits = new List<DigitFeedback>();
            for (int i = 0; i < CodeLength; i++)
            {
                digits.Add(new DigitFeedback(guess[i].ToString(),
                                             revealDigitStates ? states[i] : DigitState.Neutral));
            }

            return new GuessResult
            {
                Guess = guess,
                Hits = hits,
                Matches = matches,
                Digits = digits
            };
        }
    }
}
