﻿using System;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Client_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly HttpSignaler _signaler;

        private MainViewModel _mainViewModel;

        public MainPage()
        {
            InitializeComponent();

            _signaler = GuiLogic.Instance._httpSignaler;

            Loaded += OnLoaded;

            string name = _signaler.LocalPeer.Name;
            Debug.WriteLine($"Connecting to server from local peer: {name}");

            peersListView.SelectedIndex = -1;
            peersListView.SelectedItem = 0;

            _signaler.SignedIn += Signaler_SignedIn;
            _signaler.ServerConnectionFailed += Signaler_ServerConnectionFailed;
            _signaler.PeerConnected += Signaler_PeerConnected;
            _signaler.PeerDisconnected += Signaler_PeerDisconnected;
            _signaler.MessageFromPeer += Signaler_MessageFromPeer;

            GuiLogic.Instance.OnPeerConnectionCreated += () => 
            {
                Debug.WriteLine("MainPage: PeerConnection created!");
            };

            GuiLogic.Instance.OnAddRemoteTrack += GuiLogic.Instance.Instance_OnAddRemoteTrack;
            GuiLogic.Instance.OnAddLocalTrack += GuiLogic.Instance.Instance_OnAddLocalTrack;

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
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandlePeerDisconnected(sender, peer));

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
            tbServiceUri.Text = $"Service Uri: { GuiLogic.Instance.account?.ServiceUri }";
            tbIdentityUri.Text = $"Self Identity Uri: { GuiLogic.Instance.account?.SelfIdentityUri }";

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

                ConnectPeer.IsEnabled = false;
                DisconnectPeer.IsEnabled = true;
            };

            DisconnectPeer.Click += async (sender, args) =>
            {
                await GuiLogic.Instance.LogOutFromServer();

                peersListView.Items.Clear();

                DisconnectPeer.IsEnabled = false;
                ConnectPeer.IsEnabled = true;
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

                Frame.Navigate(typeof(CallPage));
            };
        }

        private void Signaler_MessageFromPeer(object sender, HttpSignalerMessageEvent e)
        {
            GuiLogic.Instance.MessageFromPeerTaskRun(e.Message);
        }

        private void PeersListView_Tapped(object sender, TappedRoutedEventArgs e) =>
            peersListView.SelectedItem = (Peer)((FrameworkElement)e.OriginalSource).DataContext;
    }
}
