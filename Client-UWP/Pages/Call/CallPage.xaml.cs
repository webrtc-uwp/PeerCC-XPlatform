using GuiCore;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.Call
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CallPage : Page
    {
        private CallPageViewModel ViewModel { get; set; }

        public CallPage()
        {
            InitializeComponent();

            ViewModel = new CallPageViewModel();

            Loaded += OnLoaded;

            Hangup.Click += (sender, args) =>
            {
                GuiLogic.Instance.DisconnectFromPeer();

                Frame.Navigate(typeof(MainPage));
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryResizeView(new Size(700, 700));

            ViewModel.SelfVideo = SelfVideo;
            //ViewModel.PeerVideo = PeerVideo;
        }
    }
}
