using System;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    /// <summary>
    ///     Journeys List Page
    /// </summary>
    public partial class JourneysPage : ContentPage
    {
        private bool _busy;

        public JourneysPage()
        {
            InitializeComponent();
        }

        public static event EventHandler GotoTrackPage = delegate { };

        /// <summary>
        ///     Runs when page is coming into view
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Kick off a Refresh of the View Model
            (BindingContext as JourneysViewModel).Refresh();
        }

        /// <summary>
        ///     Handles tapping of Journey items in the List
        /// </summary>
        /// <param name="sender">Will be a ListView, if not then someones using this method where they shouldnt!</param>
        /// <param name="e">
        ///     Journey ListView selection event args, e.SelectedItem will be a JourneyViewModel, if not, someone is
        ///     using this where they shouldnt.
        /// </param>
        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            //Check the Busy state to prevent Multi-Tap
            if (_busy) return;

            _busy = true;
            // Deal with empty selection (this occurs on deselection.
            if (e.SelectedItem == null) return;

            // Cast SelectedItem to JourneyViewModel, will be safe provided method isnt used where it shoudlnt be.
            var journey = e.SelectedItem as JourneyViewModel;

            // Push the Journey's Detail page onto the Navigation Stack using Main Thread
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new JourneyPage(journey)); });

            // Deselect the Journey item in the Journey List
            (sender as ListView).SelectedItem = null;

            //Use Wait() to prevent Multi-Tap
            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        /// <summary>
        ///     Track New Journey Button Handler
        /// </summary>
        /// <param name="sender">The button which kicked off the event</param>
        private void Button_OnClicked(object sender, EventArgs e)
        {
            if (_busy) return;

            _busy = true;

            // Invoke the static event to switch the Tab
            GotoTrackPage?.Invoke(this, EventArgs.Empty);

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        /// <summary>
        ///     Connect To MileEyes Button Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_OnClicked(object sender, EventArgs e)
        {
            if (_busy) return;

            _busy = true;

            // Push Premium Features page onto the Navigation Stack
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new PremiumFeaturesPage()); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void ListView_OnItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (e.Item == null) return;

            var journey = e.Item as JourneyViewModel;

            journey?.InitRoute();
        }

        //Wait() forces device to wait 1 second before a Button/Item can be tapped again
        private bool Wait()
        {
            _busy = false;
            return false;
        }
    }
}