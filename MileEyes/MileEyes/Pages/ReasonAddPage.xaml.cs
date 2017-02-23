using System;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class ReasonAddPage : ContentPage
    {
        public ReasonAddPage()
        {
            InitializeComponent();

            (BindingContext as ReasonViewModel).SaveComplete += ReasonAddPage_SaveComplete;
            (BindingContext as ReasonViewModel).SaveFailed += ReasonAddPage_SaveFailed;
        }

        private void ReasonAddPage_SaveFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () => { await DisplayAlert("Error", e, "Ok"); });
        }

        private void ReasonAddPage_SaveComplete(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }
    }
}