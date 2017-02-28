using System.ComponentModel;
using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ManualButton), typeof(ManualButtonRenderer))]

namespace MileEyes.iOS.Renderers
{
    class ManualButtonRenderer : ButtonRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            Control.ContentEdgeInsets = new UIEdgeInsets(10, 15, 10, 15);
        }
    }
}