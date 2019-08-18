using ClientCore.Signaling;
using System;

namespace PeerCC.Signaling
{
    public class HttpSignalerMessageEvent
    {
        public Peer Peer { get; set; }
        public string MessageStr { get; set; }
        public Message Message { get; set; }

        public HttpSignalerMessageEvent(Peer peer, string message)
        {
            Peer = peer;
            MessageStr = message;
        }

        public HttpSignalerMessageEvent(Message message)
        {
            Message = message;
        }
    }

    public abstract class HttpSignalerEvents
    {
        // Connection events
        public event EventHandler SignedIn;
        public event EventHandler Disconnected;
        public event EventHandler ServerConnectionFailed;

        public event EventHandler<Peer> PeerConnected;
        public event EventHandler<Peer> PeerDisconnected;
        public event EventHandler<Peer> PeerHangup;
        public event EventHandler<HttpSignalerMessageEvent> MessageFromPeer;

        //public abstract void SendToPeer(int id, string message);
        public abstract void SendToPeer(Message message);

        protected void OnSignedIn()
        {
            SignedIn?.Invoke(this, null);
        }
        protected void OnDisconnected()
        {
            Disconnected?.Invoke(this, null);
        }
        protected void OnPeerConnected(Peer peer)
        {
            PeerConnected?.Invoke(this, (peer));
        }
        protected void OnPeerDisconnected(Peer peer)
        {
            PeerDisconnected?.Invoke(this, peer);
        }
        protected void OnPeerHangup(Peer peer)
        {
            PeerHangup?.Invoke(this, peer);
        }
        public void OnMessageFromPeer(Message message)
        {
            MessageFromPeer?.Invoke(this, new HttpSignalerMessageEvent(message));
        }
        protected void OnServerConnectionFailure()
        {
            ServerConnectionFailed?.Invoke(this, null);
        }
    }
}
