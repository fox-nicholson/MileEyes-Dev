using System;
using System.Linq;
using CoreGraphics;
using CoreLocation;
using Foundation;
using MapKit;
using MileEyes.CustomControls;
using ObjCRuntime;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RouteMap), typeof(MileEyes.iOS.Renderers.RouteMapRenderer))]

namespace MileEyes.iOS.Renderers
{
    public class RouteMapRenderer : MapRenderer
    {
        private RouteMap formsMap;
        private MKMapView map;
        private MKPolylineRenderer polylineRenderer;
        private MKPolyline routeOverlay;

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (Control == null) return;

            map = Control as MKMapView;

            if (e.OldElement != null)
            {
                if (map != null)
                {
                    map.OverlayRenderer = null;
                }
            }

            if (e.NewElement == null) return;

            if (map == null) return;

            formsMap = (RouteMap) e.NewElement;

            //formsMap.ClearRouteRequested += FormsMap_ClearRouteRequested;
        }

        private void FormsMap_ClearRouteRequested(object sender, EventArgs e)
        {
            if (map.Overlays != null && map.Overlays.Any())
            {
                map.RemoveOverlays(map.Overlays);
            }

            SetNeedsDisplay();
            SetNeedsLayout();
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateRoute();
        }

        private void UpdateRoute()
        {
            map.OverlayRenderer = GetOverlayRenderer;

            routeOverlay = null;

            var coords = new CLLocationCoordinate2D[formsMap.RouteCoordinates.Count];

            var index = 0;

            foreach (var position in formsMap.RouteCoordinates)
            {
                coords[index] = new CLLocationCoordinate2D(position.Latitude, position.Longitude);
                index++;
            }

            routeOverlay = MKPolyline.FromCoordinates(coords);

            if (map.Overlays != null && map.Overlays.Any())
            {
                map.RemoveOverlays(map.Overlays);
            }

            map.AddOverlay(routeOverlay);

            SetNeedsDisplay();
            SetNeedsLayout();
        }

        MKOverlayRenderer GetOverlayRenderer(MKMapView mapView, IMKOverlay overlayWrapper)
        {
            var overlay = Runtime.GetNSObject(overlayWrapper.Handle) as IMKOverlay;
            polylineRenderer = new MKPolylineRenderer(overlay as MKPolyline)
            {
                FillColor = UIColor.FromRGB(22, 174, 231),
                StrokeColor = UIColor.FromRGB(22, 174, 231),
                LineWidth = 5,
                Alpha = 1f,
                LineDashPattern = new[] {new NSNumber(0.5), new NSNumber(12)},
                LineCap = CGLineCap.Round,
                LineJoin = CGLineJoin.Round
            };

            return polylineRenderer;
        }
    }
}