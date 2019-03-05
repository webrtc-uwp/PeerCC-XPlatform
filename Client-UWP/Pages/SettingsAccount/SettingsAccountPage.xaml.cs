using Client_UWP.Controllers;
using Client_UWP.Pages.SettingsConnection;
using Client_UWP.Pages.SettingsDebug;
using Client_UWP.Pages.SettingsDevices;
using System;
using System.Collections.Generic;
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

namespace Client_UWP.Pages.SettingsAccount
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsAccountPage : Page
    {
        public SettingsAccountPage()
        {
            InitializeComponent();

            ViewModel = new SettingsAccountPageViewModel();
            
            InitView();
        }

        public SettingsAccountPageViewModel ViewModel { get; set; }

        private void InitView()
        {
            GoToMainPage.Click += (sender, args) => Frame.Navigate(typeof(MainPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));


            AddAccount.Click += (sender, args) => { };
            RemoveAccount.Click += (sender, args) => { };
            EditAccount.Click += (sender, args) => { };
        }
    }
}
