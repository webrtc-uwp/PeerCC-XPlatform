namespace ClientCore.Signaling
{
    public interface IPeersList
    {
        void AddPeerToList(Peer peer);

        void RemovePeerFromList(int peerId);
    }
}
