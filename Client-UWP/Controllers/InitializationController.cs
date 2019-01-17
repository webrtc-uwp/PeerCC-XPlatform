using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_UWP.Controllers
{
    public sealed class InitializationController
    {
        private static InitializationController instance = null;
        private static readonly object InstanceLock = new object();

        public static InitializationController Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new InitializationController();

                    return instance;
                }
            }
        }

        public void Initialize()
        {
            SettingsController.Instance.Initialize();
            DevicesController.Instance.Initialize();
        }
    }
}
