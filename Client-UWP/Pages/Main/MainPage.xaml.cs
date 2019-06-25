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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ClientCore.Call;
using WebRtcAdapter.Call;
using ClientCore.Signaling;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Client_UWP.Pages.Main
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private HttpSignaler _signaler = GuiLogic.Instance.HttpSignaler;

        private MainViewModel _mainViewModel;

        private LocalSettings _localSettings = new LocalSettings();

        private AccountModel accountModel;

        private WebRtcAdapter.Call.Call Call;

        public MainPage()
        {
            AddDefaultAccount();

            InitializeComponent();

            AddDefaultIceServersList();

            accountModel = _localSettings.DeserializeSelectedAccount();

            GuiLogic.Instance.SetAccount(accountModel?.ServiceUri);

            //_signaler = (HttpSignaler)GuiLogic.Instance.Account.Signaler;

            ICallProvider callFactory =
                ClientCore.Factory.CallFactory.Singleton.CreateICallProvider();

            CallProvider callProvider = (CallProvider)callFactory;

            Call = (WebRtcAdapter.Call.Call)callProvider.GetCallAsync();

            Loaded += OnLoaded;

            Debug.WriteLine($"Connecting to server from local peer: {_signaler.LocalPeer.Name}");

            peersListView.SelectedIndex = -1;
            peersListView.SelectedItem = 0;

            _signaler.SignedIn += Signaler_SignedIn;
            _signaler.ServerConnectionFailed += Signaler_ServerConnectionFailed;
            _signaler.PeerConnected += Signaler_PeerConnected;
            _signaler.PeerDisconnected += Signaler_PeerDisconnected;
            _signaler.MessageFromPeer += HttpSignaler_MessageFromPeer;

            InitView();
        }

        private void HttpSignaler_MessageFromPeer(object sender, HttpSignalerMessageEvent e)
        {
            int peerId = int.Parse(e.Message.PeerId);
            string content = e.Message.Content;

            Call.MessageFromPeerTaskRun(peerId, content);
        }

        

        private void AddDefaultAccount()
        {
            if (_localSettings.DeserializeAccountsList() == null
                || !(_localSettings.DeserializeAccountsList()).Any())
            {
                ObservableCollection<AccountModel> accountsList = new ObservableCollection<AccountModel>();

                AccountModel accountModel = new AccountModel();
                accountModel.AccountName = "Default Account";
                accountModel.ServiceUri = "http://40.83.179.150:8888";

                accountsList.Add(accountModel);

                _localSettings.SerializeSelectedAccount(accountModel);
                _localSettings.SerializeAccountsList(accountsList);
            }
        }

        private void AddDefaultIceServersList()
        {
            if (_localSettings.DeserializeIceServersList() == null
                || !(_localSettings.DeserializeIceServersList()).Any())
            {
                ObservableCollection<IceServerModel> iceServersList = new ObservableCollection<IceServerModel>();

                foreach (IceServer iceServer in DefaultSettings.AddDefaultIceServers)
                {
                    IceServerModel iceServerModel = new IceServerModel();
                    iceServerModel.Urls = iceServer.Urls;
                    iceServerModel.Username = iceServer.Username;
                    iceServerModel.Credential = iceServer.Credential;

                    iceServersList.Add(iceServerModel);
                }

                _localSettings.SerializeIceServersList(iceServersList);
            }
            else
            {
                List<IceServer> iceServersList = new List<IceServer>();

                ObservableCollection<IceServerModel> list = _localSettings.DeserializeIceServersList();

                foreach (var ice in list)
                {
                    IceServer iceServer = new IceServer();
                    iceServer.Urls = ice.Urls;
                    iceServer.Username = ice.Username;
                    iceServer.Credential = ice.Credential;

                    iceServersList.Add(iceServer);
                }
                GuiLogic.Instance.AddIceServers(iceServersList);
                GuiLogic.Instance.SetIceServers(iceServersList);
            }
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

            foreach (Peer peer in _signaler._peers)
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

                //Task.Run(async () => await GuiLogic.Instance.ConnectToPeer(remotePeer.Id));

                _peerId = remotePeer.Id;

                


                Task.Run(async () => 
                {
                    

                    Call.OnSendMessageToRemotePeer += Call_OnSendMessageToRemotePeer;

                    Call.OnPeerConnectionCreated += async () =>
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()
                            => Frame.Navigate(typeof(CallPage)));

                    CallInfo callInfo = (CallInfo)await Call.PlaceCallAsync(null);
                });
            };
        }

        private int _peerId;

        private void Call_OnSendMessageToRemotePeer(object sender, string e)
        {
            _signaler.SendToPeer(new Message
            {
                Id = "0",
                Content = e,
                PeerId = _peerId.ToString()
            });
        }

        private void PeersListView_Tapped(object sender, TappedRoutedEventArgs e) =>
            peersListView.SelectedItem = (Peer)((FrameworkElement)e.OriginalSource).DataContext;
    }
}
