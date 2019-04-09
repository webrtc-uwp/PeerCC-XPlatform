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
using Windows.UI.Popups;
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
    public sealed partial class SettingsAccountPage : Page
    {
        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public SettingsAccountPage()
        {
            InitializeComponent();

            ViewModel = new SettingsAccountPageViewModel();

            InitView();
        }

        public SettingsAccountPageViewModel ViewModel { get; set; }

        private void InitView()
        {
            AccountsListView.Loaded += (sender, args) => 
            {
                AccountModel account = XmlSerialization<AccountModel>.Deserialize((string)localSettings.Values["SelectedAccount"]);

                for (int i = 0; i < AccountsListView.Items.Count; i++)
                {
                    AccountModel item = (AccountModel)AccountsListView.Items[i];

                    if (account != null)
                        if (account.AccountName == item.AccountName)
                            if (account.ServiceUri == item.ServiceUri)
                                AccountsListView.SelectedIndex = i;
                }
            };

            GoToMainPage.Click += (sender, args) => Frame.Navigate(typeof(MainPage));

            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            AccountsListView.Tapped += AccountsListView_Tapped;

            AddAccount.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountEditorPage));

            RemoveAccount.Click += async (sender, args) => 
            {
                if (AccountsListView.SelectedIndex == -1)
                {
                    await new MessageDialog("Please select Account you want to remove.").ShowAsync();
                    return;
                }

                AccountModel account = AccountsListView.SelectedItem as AccountModel;
                if (account == null) return;

                Debug.WriteLine($"Remove Account {account.AccountName}");

                // Remove Account form AccountsList
                ViewModel.AccountsList.Remove(account);

                // Save AccoutsList
                localSettings.Values["AccountsList"] =
                XmlSerialization<ObservableCollection<AccountModel>>.Serialize(ViewModel.AccountsList);
            };

            EditAccount.Click += async (sender, args) => 
            {
                if (AccountsListView.SelectedIndex == -1)
                {
                    await new MessageDialog("Please select Account you want to edit").ShowAsync();
                    return;
                }

                AccountModel account = AccountsListView.SelectedItem as AccountModel;
                if (account == null) return;

                // Remove Account from AccountsList
                ViewModel.AccountsList.Remove(account);

                // Save AccountsList
                localSettings.Values["AccountsList"] =
                    XmlSerialization<ObservableCollection<AccountModel>>.Serialize(ViewModel.AccountsList);

                Frame.Navigate(typeof(SettingsAccountEditorPage), account);
            };
        }

        private void AccountsListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AccountsListView.SelectedItem = (AccountModel)((FrameworkElement)e.OriginalSource).DataContext;

            localSettings.Values["SelectedAccount"] =
                XmlSerialization<AccountModel>.Serialize((AccountModel)AccountsListView.SelectedItem);
        }
            
    }
}
