using System.Threading.Tasks;

namespace GValidator.Validation.FrameSlicing
{
    public interface IFrameSlicer
    {
        Task TrySlice();
    }
}
