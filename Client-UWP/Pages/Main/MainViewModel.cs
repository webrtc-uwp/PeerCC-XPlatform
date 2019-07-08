using GuiCore;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace Client_UWP.Pages.Main
{
    public delegate void InitializedDelegate();
    internal class MainViewModel : DispatcherBindableBase
    {
        public event InitializedDelegate OnInitialized;

        public MainViewModel(CoreDispatcher uiDispatcher)
            : base(uiDispatcher)
        {
            Initialization.Initialized += WebRtcInitialized;

            Devices.Instance.RequestAccessForMediaCapture().AsTask().ContinueWith(antecedent =>
            {
                if (antecedent.Result)
                    Initialization.CofigureWebRtcLib(uiDispatcher);
                else
                    RunOnUiThread(async () 
                        => await new MessageDialog("Failed to obtain access to multimedia devices!").ShowAsync());
            });
        }

        private string AppVersion = "N/A";

        private void WebRtcInitialized(bool succeeded)
        {
            if (succeeded)
            {
                // Configure application version string format
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                AppVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

                Debug.WriteLine($"Application version: {AppVersion}");

                InstallFactories();

                Task.Run(async() => 
                {
                    await Devices.Instance.GetMediaAsync();
                });

                RunOnUiThread(() => OnInitialized?.Invoke());
            }
            else
                RunOnUiThread(async () => await new MessageDialog("Failed to initialize WebRTC library!").ShowAsync());
        }

        /// <summary>
        /// Install the signaler and the calling factories.
        /// </summary>
        private void InstallFactories()
        {
            PeerCC.Setup.Install();
            WebRtcAdapter.Setup.Install();
        }
    }
}
