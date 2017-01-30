using Xamarin.Forms;

namespace MileEyes.CustomControls
{
    public interface ICustomViewCell
    {
        bool ShowDisclosure { get; set; }
        ImageSource ImageSource { get; set; }
        Color TextColor { get; set; }
    }
}