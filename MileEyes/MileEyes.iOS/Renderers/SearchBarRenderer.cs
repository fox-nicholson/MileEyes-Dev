using System;
using System.Collections.Generic;
using System.Text;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SearchBar), typeof(CustomSearchBarRenderer))]
namespace MileEyes.iOS.Renderers
{
    class CustomSearchBarRenderer : SearchBarRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.ShowsCancelButton = false;
                Control.SetShowsCancelButton(false, false);
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            Control.ShowsCancelButton = false;
            Control.SetShowsCancelButton(false, false);
        }
    }
}
