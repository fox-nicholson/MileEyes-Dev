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
    public partial class ReasonsPage : ContentPage
    {
        public ReasonsPage()
        {
            InitializeComponent();
        }

        private void MenuItem_OnClicked(object sender, EventArgs e)
        {
            var addReasonPage = new ReasonAddPage();
            (addReasonPage.BindingContext as ReasonViewModel).SaveComplete += ReasonsPage_SaveComplete;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(addReasonPage);
            });
        }

        private void ReasonsPage_SaveComplete(object sender, EventArgs e)
        {
            (BindingContext as ReasonsViewModel).Refresh();
        }

        private void ReasonDeleteMenuItem_OnClicked(object sender, EventArgs e)
        {
            (BindingContext as ReasonsViewModel).DeleteReason(((sender as MenuItem).CommandParameter as ReasonViewModel));
        }

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            (sender as CustomListView).SelectedItem = null;
        }
    }
}

