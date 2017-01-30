using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using MileEyes.CustomControls;
using MileEyes.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RouteMap), typeof(RouteMapRenderer))]
namespace MileEyes.Droid.Renderers
{
    public class RouteMapRenderer : MapRenderer, IOnMapReadyCallback
    {
        GoogleMap map;
        ObservableCollection<Position> routeCoordinates;
        private Polyline polyline;

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);
            
            if (e.OldElement != null)
            {
                // Unsubscribe
            }

            if (e.NewElement != null)
            {
                var formsMap = (RouteMap)e.NewElement;
                routeCoordinates = formsMap.RouteCoordinates;

                ((MapView)Control).GetMapAsync(this);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (polyline != null)
            {
                UpdateRoute();
            }
        }

        public void UpdateRoute()
        {
            polyline.Remove();

            var polylineOptions = new PolylineOptions();
            polylineOptions.InvokeColor(0x66FF0000);

            foreach (var position in routeCoordinates)
            {
                polylineOptions.Add(new LatLng(position.Latitude, position.Longitude));
            }

            polyline = map.AddPolyline(polylineOptions);
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            map = googleMap;

            var polylineOptions = new PolylineOptions();
            polylineOptions.InvokeColor(0x66FF0000);

            foreach (var position in routeCoordinates)
            {
                polylineOptions.Add(new LatLng(position.Latitude, position.Longitude));
            }

            polyline = map.AddPolyline(polylineOptions);
        }
    }
}