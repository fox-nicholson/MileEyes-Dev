using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class PremiumFeaturesPage : ContentPage
    {
        public PremiumFeaturesPage()
        {
            InitializeComponent();
        }

        private void LoginButton_OnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new LoginPage());
            });
        }

        private void SubscribeButton_OnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new RegisterPage());
            });
        }
    }
}
