﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopToRootAsync();
            });
        }

        private void LoginPage_LoginFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Login Failed", e, "Ok");
            });
        }
    }
}
