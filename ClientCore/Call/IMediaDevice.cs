
using System.Collections.Generic;

namespace ClientCore.Call
{
    interface IMediaDevice
    {
        /// <summary>
        /// Get the media kind for the device.
        /// </summary>
        MediaKind Kind { get; }

        /// <summary>
        /// Get the identifier for the media device.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the display name for the media device.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the video formats for a video kind. The
        /// list will be empty for audio media kind.
        /// </summary>
        IList<IMediaVideoFormat> VideoFormats { get; }
    }
}
