using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Pages.SettingsDebug;
using Client_UWP.Pages.SettingsDevices;
using Client_UWP.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.SettingsConnection
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsConnectionPage : Page
    {
        public SettingsConnectionPage()
        {
            InitializeComponent();

            DataContext = server;

            ViewModel = new SettingsConnectionPageViewModel();

            InitView();
        }

        private void IceServersListView_Tapped(object sender, TappedRoutedEventArgs e) =>
            IceServersListView.SelectedItem = (IceServer)((FrameworkElement)e.OriginalSource).DataContext;

        public SettingsConnectionPageViewModel ViewModel { get; set; }

        Server server = new Server
        {
            IP = DefaultSettings.IP,
            Port = DefaultSettings.Port
        };

        private void InitView()
        {
            GoToMainPage.Click += (sender, args) => Frame.Navigate(typeof(MainPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            AddServer.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionIceServerEditorPage));

            EditServer.Click += async (sender, args) => 
            {
                if (IceServersListView.SelectedIndex == -1)
                {
                    await new MessageDialog("Please select Ice Server you want to edit.").ShowAsync();
                    return;
                }

                IceServer iceServer = IceServersListView.SelectedItem as IceServer;
                if (iceServer == null) return;

                // Remove IceServer from IceServersList
                ViewModel.IceServersList.Remove(iceServer);

                // Save IceServersList
                SettingsController.IceServersList =
                    SettingsConnectionPageViewModel.SerializedList(ViewModel.IceServersList);

                Frame.Navigate(typeof(SettingsConnectionIceServerEditorPage), iceServer);
            };

            IceServersListView.Tapped += IceServersListView_Tapped;

            RemoveServer.Click += async (sender, args) =>
            {
                if (IceServersListView.SelectedIndex == -1)
                {
                    await new MessageDialog("Please select Ice Server you want to remove.").ShowAsync();
                    return;
                }

                IceServer iceServer = IceServersListView.SelectedItem as IceServer;
                if (iceServer == null) return;

                Debug.WriteLine($"Remove Ice Server {iceServer.ServerDetails}");

                // Remove IceServer from IceServersList
                ViewModel.IceServersList.Remove(iceServer);

                // Save IceServersList
                SettingsController.IceServersList =
                    SettingsConnectionPageViewModel.SerializedList(ViewModel.IceServersList);
            };
        }
    }
}
