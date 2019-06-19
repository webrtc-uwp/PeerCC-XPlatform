using Windows.Data.Json;

namespace ClientCore.Call
{
    public interface ICallInfo
    {
        /// <summary>
        /// Get the associated call object.
        /// </summary>
        ICall Call { get; }

        /// <summary>
        /// Get the offer or answer SDP.
        /// </summary>
        string Sdp { get; }

        /// <summary>
        /// Get the offer or answer JSON.
        /// </summary>
        JsonObject Json { get; }
    }
}
