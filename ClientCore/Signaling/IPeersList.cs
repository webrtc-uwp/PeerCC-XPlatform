namespace ClientCore.Signaling
{
    public interface IPeersList
    {
        void AddPeerToList(IPeer peer);

        void RemovePeerFromList(int peerId);
    }
}
