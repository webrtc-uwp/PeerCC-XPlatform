namespace ClientCore.Call
{
    public interface ICodec
    {
        /// <summary>
        /// Gets the media kind for the codec.
        /// </summary>
        MediaKind Kind { get; }
        /// <summary>
        /// Gets the unique identifier for the codec.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Gets the display name for the codec (to be used with settings).
        /// </summary>
        string DisplayName { get; }
        /// <summary>
        /// Gets the Hz rate for the codec.
        /// </summary>
        int Rate { get; }
    }
}
