using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_UWP.Controllers
{
    public sealed class DevicesController
    {
        private static DevicesController instance = null;
        private static readonly object InstanceLock = new object();

        public static DevicesController Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new DevicesController();

                    return instance;
                }
            }
        }

        private DevicesController()
        {

        }

        public void Initialize()
        {

        }
    }
}
