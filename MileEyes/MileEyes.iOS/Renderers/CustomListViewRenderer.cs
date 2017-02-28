using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomListView), typeof(CustomListViewRenderer))]

namespace MileEyes.iOS.Renderers
{
    class CustomListViewRenderer : ListViewRenderer
    {
    }
}