using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.CustomControls;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class VehiclesPage : ContentPage
    {
        public VehiclesPage()
        {
            InitializeComponent();

            (BindingContext as VehiclesViewModel).VehicleRemoved += VehiclesPage_VehicleRemoved;
            (BindingContext as VehiclesViewModel).VehicleNotRemoved += VehiclesPage_VehicleNotRemoved;
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();

            (BindingContext as VehiclesViewModel).Refresh();
        }

        private void VehiclesPage_VehicleNotRemoved(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Vehicle Not Removed", e, "Ok");
            });
        }

        private void VehiclesPage_VehicleRemoved(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Vehicle Removed", e, "Ok");
            });
        }

        private void RemoveMenuItem_OnClicked(object sender, EventArgs e)
        {
            (BindingContext as VehiclesViewModel).Remove(((sender as MenuItem).CommandParameter as VehicleViewModel));
        }

        private void MenuItem_OnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new VehicleAddPage());
            });
        }

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            (sender as CustomListView).SelectedItem = null;
        }
    }
}
