using ClientCore.Signaling;

namespace PeerCC.Signaling
{
    public class SignalerFactory
    {
        public static ISignaler Create()
        {
            // if (...) http signaler .net standard 2.0
            return new HttpSignaler();

            // else  
            // return another signaler class
        }
    }
}
