using System.Threading.Tasks;

namespace ClientCore.Call
{
    public interface ICallProvider
    {
        Task<ICallInfo> PlaceCallAsync(CallConfiguration config);

        Task<ICallInfo> AnswerCallAsync(
            CallConfiguration config,
            string sdpOfRemoteParty
            );
    }
}
