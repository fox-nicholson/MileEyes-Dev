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

        public async void Refresh()
        {
            Refreshing = true;

            Journeys.Clear();

            var journeys = await Services.Host.JourneyService.GetJourneys();

            var enumerable = journeys as Journey[] ?? journeys.ToArray();

            var todaysDate = DateTime.UtcNow.Date;
            var todaysJourneys = enumerable.Where(j => j.Date.Date == todaysDate).Select(j => new JourneyViewModel(j));
            var todaysJourneyViewModels = todaysJourneys as JourneyViewModel[] ?? todaysJourneys.ToArray();
            if (todaysJourneyViewModels.Any())
            {
                var todayGroup = new JourneyGroup("Today", todaysDate.Date.ToString());
                todayGroup.AddRange(todaysJourneyViewModels);
                Journeys.Add(todayGroup);
            }

            var yesterdaysDate = DateTimeOffset.UtcNow.AddDays(-1).Date;
            var yesterdaysJourneys =
                enumerable.Where(j => j.Date.Date == yesterdaysDate)
                    .Select(j => new JourneyViewModel(j));
            var yesterdaysJourneyViewModels = yesterdaysJourneys as JourneyViewModel[] ?? yesterdaysJourneys.ToArray();
            if (yesterdaysJourneyViewModels.Any())
            {
                var yesterdayGroup = new JourneyGroup("Yesterday", yesterdaysDate.Date.ToString());
                yesterdayGroup.AddRange(yesterdaysJourneyViewModels);

                Journeys.Add(yesterdayGroup);
            }

            var lastWeeksDate = DateTimeOffset.UtcNow.Date.AddDays(-7).StartOfWeek(DayOfWeek.Sunday);

            var lastWeeksJourneys = enumerable.Where(j => j.Date.Date > lastWeeksDate && j.Date.Date < yesterdaysDate)
                .Select(j => new JourneyViewModel(j));
            var lastWeeksJourneyViewModels = lastWeeksJourneys as JourneyViewModel[] ?? lastWeeksJourneys.ToArray();
            if (lastWeeksJourneyViewModels.Any())
            {
                var lastWeekGroup = new JourneyGroup("Last Week", lastWeeksDate.Date.ToString());
                lastWeekGroup.AddRange(lastWeeksJourneyViewModels);

                Journeys.Add(lastWeekGroup);
            }

            var remainingJourneys = enumerable.Where(j => j.Date < lastWeeksDate);

            var journeysByYear = remainingJourneys.GroupBy(j => j.Date.Date.Year).Select(g => g.ToList()).ToList();

            foreach (var yearsJourneys in journeysByYear)
            {
                var journeysByMonth =
                    yearsJourneys.GroupBy(j => j.Date.Date.Month).Select(g => g.ToList()).ToList();

                foreach (var monthsJourneys in journeysByMonth)
                {
                    var monthsGroup = new JourneyGroup(monthsJourneys.FirstOrDefault().Date.ToString("MMMM", null), monthsJourneys.FirstOrDefault().Date.ToString("MMMM", null));

                    monthsGroup.AddRange(monthsJourneys.Select(j => new JourneyViewModel(j)));

                    Journeys.Add(monthsGroup);
                }
            }

            HasJourneys = Journeys.Any();

            Refreshing = false;
        }
    }
}
