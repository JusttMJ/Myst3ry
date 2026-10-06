namespace Myst3ry.Models
{
    /// <summary>
    /// What we know about one digit of a guess.
    /// Neutral is used when the difficulty does not reveal per-digit clues.
    /// </summary>
    public enum DigitState
    {
        Neutral,
        Hit,
        Match,
        Miss
    }

    /// <summary>
    /// One digit tile shown in the guess history or in the revealed secret code.
    /// </summary>
    public class DigitFeedback
    {
        public DigitFeedback(string digit, DigitState state)
        {
            Digit = digit;
            State = state;
        }

        public string Digit { get; }
        public DigitState State { get; }
    }
}
