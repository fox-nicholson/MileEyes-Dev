using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class ReasonSelectionPage : ContentPage
    {
        public ReasonSelectionPage()
        {
            InitializeComponent();

            (BindingContext as ReasonsViewModel).NoReasons += ReasonSelectionPage_NoReasons;
            (BindingContext as ReasonsViewModel).NotSelected += ReasonSelectionPage_NotSelected;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            (BindingContext as ReasonsViewModel).Refresh();
        }

        private void ReasonSelectionPage_NoReasons(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var choice =
                    await DisplayAlert("No Reasons Saved", "Would you like to add a new reason now?", "Yes", "No");

                if (choice)
                {
                    var addReasonPage = new ReasonAddPage();
                    (addReasonPage.BindingContext as ReasonViewModel).SaveComplete += ReasonSelectionPage_SaveComplete;
                    ;
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PushAsync(addReasonPage);
                    });
                }
                else
                {
                    await Navigation.PopAsync();
                }
            });
        }

        private void ReasonSelectionPage_SaveComplete(object sender, EventArgs e)
        {
            (BindingContext as ReasonsViewModel).Refresh();
        }

        private void ReasonSelectionPage_NotSelected(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Problem...", "You Must Select a Default Reason", "Ok");
            });
        }
    }
}
