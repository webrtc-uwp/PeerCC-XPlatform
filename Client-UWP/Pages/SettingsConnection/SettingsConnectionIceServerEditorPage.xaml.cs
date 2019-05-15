using Client_UWP.Models;
using Client_UWP.Pages.SettingsAccount;
using Client_UWP.Pages.SettingsDebug;
using Client_UWP.Pages.SettingsDevices;
using Client_UWP.Utilities;
using GuiCore.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public SettingsConnectionIceServerEditorPage()
        {
            InitializeComponent();

            ViewModel = new SettingsConnectionPageViewModel();

            GoToSettingsConnectionPage.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            AccountSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));
        }

        public SettingsConnectionPageViewModel ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                IceServerModel iceServer = (IceServerModel)e.Parameter;

                // Add new IceServer to IceServersList
                ViewModel.IceServersList.Add(iceServer);

                // Save IceServersList
                localSettings.Values["IceServersList"] =
                    XmlSerialization<ObservableCollection<IceServerModel>>.Serialize(ViewModel.IceServersList);

                Debug.WriteLine("Edit ice server: " + ViewModel.ListUrls[0]);

                tbServerUrl.Text = ViewModel.ListUrls[0];
                tbUsername.Text = iceServer.Username != null ? iceServer.Username : string.Empty;
                pbCredential.Password = iceServer.Credential != null ? iceServer.Credential : string.Empty;
                btnAdd.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;

                btnDelete.Click += (sender, args) =>
                {
                    // Remove IceServer from IceServersList
                    ViewModel.IceServersList.Remove(iceServer);

                    // Save IceServersList
                    localSettings.Values["IceServersList"] =
                        XmlSerialization<ObservableCollection<IceServerModel>>.Serialize(ViewModel.IceServersList);

                    Frame.Navigate(typeof(SettingsConnectionPage));
                };

                btnSave.Click += (sender, args) =>
                {
                    // Remove IceServer from IceServersList
                    ViewModel.IceServersList.Remove(iceServer);

                    // Add new IceServer to IceServersList
                    ViewModel.IceServersList.Add(new IceServerModel
                    {
                        Urls = ViewModel.ListUrls,
                        Username = tbUsername.Text,
                        Credential = pbCredential.Password
                    });

                    // Save IceServersList
                    localSettings.Values["IceServersList"] =
                        XmlSerialization<ObservableCollection<IceServerModel>>.Serialize(ViewModel.IceServersList);

                    Frame.Navigate(typeof(SettingsConnectionPage));
                };
            }

            btnAdd.Click += (sender, args) => 
            {
                // Add new IceServer to IceServersList
                ViewModel.IceServersList.Add(new IceServerModel
                {
                    Urls = ViewModel.ListUrls,
                    Username = tbUsername.Text,
                    Credential = pbCredential.Password
                });

                // Save IceServersList
                localSettings.Values["IceServersList"] =
                    XmlSerialization<ObservableCollection<IceServerModel>>.Serialize(ViewModel.IceServersList);

                Frame.Navigate(typeof(SettingsConnectionPage));
            };
        }

        

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                listIceServers.Items.Add(tbServerUrl.Text);

                ViewModel.ListUrls.Add(tbServerUrl.Text);

                tbServerUrl.Text = "";
            }
        }
    }
}
