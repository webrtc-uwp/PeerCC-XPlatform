using Client_UWP.MVVM;
using GuiCore;
using Windows.UI.Xaml.Controls;

namespace Client_UWP.Pages.Call
{
    internal class CallPageViewModel : BindableBase
    {
        private MediaElement _selfVideo;

        public MediaElement SelfVideo
        {
            get { return _selfVideo; }
            set
            {
                _selfVideo = value;
                GuiLogic.Instance.SelfVideo = _selfVideo;
            }
        }

        private MediaElement _peerVideo;
        
        public MediaElement PeerVideo
        {
            get { return _peerVideo; }
            set
            {
                _peerVideo = value;
                GuiLogic.Instance.PeerVideo = _peerVideo;
            }
        }
    }
}
