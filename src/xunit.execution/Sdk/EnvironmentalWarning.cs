namespace Xunit.Sdk
{
    /// <summary>
    /// Messages reported during test discovery that indicate that there is an issue with the
    /// test environment (for example, declaring two test collection classes with the same
    /// test collection name).
    /// </summary>
    public class EnvironmentalWarning
    {
        /// <summary>
        /// The warning message.
        /// </summary>
        public string Message { get; set; }
    }
}
