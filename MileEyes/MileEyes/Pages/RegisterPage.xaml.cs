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
    }
}