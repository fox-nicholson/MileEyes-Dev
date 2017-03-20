using System;
using MileEyes.Services.Models;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class CloudPage : ContentPage
    {
        public CloudPage()
        {
            InitializeComponent();

            (BindingContext as CloudViewModel).LoggedOut += CloudPage_LoggedOut;
        }

        private void CloudPage_LoggedOut(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopToRootAsync(); });
        }
                
        public bool Wait()
        {
            (BindingContext as CloudViewModel).Busy = false;
            return false;
        }
    }
}