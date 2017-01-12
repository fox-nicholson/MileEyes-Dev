﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
                await Navigation.PopModalAsync();
            });
        }

        private void RegisterPage_RegisterFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Registration Failed", e, "Ok");
            });
        }

        private void AddressCell_OnTapped(object sender, EventArgs e)
        {
            var selectAddressPage = new AddressSelectionPage();
            selectAddressPage.AddressSelected += SelectAddressPage_AddressSelected; ;

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushModalAsync(new NavigationPage(selectAddressPage));
            });
        }

        private void SelectAddressPage_AddressSelected(object sender, Services.Models.Address e)
        {
            (BindingContext as RegisterViewModel).Address = e;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
            });
        }
    }
}
