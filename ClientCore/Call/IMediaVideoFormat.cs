
using System.Drawing;

namespace ClientCore.Call
{
    interface IMediaVideoFormat
    {
        /// <summary>
        /// Get the identifier for the specified video format.
        /// </summary>
        string Id { get; }
        Size Size { get; }
    }
}
