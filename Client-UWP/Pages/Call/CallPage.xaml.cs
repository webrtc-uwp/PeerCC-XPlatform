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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.Call
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CallPage : Page
    {
        private WebRtcAdapter.Call.Call call;

        private HttpSignaler _signaler = HttpSignaler.Instance;

        private CallPageViewModel ViewModel { get; set; }

        public CallPage()
        {
            InitializeComponent();

            ViewModel = new CallPageViewModel();

            Loaded += OnLoaded;

            _signaler.PeerHangup += Signaler_PeerHangup;

            Hangup.Click += (sender, args) =>
            {
                _signaler.SendToPeer(new Message
                {
                    Id = "0",
                    Content = "BYE",
                    PeerId = call.PeerId.ToString()
                });

                call.PeerId = -1;

                call.ClosePeerConnection();

                Frame.Navigate(typeof(MainPage));
            };
        }

        /// <summary>
        /// See Page.OnNavigatedTo()
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            call = (WebRtcAdapter.Call.Call)e.Parameter;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryResizeView(new Size(700, 650));

            ViewModel.SelfVideo = SelfVideo;
            ViewModel.PeerVideo = PeerVideo;
        }

        private void Signaler_PeerHangup(object sender, Peer e)
        {
            Task.Run(async ()
                => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()
                => Frame.Navigate(typeof(MainPage))));
        }
    }
}
