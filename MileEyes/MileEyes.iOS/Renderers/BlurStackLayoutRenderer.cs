using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly:ExportRenderer(typeof(BlurStackLayout), typeof(BlurStackLayoutRenderer))]
namespace MileEyes.iOS.Renderers
{
    class BlurStackLayoutRenderer : ViewRenderer<BlurStackLayout, UIView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<BlurStackLayout> e)
        {
            base.OnElementChanged(e);
            
            if (Control != null)
            {
                Control.BackgroundColor = UIColor.Clear;

                var blur = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraDark);
                var blurView = new UIVisualEffectView(blur);

                blurView.Frame = Control.Frame;

                blurView.Layer.CornerRadius = e.NewElement.CornerRadius;
                blurView.Layer.MasksToBounds = true;

                Control.InsertSubview(blurView, 0);

                SetNeedsDisplay();
            }
        }
    }
}
