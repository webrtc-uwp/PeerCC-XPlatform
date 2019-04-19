using Client_UWP.MVVM;
using GuiCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace Client_UWP
{
    public delegate void InitializedDelegate();
    internal class MainViewModel : DispatcherBindableBase
    {
        public event InitializedDelegate OnInitialized; 

        public MainViewModel(CoreDispatcher uiDispatcher)
            : base(uiDispatcher)
        {
            Devices.Instance.RequestAccessForMediaCapture().AsTask().ContinueWith(antecedent =>
            {
                if (antecedent.Result)
                {
                    Initialize(uiDispatcher);
                }
                else
                {
                    var task = new MessageDialog("Failed to obtain access to multimedia devices!").ShowAsync();
                }
            });

            IList<Devices.MediaDeviceModel> videoDevices;

            Task.Run(async () =>
            {
                videoDevices = await Devices.Instance.GetVideoCaptureDevices();

                foreach (Devices.MediaDeviceModel videoCaptureDevice in videoDevices)
                    Devices.Instance.CamerasList.Add(videoCaptureDevice.Name);
            });
        }

        /// <summary>
        /// The initializer for MainViewModel.
        /// </summary>
        /// <param name="uiDispatcher">The UI dispatcher.</param>
        private void Initialize(CoreDispatcher uiDispatcher)
        {
            RunOnUiThread(() => OnInitialized?.Invoke());
        }
    }
}
