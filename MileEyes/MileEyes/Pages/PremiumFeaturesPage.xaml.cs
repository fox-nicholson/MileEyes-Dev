using System;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class PremiumFeaturesPage : ContentPage
    {
        private bool _busy;

        public PremiumFeaturesPage()
        {
            InitializeComponent();
        }

        private void LoginButton_OnClicked(object sender, EventArgs e)
        {
            if (_busy) return;

            _busy = true;

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new LoginPage()); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void SubscribeButton_OnClicked(object sender, EventArgs e)
        {
            if (_busy) return;

            _busy = true;

            var RegisterPage = new Uri("http://mileeyes-portal.azurewebsites.net");

            Device.OpenUri(RegisterPage);

            //Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new RegisterPage()); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private bool Wait()
        {
            _busy = false;
            return false;
        }
    }
}