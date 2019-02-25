using System.Threading.Tasks;
using System.Drawing;

namespace ClientCore.Call
{
    public delegate void ResolutionChangeHandler(
        MediaDirection direction,
        Size dimension
        );
    public delegate void FrameRateChangeHandler(
        MediaDirection direction,
        int frameRate
        );

    public interface ICall
    {
        Task HangupAsync();

        event FrameRateChangeHandler OnFrameRateChanged;
        event ResolutionChangeHandler OnResolutionChanged;
    }
}
