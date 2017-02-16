using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MileEyes.Extensions;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    /// <summary>
    /// Journeys List View Model
    /// </summary>
    class JourneysViewModel : ViewModel
    {
        public ObservableCollection<JourneyGroup> Journeys { get; set; } = new ObservableCollection<JourneyGroup>();

        private bool _hasJourneys;

        public bool HasJourneys
        {
            get { return _hasJourneys; }
            set
            {
                if (_hasJourneys == value) return;
                _hasJourneys = value;
                OnPropertyChanged(nameof(HasJourneys));
                OnPropertyChanged(nameof(HasNoJourneys));
            }
        }

        public bool HasNoJourneys => !_hasJourneys;

        public JourneysViewModel()
        {
            RefreshCommand = new Command(Refresh);
            Refresh();
        }

        public ICommand RefreshCommand { get; set; }

        public override async void Refresh()
        {
            base.Refresh();

            // Mark View as Refreshing
            Refreshing = true;

            // Clear Journey Groups
            Journeys.Clear();

            // Get collection of Journey objects from Journey Service
            var journeys = await Services.Host.JourneyService.GetJourneys();

            // Dump to array to prevent multiple enumerations
            var enumerable = journeys as Journey[] ?? journeys.ToArray();

            var sortedJourneys = enumerable.OrderBy(j => j.Date);

            // Hold todays date
            var todaysDate = DateTime.UtcNow.Date;
            // Use Todays Date to filter journeys for Today and factory them into JourneyViewModel's
            var todaysJourneys = sortedJourneys.Where(j => j.Date.Date == todaysDate).Select(j => new JourneyViewModel(j));
            // Dump to array to prevent multiple enumerations.
            var todaysJourneyViewModels = todaysJourneys as JourneyViewModel[] ?? todaysJourneys.ToArray();
            // Check if there were any journey's today.
            if (todaysJourneyViewModels.Any())
            {
                // Create the JourneyGroup for Today
                var todayGroup = new JourneyGroup("Today", todaysDate.Date.ToString());
                // Add Todays Journey's to the JourneyGroup
                todayGroup.AddRange(todaysJourneyViewModels);
                // Add Todays JourneyGroup to the Journey Grousp list
                Journeys.Add(todayGroup);
            }


            // Hold yesterdays date
            var yesterdaysDate = DateTimeOffset.UtcNow.AddDays(-1).Date;
            // Use Yesterdays Date to filter journeys for Yesterday and factory them into JourneyViewModel's
            var yesterdaysJourneys =
                sortedJourneys.Where(j => j.Date.Date == yesterdaysDate)
                    .Select(j => new JourneyViewModel(j));
            // Dump to array to prevent multiple enumerations.
            var yesterdaysJourneyViewModels = yesterdaysJourneys as JourneyViewModel[] ?? yesterdaysJourneys.ToArray();
            // Check if there were any journey's yesterday.
            if (yesterdaysJourneyViewModels.Any())
            {
                // Create the JourneyGroup for Yesterday
                var yesterdayGroup = new JourneyGroup("Yesterday", yesterdaysDate.Date.ToString());
                // Add Yesterdays Journey's to the JourneyGroup
                yesterdayGroup.AddRange(yesterdaysJourneyViewModels);
                // Add Yesterdays JourneyGroup to the Journey Grousp list
                Journeys.Add(yesterdayGroup);
            }

            // Hold Last Weeks date
            var lastWeeksDate = DateTimeOffset.UtcNow.Date.AddDays(-7).StartOfWeek(DayOfWeek.Sunday);

            // Use Yesterdays Date and Last Weeks Date to filter journeys for Last Week and factory them into JourneyViewModel's
            var lastWeeksJourneys = sortedJourneys.Where(j => j.Date.Date > lastWeeksDate && j.Date.Date < yesterdaysDate)
                .Select(j => new JourneyViewModel(j));
            // Dump to array to prevent multiple enumerations.
            var lastWeeksJourneyViewModels = lastWeeksJourneys as JourneyViewModel[] ?? lastWeeksJourneys.ToArray();
            // Check if there were any journey's last week.
            if (lastWeeksJourneyViewModels.Any())
            {
                // Create the JourneyGroup for Last Week
                var lastWeekGroup = new JourneyGroup("Last Week", lastWeeksDate.Date.ToString());
                // Add Last Weeks Journey's to the JourneyGroup
                lastWeekGroup.AddRange(lastWeeksJourneyViewModels);
                // Add Last Weeks JourneyGroup to the Journey Grousp list
                Journeys.Add(lastWeekGroup);
            }

            // Filter remaining Journeys that havent been dealt with
            var remainingJourneys = sortedJourneys.Where(j => j.Date < lastWeeksDate);
            // Group journeys by Year
            var journeysByYear = remainingJourneys.GroupBy(j => j.Date.Date.Year).Select(g => g.ToList()).ToList();
            
            // Go through years
            foreach (var yearsJourneys in journeysByYear)
            {
                // Group this years Journeys by Month
                var journeysByMonth =
                    yearsJourneys.GroupBy(j => j.Date.Date.Month).Select(g => g.ToList()).ToList();

                // Go through months 
                foreach (var monthsJourneys in journeysByMonth)
                {
                    // Create the JourneyGroup for the Month
                    var monthsGroup = new JourneyGroup(monthsJourneys.FirstOrDefault().Date.ToString("MMMM", null), monthsJourneys.FirstOrDefault().Date.ToString("MMMM", null));
                    // Add Journeys to the JourneyGroup
                    monthsGroup.AddRange(monthsJourneys.Select(j => new JourneyViewModel(j)));
                    // Add 
                    Journeys.Add(monthsGroup);
                }
            }

            // Set whether there are any journeys
            HasJourneys = Journeys.Any();

            // Mark View as not Refreshing
            Refreshing = false;
        }
    }
}
