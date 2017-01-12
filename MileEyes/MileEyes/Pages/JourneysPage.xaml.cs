using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.CustomControls;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class JourneysPage : ContentPage
    {
        public static event EventHandler GotoTrackPage = delegate { };

        public JourneysPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            (BindingContext as JourneysViewModel).Refresh();
        }

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            var journey = e.SelectedItem as JourneyViewModel;


            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new JourneyPage(journey));
            });

            (sender as ListView).SelectedItem = null;
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //    await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyManualPage())
            //    {
            //        BarBackgroundColor = Color.FromHex("#103D47"),
            //        BarTextColor = Color.White
            //    });
            //});

            GotoTrackPage?.Invoke(this, EventArgs.Empty);
        }

        private void ConnectButton_OnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new PremiumFeaturesPage());
            });
        }
    }
}
