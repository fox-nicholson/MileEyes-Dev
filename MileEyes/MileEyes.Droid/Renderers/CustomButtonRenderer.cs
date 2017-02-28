using MileEyes.CustomControls;
using MileEyes.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MileEyes.CustomControls.CustomButton), typeof(CustomButtonRenderer))]

namespace MileEyes.Droid.Renderers
{
    class CustomButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Background = Resources.GetDrawable((Element as CustomButton).IsRed ? "buttoncirclered" : "buttoncircle");
            }
        }
    }
}