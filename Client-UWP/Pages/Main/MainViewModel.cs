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
            Initialization.Instance.Initialized += Instance_Initialized;

            Devices.Instance.RequestAccessForMediaCapture().AsTask().ContinueWith(antecedent =>
            {
                if (antecedent.Result)
                    Initialization.Instance.CofigureWebRtcLib(uiDispatcher);
                else
                    RunOnUiThread(async () 
                        => await new MessageDialog("Failed to obtain access to multimedia devices!").ShowAsync());
            });
        }

        private string AppVersion = "N/A";

        private void Instance_Initialized(bool succeeded)
        {
            if (succeeded)
            {
                // Configure application version string format
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                AppVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

                Debug.WriteLine($"Application version: {AppVersion}");

                Initialization.Instance.InstallFactories();

                Task.Run(async() => 
                {
                    await Devices.Instance.GetMediaAsync();
                });

                RunOnUiThread(() => OnInitialized?.Invoke());
            }
            else
                RunOnUiThread(async () => await new MessageDialog("Failed to initialize WebRTC library!").ShowAsync());
        }
    }
}
