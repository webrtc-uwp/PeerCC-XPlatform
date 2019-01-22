using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Pages.SettingsDebug;
using Client_UWP.Pages.SettingsDevices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class SettingsConnectionIceServerEditorPage : Page
    {
        public SettingsConnectionIceServerEditorPage()
        {
            InitializeComponent();

            ViewModel = new SettingsConnectionPageViewModel();

            cbType.Items.Add("STUN");
            cbType.Items.Add("TURN");

            GoToSettingsConnectionPage.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));
        }

        public SettingsConnectionPageViewModel ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                IceServer iceServer = (IceServer)e.Parameter;

                // Add new IceServer to IceServersList
                ViewModel.IceServersList.Add(iceServer);

                // Save IceServersList
                SettingsController.IceServersList =
                    SettingsConnectionPageViewModel.SerializedList(ViewModel.IceServersList);

                Debug.WriteLine("Edit ice server: " + iceServer.ServerDetails);

                tbServerUrl.Text = iceServer.Url;
                cbType.SelectedIndex = cbType.Items.IndexOf(iceServer.Type);
                tbPort.Text = iceServer.Port;
                tbUsername.Text = iceServer.Username != null ? iceServer.Username : string.Empty;
                pbPassword.Password = iceServer.Password != null ? iceServer.Password : string.Empty;
                btnAdd.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;

                btnDelete.Click += (sender, args) =>
                {
                    // Remove IceServer from IceServersList
                    ViewModel.IceServersList.Remove(iceServer);

                    // Save IceServersList
                    SettingsController.IceServersList = 
                        SettingsConnectionPageViewModel.SerializedList(ViewModel.IceServersList);

                    Frame.Navigate(typeof(SettingsConnectionPage));
                };

                btnSave.Click += (sender, args) =>
                {
                    // Remove IceServer from IceServersList
                    ViewModel.IceServersList.Remove(iceServer);

                    // Add new IceServer to IceServersList
                    ViewModel.IceServersList.Add(new IceServer
                    {
                        Url = tbServerUrl.Text,
                        Type = cbType.Items[cbType.SelectedIndex].ToString(),
                        Port = tbPort.Text,
                        Username = tbUsername.Text,
                        Password = pbPassword.Password
                    });

                    // Save IceServersList
                    SettingsController.IceServersList =
                        SettingsConnectionPageViewModel.SerializedList(ViewModel.IceServersList);

                    Frame.Navigate(typeof(SettingsConnectionPage));
                };
            }

            btnAdd.Click += (sender, args) => 
            {
                // Add new IceServer to IceServersList
                ViewModel.IceServersList.Add(new IceServer
                {
                    Url = tbServerUrl.Text,
                    Type = cbType.Items[cbType.SelectedIndex].ToString(),
                    Port = tbPort.Text,
                    Username = tbPort.Text,
                    Password = pbPassword.Password
                });

                // Save IceServersList
                SettingsController.IceServersList =
                    SettingsConnectionPageViewModel.SerializedList(ViewModel.IceServersList);

                Frame.Navigate(typeof(SettingsConnectionPage));
            };
        }
    }
}
