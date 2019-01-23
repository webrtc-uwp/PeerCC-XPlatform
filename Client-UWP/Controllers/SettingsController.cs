using Windows.Storage;

namespace Client_UWP.Controllers
{
    public sealed class SettingsController
    {
        private static SettingsController instance = null;
        private static readonly object InstanceLock = new object();

        public static SettingsController Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new SettingsController();

                    return instance;
                }
            }
        }

        private SettingsController() { }

        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public void Initialize ()
        {
            
        }
    }
}
