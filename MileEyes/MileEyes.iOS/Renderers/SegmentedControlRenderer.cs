using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SegmentedControl), typeof(SegmentedControlRenderer))]

namespace MileEyes.iOS.Renderers
{
    public class SegmentedControlRenderer : ViewRenderer<SegmentedControl, UISegmentedControl>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SegmentedControl> e)
        {
            base.OnElementChanged(e);

            var segmentedControl = new UISegmentedControl();

            for (var i = 0; i < e.NewElement.Children.Count; i++)
            {
                segmentedControl.InsertSegment(e.NewElement.Children[i].Text, i, false);
            }

            segmentedControl.ValueChanged +=
                (sender, eventArgs) =>
                {
                    e.NewElement.SelectedValue = segmentedControl.TitleAt(segmentedControl.SelectedSegment);
                };

            segmentedControl.TintColor = UIColor.Green;

            segmentedControl.SelectedSegment = 0;

            SetNativeControl(segmentedControl);
        }
    }
}