using Client_UWP.Pages.Main;
using ClientCore.Signaling;
using PeerCC.Signaling;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Client_UWP.Pages.Call
{
    public sealed partial class CallPage : Page
    {
        private WebRtcAdapter.Call.Call _call;

        private HttpSignaler _signaler = HttpSignaler.Instance;

        private CallPageViewModel _viewModel { get; set; }

        public CallPage()
        {
            InitializeComponent();

            _viewModel = new CallPageViewModel();

            Loaded += OnLoaded;

            _signaler.PeerHangup += Signaler_PeerHangup;

            Hangup.Click += (sender, args) =>
            {
                _signaler.SendToPeer(new Message
                {
                    Id = "0",
                    Content = "BYE",
                    PeerId = _call.PeerId.ToString()
                });

                _call.PeerId = -1;

                _call.ClosePeerConnection();

                Frame.Navigate(typeof(MainPage));
            };

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// See Page.OnNavigatedTo()
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _call = (WebRtcAdapter.Call.Call)e.Parameter;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryResizeView(new Size(700, 650));

            _viewModel.SelfVideo = SelfVideo;
            _viewModel.PeerVideo = PeerVideo;
        }

        private void Signaler_PeerHangup(object sender, Peer e)
        {
            _signaler.SendToPeer(new Message
            {
                Id = "0",
                Content = "BYE",
                PeerId = _call.PeerId.ToString()
            });

            _call.PeerId = -1;

            _call.ClosePeerConnection();

            Task.Run(async ()
                => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()
                => Frame.Navigate(typeof(MainPage))));
        }
    }
}
