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
using PeerCC.Signaling;
using Client_UWP.Pages.SettingsAccount;
using GuiCore;
using System.Linq;
using Client_UWP.Pages.Call;
using System.Threading.Tasks;
using Client_UWP.Models;
using GuiCore.Utilities;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Client_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private HttpSignaler _signaler = GuiLogic.Instance.HttpSignaler;

        private MainViewModel _mainViewModel;

        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        private AccountModel accountModel;

        private object _locker = new object();

        public MainPage()
        {
            InitializeComponent();

            accountModel =
                XmlSerialization<AccountModel>.Deserialize((string)localSettings.Values["SelectedAccount"]);

            GuiLogic.Instance.SetAccount(accountModel?.ServiceUri);

            //_signaler = (HttpSignaler)GuiLogic.Instance.Account.Signaler;

            Loaded += OnLoaded;

            Debug.WriteLine($"Connecting to server from local peer: {_signaler.LocalPeer.Name}");

            peersListView.SelectedIndex = -1;
            peersListView.SelectedItem = 0;

            _signaler.SignedIn += Signaler_SignedIn;
            _signaler.ServerConnectionFailed += Signaler_ServerConnectionFailed;
            _signaler.PeerConnected += Signaler_PeerConnected;
            _signaler.PeerDisconnected += Signaler_PeerDisconnected;

            GuiLogic.Instance.OnPeerConnectionCreated += async () => 
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()
                    => Frame.Navigate(typeof(CallPage)));

            GuiLogic.Instance.OnAddLocalTrack += GuiLogic.Instance.Instance_OnAddLocalTrack;

            InitView();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryResizeView(new Size(700, 650));
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
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () 
                => HandleSignedIn(sender, e));
        }

        private void HandleSignedIn(object sender, EventArgs e)
        {
            Debug.WriteLine("Peer signed in to server.");
        }

        private async void Signaler_ServerConnectionFailed(object sender, EventArgs e)
        {
            // See method Signaler_SignedIn for concurrency comments.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () 
                => HandleServerConnectionFailed(sender, e));

        }

        private void HandleServerConnectionFailed(object sender, EventArgs e)
        {
            Debug.WriteLine("Server connection failure.");
        }

        private async void Signaler_PeerConnected(object sender, Peer peer)
        {
            // See method Signaler_SignedIn for concurrency comments.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () 
                => HandlePeerConnected(sender, peer));
        }

        private void HandlePeerConnected(object sender, Peer peer)
        {
            Debug.WriteLine($"Peer connected {peer.Name} / {peer.Id}");

            foreach (object item in peersListView.Items)
            {
                Peer p = (Peer)item;

                if (p.Id == peer.Id)
                {
                    Debug.WriteLine($"Peer already found in list: {peer.ToString()}");
                    return;
                }
            }

            if (_signaler.LocalPeer.Name == peer.Name)
            {
                Debug.WriteLine($"Peer is our local peer: {peer.ToString()}");
                return;
            }

            if (GuiLogic.Instance.PeersList.Count == 0)
            {
                lock (_locker)
                {
                    GuiLogic.Instance.PeersList.Add(peer);
                }
            }

            foreach (Peer p in GuiLogic.Instance.PeersList)
            {
                if (p.Id != peer.Id)
                {
                    lock (_locker)
                    {
                        GuiLogic.Instance.PeersList.Add(peer);
                    }
                }
            }

            foreach (Peer p in GuiLogic.Instance.PeersList)
                peersListView.Items.Add(peer);
        }

        private async void Signaler_PeerDisconnected(object sender, Peer peer)
        {
            // See method Signaler_SignedIn for concurrency comments.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () 
                => HandlePeerDisconnected(sender, peer));

        }

        private void HandlePeerDisconnected(object sender, Peer peer)
        {
            Debug.WriteLine($"Peer disconnected {peer.Name} / {peer.Id}");

            for (int i = 0; i < peersListView.Items.Count(); i++)
            {
                Peer p = (Peer)peersListView.Items[i];
                if (p.Name == peer.Name)
                {
                    peersListView.Items.Remove(peersListView.Items[i]);
                }
            }

            for (int i = 0; i < GuiLogic.Instance.PeersList.Count(); i++)
            {
                Peer p = GuiLogic.Instance.PeersList[i];
                if (p.Name == peer.Name)
                {
                    GuiLogic.Instance.PeersList.Remove(p);
                }
            }
        }

        private void InitView()
        {
            if (GuiLogic.Instance.PeerConnectedToServer)
            {
                ConnectPeer.IsEnabled = false;
                DisconnectPeer.IsEnabled = true;
            }
            else
            {
                ConnectPeer.IsEnabled = true;
                DisconnectPeer.IsEnabled = false;
            }

            peersListView.Items.Clear();
            foreach (Peer peer in GuiLogic.Instance.PeersList)
            {
                peersListView.Items.Add(peer);
            }

            if (accountModel == null)
            {
                tbServiceUri.Text = "Please create/select account Service Uri.";
            }
            else
            {
                tbServiceUri.Text = $"Service Uri: { GuiLogic.Instance.Account?.ServiceUri }";
            }

            peersListView.SelectedIndex = -1;
            peersListView.SelectedItem = null;

            peersListView.Tapped += PeersListView_Tapped;

            AccountSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));

            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            ConnectPeer.Click += async (sender, args) =>
            {
                await GuiLogic.Instance.LogInToServer();

                GuiLogic.Instance.PeerConnectedToServer = true;

                ConnectPeer.IsEnabled = false;
                DisconnectPeer.IsEnabled = true;
            };

            DisconnectPeer.Click += async (sender, args) =>
            {
                await GuiLogic.Instance.LogOutFromServer();

                GuiLogic.Instance.PeerConnectedToServer = false;

                peersListView.Items.Clear();

                GuiLogic.Instance.PeersList.Clear();

                ConnectPeer.IsEnabled = true;
                DisconnectPeer.IsEnabled = false;
            };

            CallRemotePeer.Click += (sender, args) =>
            {
                if (peersListView.SelectedIndex == -1)
                {
                    new MessageDialog("Please select a peer.").ShowAsync();
                    return;
                }

                Peer remotePeer = peersListView.SelectedItem as Peer;
                if (remotePeer == null) return;

                Debug.WriteLine($"Call remote peer {remotePeer.ToString()}");

                Task.Run(async () => await GuiLogic.Instance.ConnectToPeer(remotePeer.Id));
            };
        }

        private void PeersListView_Tapped(object sender, TappedRoutedEventArgs e) =>
            peersListView.SelectedItem = (Peer)((FrameworkElement)e.OriginalSource).DataContext;
    }
}
