namespace GuiCore
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
            DevicesController.Instance.Initialize();
        }
    }
}
