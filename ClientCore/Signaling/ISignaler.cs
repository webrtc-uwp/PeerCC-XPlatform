using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientCore.Signaling
{
    public interface ISignaler
    {
        /// <summary>
        /// Send a message to a peer.
        /// </summary>
        /// <param name="PeerId"></param>
        /// <param name="payload"></param>
        /// <returns>Returns when message is delivered or an
        /// exception if the message could not be delivered.</returns>
        Task SentToPeerAsync(Message message);

        /// <summary>
        /// Wait for messages to arrive from peers. This method will not
        /// return until there is at least one message available.
        /// </summary>
        /// <returns>Returns a list messages sent from peers or
        /// returns an exception the ability to receive messages
        /// failed.</returns>
        Task<IList<Message>> WaitForMessagesAsync();
    }
}
