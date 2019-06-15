using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientCore.Call
{
    public interface IMedia
    {
        /// <summary>
        /// Get a list of associated codecs available based on a media kind.
        /// </summary>
        /// <param name="kind">Either audio or video</param>
        /// <returns>Returns a list of available codecs.</returns>
        Task<IList<ICodec>> GetCodecsAsync(MediaKind kind);
        /// <summary>
        /// Get a list of media devices available based on a media kind.
        /// </summary>
        /// <param name="kind">Either audio input or audio output or video devices</param>
        /// <returns>Returns a list of available media devices.</returns>
        Task<IList<IMediaDevice>> GetMediaDevicesAsync(MediaKind kind);
    }
}
