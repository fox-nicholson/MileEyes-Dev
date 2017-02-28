using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomNavigationPage), typeof(CustomNavigationPageRenderer))]

namespace MileEyes.iOS.Renderers
{
    class CustomNavigationPageRenderer : NavigationRenderer
    {
        IPageController PageController => Element as IPageController;
        IElementController ElementController => Element;
    }
}