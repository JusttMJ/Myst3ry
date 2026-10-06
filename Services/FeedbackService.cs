using System.Diagnostics;

namespace Myst3ry.Services
{
    /// <summary>
    /// Small vibrations (haptic feedback) so key presses and results can be felt as well as seen.
    /// Devices without a vibration motor (or the Windows desktop) simply skip them.
    /// </summary>
    public static class FeedbackService
    {
        // A short tick, used for key presses.
        public static void Tap() => Perform(HapticFeedbackType.Click);

        // A longer buzz, used for a rejected guess or the end of a game.
        public static void Buzz() => Perform(HapticFeedbackType.LongPress);

        private static void Perform(HapticFeedbackType type)
        {
            try
            {
                if (HapticFeedback.Default.IsSupported)
                    HapticFeedback.Default.Perform(type);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FeedbackService] Haptics unavailable: {ex.Message}");
            }
        }
    }
}
