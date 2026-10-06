namespace Myst3ry.Models
{
    /// <summary>
    /// Difficulty levels. The secret code always has three different digits;
    /// the levels only change how much help the player gets and whether there is a time limit.
    /// </summary>
    public enum Difficulty
    {
        Easy,   // coloured tiles show which digit is a hit, a match or a miss
        Normal, // only the number of hits and matches is shown
        Hard    // like Normal, but the player only has 60 seconds
    }
}
