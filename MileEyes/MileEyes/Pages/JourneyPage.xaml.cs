using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MileEyes.Pages
{
    public partial class JourneyPage : ContentPage
    {
        public JourneyPage(JourneyViewModel j)
        {
            InitializeComponent();
            

            BindingContext = j;

            InitRoute();
        }

        private bool _open = true;
        private Rectangle _drawerExpandedPosition;
        private Rectangle _drawerMidPosition;
        private Rectangle _drawerCollapsedPosition;

        protected override void OnAppearing()
        {
            // PopulateViewModel(exercises[currentIndex]);

            var actualHeight = instructionsTitleLayout.Height;

            _drawerExpandedPosition = aInstructions.Bounds;

            _drawerMidPosition = aInstructions.Bounds;
            _drawerCollapsedPosition = aInstructions.Bounds;

            _drawerCollapsedPosition.Y = aInstructions.Height - actualHeight;

            _drawerExpandedPosition.Y = 0;

            base.OnAppearing();
        }

        private void PanGestureRecognizer_OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            //if (e.TotalY < 0)
            //{
            //    aInstructions.TranslateTo(_drawerExpandedPosition.X, _drawerExpandedPosition.Y, 200, Easing.CubicOut);
            //}
            //else if (e.TotalY > 0)
            //{
            //    aInstructions.TranslateTo(_drawerCollapsedPosition.X, _drawerCollapsedPosition.Y, 200, Easing.CubicOut);
            //}

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    if (_open)
                    {
                        _drawerMidPosition.Y = _drawerExpandedPosition.Y;
                    }
                    else
                    {
                        _drawerMidPosition.Y = _drawerCollapsedPosition.Y;
                    }
                    break;
                case GestureStatus.Running:
                    if (e.TotalY > 0)
                    {
                        _drawerMidPosition.Y = e.TotalY;
                        aInstructions.TranslateTo(_drawerMidPosition.X, _drawerMidPosition.Y, 200);
                    }
                    else
                    {
                        if (!_open)
                        {
                            _drawerMidPosition.Y = _drawerCollapsedPosition.Y + e.TotalY;
                            aInstructions.TranslateTo(_drawerMidPosition.X, _drawerMidPosition.Y, 200);
                        }
                        else
                        {
                            _drawerMidPosition.Y = _drawerExpandedPosition.Y + e.TotalY;
                            aInstructions.TranslateTo(_drawerMidPosition.X, _drawerMidPosition.Y, 200);
                        }
                    }

                    break;
                case GestureStatus.Completed:
                    if (_drawerMidPosition.Y > _drawerExpandedPosition.Y / 2)
                    {
                        aInstructions.TranslateTo(_drawerCollapsedPosition.X, _drawerCollapsedPosition.Y, 200,
                            Easing.CubicOut);

                        _drawerMidPosition.Y = _drawerCollapsedPosition.Y;

                        _open = false;
                    }
                    else
                    {
                        aInstructions.TranslateTo(_drawerExpandedPosition.X, _drawerExpandedPosition.Y, 200,
                            Easing.CubicOut);

                        _drawerMidPosition.Y = _drawerExpandedPosition.Y;

                        _open = true;
                    }
                    break;
                default:
                    // throw new ArgumentOutOfRangeException();
                    break;
            }
        }

        private void InitRoute()
        {
            var j = BindingContext as JourneyViewModel;

            var origin = j.OriginAddress;
            var dest = j.DestinationAddress;

            var midpoint = Services.Helpers.TrigHelpers.MidPoint(new[] { origin.Latitude, origin.Longitude },
                                new[] { dest.Latitude, dest.Longitude });

            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(midpoint[0], midpoint[1]), Distance.FromMiles(j.Distance * 0.6)));

            (BindingContext as JourneyViewModel).InitRoute();
        }
    }
}