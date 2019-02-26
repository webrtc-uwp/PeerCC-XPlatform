using ClientCore.Signaling;
using System.Diagnostics;

namespace PeerCC.Signaling
{
    public class SignalerFactory
    {
        public static ISignaler Create()
        {
            

            try
            {
                // if (...) http signaler .net standard 2.0
                return new HttpSignaler();

                // else  
                // return another signaler class
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Create signaler: " + ex.Message);

                return null;
            }

            
        }
    }
}
