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
                var loginPage = new LoginPage();
                (loginPage.BindingContext as LoginViewModel).LoginSuccess += PremiumFeaturesPage_LoginSuccess;
                await Navigation.PushAsync(loginPage);
            });
        }

        private void PremiumFeaturesPage_LoginSuccess(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopToRootAsync();
            });
        }

        private void SubscribeButton_OnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var registerPage = new RegisterPage();
                (registerPage.BindingContext as RegisterViewModel).RegisterSuccess += PremiumFeaturesPage_RegisterSuccess;
                await Navigation.PushAsync(registerPage);
            });
        }

        private void PremiumFeaturesPage_RegisterSuccess(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopToRootAsync();
            });
        }

        private void CancelMenuItem_OnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
            });
        }
    }
}
