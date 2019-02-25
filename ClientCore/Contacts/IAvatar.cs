using System.Drawing;

namespace ClientCore.Contacts
{
    public interface IAvatar
    {
        /// <summary>
        /// Gets an unique identifier for this avatar.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Gets the dimensions of this avatar (if known).
        /// </summary>
        Size? Dimension { get; }
        /// <summary>
        /// Gets a URL of where to download the avatar image.
        /// </summary>
        string Uri { get; }
    }
}
