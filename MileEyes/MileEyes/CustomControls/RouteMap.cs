using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MileEyes.CustomControls
{
    public class RouteMap : Map
    {
        public static readonly BindableProperty RouteCoordinatesProperty = BindableProperty.Create(
            propertyName: "RouteCoordinates",
            returnType: typeof(ObservableCollection<Position>),
            declaringType: typeof(RouteMap),
            defaultValue: new ObservableCollection<Position>(),
            propertyChanged: OnItemSourcePropertyChanged,
            defaultBindingMode: BindingMode.TwoWay);


        public ObservableCollection<Position> RouteCoordinates
        {
            get { return (ObservableCollection<Position>) GetValue(RouteCoordinatesProperty); }
            set { SetValue(RouteCoordinatesProperty, value); }
        }

        //public event EventHandler ClearRouteRequested = delegate { };

        //public void ClearRoute()
        //{
        //    ClearRouteRequested?.Invoke(this, null);
        //}

        private static void OnItemSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
        }

        public void Invalidate()
        {
            OnPropertyChanged();
        }
    }
}