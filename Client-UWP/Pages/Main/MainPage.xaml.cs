using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Core;
using Client_UWP.Pages.SettingsConnection;
using Client_UWP.Pages.SettingsDevices;
using Client_UWP.Pages.SettingsDebug;
using ClientCore.Signaling;
using PeerCC.Signaling;
using PeerCC.Account;
using Client_UWP.Pages.SettingsAccount;
using WebRtcAdapter.Call;
using Client_UWP.Utilities;
using Windows.Storage;
using Client_UWP.Models;
using WebRtcAdapter;
using GuiCore;
using GuiCore.Utilities;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Client_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //HttpSignaler _httpSignaler;

        private MainViewModel _mainViewModel;

        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            string name = RtcController.Instance._httpSignaler.LocalPeer.Name;
            Debug.WriteLine($"Connecting to server from local peer: {name}");

            RtcController.Instance._httpSignaler.SignedIn += Signaler_SignedIn;
            RtcController.Instance._httpSignaler.ServerConnectionFailed += Signaler_ServerConnectionFailed;
            RtcController.Instance._httpSignaler.PeerConnected += Signaler_PeerConnected;
            RtcController.Instance._httpSignaler.PeerDisconnected += Signaler_PeerDisconnected;
            RtcController.Instance._httpSignaler.MessageFromPeer += Signaler_MessageFromPeer;

            InitView();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryResizeView(new Size(450, 700));
        }

        /// <summary>
        /// See Page.OnNavigatedTo()
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _mainViewModel = (MainViewModel)e.Parameter;
            DataContext = _mainViewModel;
        }

        private async void Signaler_SignedIn(object sender, EventArgs e)
        {
            // The signaler will notify all events from the signaler
            // task thread. To prevent concurrency issues, ensure all
            // notifications from this thread are asynchronously
            // forwarded back to the GUI thread for further processing.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleSignedIn(sender, e));
        }

        private void HandleSignedIn(object sender, EventArgs e)
        {
            Debug.WriteLine("Peer signed in to server.");
        }

        private async void Signaler_ServerConnectionFailed(object sender, EventArgs e)
        {
            // See method Signaler_SignedIn for concurrency comments.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleServerConnectionFailed(sender, e));

        }

        private void HandleServerConnectionFailed(object sender, EventArgs e)
        {
            Debug.WriteLine("Server connection failure.");
        }

        private async void Signaler_PeerConnected(object sender, Peer peer)
        {
            // See method Signaler_SignedIn for concurrency comments.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandlePeerConnected(sender, peer));
        }

        private void HandlePeerConnected(object sender, Peer peer)
        {
            Debug.WriteLine($"Peer connected {peer.Name} / {peer.Id}");

            if (peersListView.Items.Contains(peer))
            {
                Debug.WriteLine($"Peer already found in list: {peer.ToString()}");
                return;
            }

            if (RtcController.Instance._httpSignaler.LocalPeer.Name == peer.Name)
            {
                Debug.WriteLine($"Peer is our local peer: {peer.ToString()}");
                return;
            }
        }

        private async void Signaler_PeerDisconnected(object sender, Peer peer)
        {
            // See method Signaler_SignedIn for concurrency comments.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandlePeerDisconnected(sender, peer));

        }

        private void HandlePeerDisconnected(object sender, Peer peer)
        {
            Debug.WriteLine($"Peer disconnected {peer.Name} / {peer.Id}");
        }

        private void InitView()
        {
            tbServiceUri.Text = $"Service Uri: { RtcController.Instance.account?.ServiceUri }";
            tbIdentityUri.Text = $"Self Identity Uri: { RtcController.Instance.account?.SelfIdentityUri }";

            peersListView.ItemsSource = HttpSignaler.Instance._peers;

            peersListView.SelectedIndex = -1;
            peersListView.SelectedItem = null;

            peersListView.Tapped += PeersListView_Tapped;

            AccountSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));

            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            ConnectPeer.Click += async (sender, args) =>
            {
                Debug.WriteLine("Connects to server.");

                AccountModel account = XmlSerialization<AccountModel>.Deserialize((string)localSettings.Values["SelectedAccount"]);

                await RtcController.Instance._httpSignaler.Connect(account.ServiceUri);

                ConnectPeer.IsEnabled = false;
                DisconnectPeer.IsEnabled = true;
            };

            DisconnectPeer.Click += async (sender, args) =>
            {
                Debug.WriteLine("Disconnects from server.");

                await RtcController.Instance._httpSignaler.SignOut();

                HttpSignaler.Instance._peers.Clear();

                DisconnectPeer.IsEnabled = false;
                ConnectPeer.IsEnabled = true;
            };

            CallRemotePeer.Click += async (sender, args) =>
            {
                if (peersListView.SelectedIndex == -1)
                {
                    await new MessageDialog("Please select a peer.").ShowAsync();
                    return;
                }

                Peer remotePeer = peersListView.SelectedItem as Peer;
                if (remotePeer == null) return;

                Debug.WriteLine($"Call remote peer {remotePeer.ToString()}");

                var message = new Message();

                string content = await RtcController.Instance.ConnectToPeer(remotePeer.Id);

                message.Id = "1";
                message.PeerId = remotePeer.Id.ToString();
                message.Content = content;

                await RtcController.Instance._httpSignaler.SentToPeerAsync(message);
            };
        }

        private void Signaler_MessageFromPeer(object sender, HttpSignalerMessageEvent e)
        {
            Debug.WriteLine("Message from peer MainPage!");
            Debug.WriteLine("Peer id: " + e.Message.PeerId);
            Debug.WriteLine("Message id: " + e.Message.Id);
            Debug.WriteLine("Message content: " + e.Message.Content);

            int peerId = int.Parse(e.Message.PeerId);
            string message = e.Message.Content;

            RtcController.Instance.MessageFromPeerTaskRun(peerId, message);
        }

        private void PeersListView_Tapped(object sender, TappedRoutedEventArgs e) =>
            peersListView.SelectedItem = (Peer)((FrameworkElement)e.OriginalSource).DataContext;
    }
}
