using MileEyes.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MileEyes.CustomControls.LoadingButton), typeof(LoadingButtonRenderer))]

namespace MileEyes.Droid.Renderers
{
    class LoadingButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Background = Resources.GetDrawable("buttoncirclegray");
            }
        }
    }
}