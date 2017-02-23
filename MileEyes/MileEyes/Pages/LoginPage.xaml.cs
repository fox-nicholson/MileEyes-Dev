using System;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            (BindingContext as LoginViewModel).LoginSuccess += LoginPage_LoginSuccess;
            (BindingContext as LoginViewModel).LoginFailed += LoginPage_LoginFailed;
        }

        private void LoginPage_LoginSuccess(object sender, EventArgs e)
        {
            (BindingContext as LoginViewModel).Busy = true;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopToRootAsync(); });
            (BindingContext as LoginViewModel).Busy = false;
        }

        private void LoginPage_LoginFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () => { await DisplayAlert("Login Failed", e, "Ok"); });
        }
    }
}