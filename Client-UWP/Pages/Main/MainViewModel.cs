using Client_UWP.Controllers;
using Client_UWP.MVVM;
using System;
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
            DevicesController.RequestAccessForMediaCapture().AsTask().ContinueWith(antecedent =>
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
