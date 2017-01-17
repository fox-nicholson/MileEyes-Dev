using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MileEyes;
using MileEyes.Droid.Renderers;

[assembly:ExportRenderer(typeof(MileEyes.CustomControls.CustomButton), typeof(CustomButtonRenderer))]
namespace MileEyes.Droid.Renderers
{
    class CustomButtonRenderer : ButtonRenderer
    {

    }
}