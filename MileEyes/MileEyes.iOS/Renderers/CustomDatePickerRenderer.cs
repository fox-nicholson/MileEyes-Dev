using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Foundation;
using MileEyes.CustomControls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MileEyes.CustomControls.CustomDatePicker), typeof(MileEyes.iOS.Renderers.CustomDatePickerRenderer))]
namespace MileEyes.iOS.Renderers
{
    class CustomDatePickerRenderer : ViewRenderer<CustomDatePicker, UIDatePicker>
    {
        private UIDatePicker uiDatePicker;

        private CustomDatePicker dp;

        protected override void OnElementChanged(ElementChangedEventArgs<CustomDatePicker> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var frame = e.NewElement.Bounds;
                uiDatePicker = new UIDatePicker(frame.ToRectangleF());
                uiDatePicker.Mode = UIDatePickerMode.Date;

                uiDatePicker.SetValueForKey(e.NewElement.TextColor.ToUIColor(), (NSString)"textColor");
                

                SetNativeControl(uiDatePicker);
            }

            if (e.OldElement != null)
            {
                // Unsubscribe
                dp = null;

                Control.ValueChanged -= Control_ValueChanged;
            }
            if (e.NewElement != null)
            {
                // Subscribe
                dp = e.NewElement;

                Control.ValueChanged += Control_ValueChanged;
            }
        }

        private void Control_ValueChanged(object sender, EventArgs e)
        {
            dp.Date = Control.Date.ToDateTime().AddHours(1);
        }
    }
}
