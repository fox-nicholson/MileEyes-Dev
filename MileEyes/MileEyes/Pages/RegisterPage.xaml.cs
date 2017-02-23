using System;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();

            (BindingContext as RegisterViewModel).RegisterFailed += RegisterPage_RegisterFailed;
            (BindingContext as RegisterViewModel).RegisterSuccess += RegisterPage_RegisterSuccess;
        }

        private void RegisterPage_RegisterSuccess(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopToRootAsync(); });
        }

        private void RegisterPage_RegisterFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () => { await DisplayAlert("Registration Failed", e, "Ok"); });
        }

        private void AddressCell_OnTapped(object sender, EventArgs e)
        {
            var selectAddressPage = new RegisterAddressSelectionPage();
            selectAddressPage.AddressSelected += SelectAddressPage_AddressSelected;

            Device.BeginInvokeOnMainThread(() =>
            {
                Device.OnPlatform(
                    async () =>
                    {
                        await Navigation.PushModalAsync(new NavigationPage(selectAddressPage)
                        {
                            BarBackgroundColor = Color.FromHex("#103D47"),
                            BarTextColor = Color.White
                        });
                    }, async () =>
                    {
                        await Navigation.PushModalAsync(new NavigationPage(selectAddressPage)
                        {
                            BarBackgroundColor = Color.FromHex("#58C0EE"),
                            BarTextColor = Color.White
                        });
                    });
            });
        }

        private void SelectAddressPage_AddressSelected(object sender, Services.Models.Address e)
        {
            (BindingContext as RegisterViewModel).Address = e;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopModalAsync(); });
        }
    }
}