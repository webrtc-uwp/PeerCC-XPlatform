using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Pages.SettingsConnection;
using Client_UWP.Pages.SettingsDebug;
using Client_UWP.Pages.SettingsDevices;
using Client_UWP.Utilities;
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

namespace Client_UWP.Pages.SettingsAccount
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsAccountEditorPage : Page
    {
        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public SettingsAccountEditorPage()
        {
            InitializeComponent();

            ViewModel = new SettingsAccountPageViewModel();

            GoToSettingsAccountPage.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));

            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));
        }

        public SettingsAccountPageViewModel ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Account account = (Account)e.Parameter;

            if (e.Parameter != null)
            {
                // Add new Account to AccountsList
                ViewModel.AccountsList.Add(account);

                // Save AccountsList
                localSettings.Values["AccountsList"] =
                    XmlSerialization<ObservableCollection<Account>>.Serialize(ViewModel.AccountsList);

                Debug.WriteLine("Edit Account: " + account.AccountName);

                tbAccountName.Text = account.AccountName;
                tbServiceUri.Text = account.ServiceUri;
                tbIdentityUri.Text = account.IdentityUri;
                btnAdd.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;

                btnDelete.Click += (sender, args) =>
                {
                    // Remove Account from AccountsList
                    ViewModel.AccountsList.Remove(account);

                    // Save AccountsList
                    localSettings.Values["AccountsList"] =
                        XmlSerialization<ObservableCollection<Account>>.Serialize(ViewModel.AccountsList);

                    Frame.Navigate(typeof(SettingsAccountPage));
                };

                btnSave.Click += (sender, args) =>
                {
                    // Remove Account from AccountsList
                    ViewModel.AccountsList.Remove(account);

                    // Add Account to AccountsList
                    ViewModel.AccountsList.Add(new Account
                    {
                        AccountName = tbAccountName.Text,
                        ServiceUri = tbServiceUri.Text,
                        IdentityUri = tbIdentityUri.Text
                    });

                    // Save AccountsList
                    localSettings.Values["AccountsList"] =
                        XmlSerialization<ObservableCollection<Account>>.Serialize(ViewModel.AccountsList);

                    Frame.Navigate(typeof(SettingsAccountPage));
                };
            }

            btnAdd.Click += (sender, args) =>
            {
                // Add new Account to AccountsList
                ViewModel.AccountsList.Add(new Account
                {
                    AccountName = tbAccountName.Text,
                    ServiceUri = tbServiceUri.Text,
                    IdentityUri = tbIdentityUri.Text
                });

                // Save AccountsList
                localSettings.Values["AccountsList"] =
                    XmlSerialization<ObservableCollection<Account>>.Serialize(ViewModel.AccountsList);

                Frame.Navigate(typeof(SettingsAccountPage));
            };
        }
    }
}
