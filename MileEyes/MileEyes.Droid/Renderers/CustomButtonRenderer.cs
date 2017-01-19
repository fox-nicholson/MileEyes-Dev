using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MileEyes;
using MileEyes.CustomControls;
using MileEyes.Droid.Renderers;

[assembly:ExportRenderer(typeof(MileEyes.CustomControls.CustomButton), typeof(CustomButtonRenderer))]
namespace MileEyes.Droid.Renderers
{
    class CustomButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                if ((Element as CustomButton).IsRed)
                {
                    Control.Background = Resources.GetDrawable("buttoncirclered");
                }
                else
                {
                    Control.Background = Resources.GetDrawable("buttoncircle");
                }
            }
        }
    }
}