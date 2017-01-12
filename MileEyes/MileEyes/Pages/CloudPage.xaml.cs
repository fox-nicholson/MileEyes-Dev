using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class CloudPage : ContentPage
    {
        public CloudPage()
        {
            InitializeComponent();
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
            (BindingContext as CloudViewModel).Address = e;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
            });
        }
    }
}
