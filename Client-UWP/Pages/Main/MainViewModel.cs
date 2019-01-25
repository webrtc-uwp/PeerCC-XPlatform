using Client_UWP.Controllers;
using Client_UWP.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Client_UWP
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            DevicesController.RequestAccessForMediaCapture().AsTask().ContinueWith(antecedent =>
            {
                if (antecedent.Result)
                {
                    Initialize();
                }
                else
                {
                    var task = new MessageDialog("Failed to obtain access to multimedia devices!").ShowAsync();
                }
            });
        }

        private void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
