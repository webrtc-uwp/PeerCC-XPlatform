using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
using System.Linq;
using Client_UWP.Pages.Call;
using System.Threading.Tasks;
using Client_UWP.Models;
using ClientCore.Call;
using WebRtcAdapter.Call;
using ClientCore.Signaling;
using ClientCore.Account;
using PeerCC.Account;

namespace Client_UWP.Pages.Main
{
    public sealed partial class MainPage : Page
    {
        private HttpSignaler _signaler = HttpSignaler.Instance;
        private MainViewModel _mainViewModel;
        private LocalSettings _localSettings = new LocalSettings();
        private AccountModel _accountModel;

        private WebRtcAdapter.Call.Call _call;
        private Account _account;

        private Account SetAccount(string serviceUri)
        {
            IAccountProvider accountFactory =
                ClientCore.Factory.SignalingFactory.Singleton.CreateIAccountProvider();

            AccountProvider accountProvider = (AccountProvider)accountFactory;

            return (Account)accountProvider.GetAccount(serviceUri, _signaler.LocalPeer.Name, _signaler);
        }

        private WebRtcAdapter.Call.Call SetCall()
        {
            ICallProvider callFactory =
                ClientCore.Factory.CallFactory.Singleton.CreateICallProvider();

            CallProvider callProvider = (CallProvider)callFactory;

            return (WebRtcAdapter.Call.Call)callProvider.GetCall();
        }

        public MainPage()
        {
            InitializeComponent();

            DefaultSettings.AddDefaultAccount();

            _call = SetCall();

            DefaultSettings.AddDefaultIceServersList(_call);

            InitEvents();

            InitView();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _mainViewModel = (MainViewModel)e.Parameter;
            DataContext = _mainViewModel;

            _accountModel = _localSettings.DeserializeSelectedAccount();
            _account = SetAccount(_accountModel?.ServiceUri);

            tbServiceUri.Text =
                _accountModel == null ? "Please create/select account Service Uri." : $"Service Uri: { _account?.ServiceUri }";
        }

        private void InitEvents()
        {
            _call.OnSendMessageToRemotePeer += (sender, args) => _signaler.SendToPeer(new Message
            {
                Id = "0",
                Content = args,
                PeerId = _call.PeerId.ToString()
            });

            _call.OnSignedOut += (sender, args) => Task.Run(async () => await _signaler.SignOut());

            _call.OnPeerConnectionCreated += async () =>
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()
                    => Frame.Navigate(typeof(CallPage), _call));

            _signaler.SignedIn += (sender, args) => Debug.WriteLine("Peer signed in to server.");
            _signaler.ServerConnectionFailed += (sender, args) => Debug.WriteLine("Server connection failure.");
            _signaler.PeerConnected += Signaler_PeerConnected;
            _signaler.PeerDisconnected += Signaler_PeerDisconnected;
            _signaler.MessageFromPeer += (sender, args) 
                => _call.MessageFromPeerTaskRun(int.Parse(args.Message.PeerId), args.Message.Content);
        }

        private async void Signaler_PeerConnected(object sender, Peer peer)
        {
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
            peersListView.Items.Add(peer);
        }

        private async void Signaler_PeerDisconnected(object sender, Peer peer)
        {
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
                    peersListView.Items.Remove(peersListView.Items[i]);
            }
        }

        private void InitView()
        {
            PrepareView();

            foreach (Peer peer in _signaler._peers) peersListView.Items.Add(peer);

            AccountSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));
            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));
            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));
            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            ConnectPeer.Click += async (sender, args) =>
            {
                await _signaler.Connect(_account.ServiceUri);

                ConnectPeer.IsEnabled = false;
                DisconnectPeer.IsEnabled = true;
            };
            DisconnectPeer.Click += async (sender, args) =>
            {
                await _signaler.SignOut();

                peersListView.Items.Clear();

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

                Task.Run(async () =>
                {
                    _call.PeerId = remotePeer.Id;
                    CallInfo callInfo = (CallInfo)await _call.PlaceCallAsync(null);
                });
            };
        }

        private void PrepareView()
        {
            NavigationCacheMode = NavigationCacheMode.Required;

            if (_signaler.LocalPeerSignedIn)
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
            peersListView.SelectedIndex = -1;
            peersListView.SelectedItem = null;

            Loaded += (sender, args) => ApplicationView.GetForCurrentView().TryResizeView(new Size(700, 650));

            peersListView.Tapped += (sender, args)
                => peersListView.SelectedItem = (Peer)((FrameworkElement)args.OriginalSource).DataContext;
        }
    }
}
