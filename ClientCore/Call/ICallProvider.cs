namespace ClientCore.Call
{
    public interface ICallProvider
    {
        ICall GetCall();

        //Task<ICallInfo> PlaceCallAsync(CallConfiguration config);

        //Task<ICallInfo> AnswerCallAsync(
        //    CallConfiguration config, 
        //    string sdpOfRemoteParty
        //    );
    }
}
