using System.Collections.Generic;
using System.Drawing;

namespace ClientCore.Call
{
    public interface IMediaVideoFormat
    {
        /// <summary>
        /// Get the identifier for the specified video format.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Get the dimension of the video format.
        /// </summary>
        Size Dimension { get; }

        /// <summary>
        /// Get a list of frame rates supported at this resolution.
        /// </summary>
        IList<int> FrameRates { get; }
    }
}
