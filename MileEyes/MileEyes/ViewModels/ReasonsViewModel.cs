using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    class ReasonsViewModel : ViewModel
    {
        private ReasonViewModel _selectedReason;

        public ReasonViewModel SelectedReason
        {
            get { return _selectedReason; }
            set
            {
                if (_selectedReason == value) return;

                _selectedReason = value;

                OnPropertyChanged(nameof(SelectedReason));
            }
        }

        public ObservableCollection<ReasonViewModel> Reasons { get; set; } = new ObservableCollection<ReasonViewModel>();

        public ReasonsViewModel()
        {
            SelectedReason = new ReasonViewModel(new Reason()
            {
                Text = string.Empty
            });
            RefreshCommand = new Command(Refresh);
            SetDefaultCommand = new Command<ReasonViewModel>(SetDefault);
            DeleteCommand = new Command<ReasonViewModel>(DeleteReason);
            SelectCommand = new Command(Select);
            Refresh();
        }

        public ICommand RefreshCommand { get; set; }

        public event EventHandler NoReasons = delegate { };

        public async void Refresh()
        {
            Refreshing = true;

            var reasons = await Services.Host.ReasonService.GetReasons();

            Reasons.Clear();

            var enumerable = reasons as Reason[] ?? reasons.ToArray();
            Reason defaultReason = null;

            if (enumerable.Any())
            {
                foreach (var reason in enumerable)
                {
                    if (reason.Default == true)
                    {
                        defaultReason = reason;
                    }

                    Reasons.Add(new ReasonViewModel(reason));
                }
            }

            if (defaultReason != null)
            {
                SelectedReason = new ReasonViewModel(defaultReason);
            }
            else
            {
                SelectedReason = new ReasonViewModel(new Reason()
                {
                    Text = ""
                });
            }

            //else
            //{
            //    NoReasons?.Invoke(this, EventArgs.Empty);
            //}

            Refreshing = false;
        }

        public ICommand SetDefaultCommand { get; set; }

        public async void SetDefault(ReasonViewModel reason)
        {
            Busy = true;

            await Services.Host.ReasonService.SetDefault(reason.Id);

            Refresh();

            Busy = false;
        }

        public ICommand DeleteCommand { get; set; }

        public async void DeleteReason(ReasonViewModel reason)
        {
            Busy = true;

            await Services.Host.ReasonService.DeleteReason(reason.Id);

            Refresh();

            Busy = false;
        }
        
        public ICommand SelectCommand { get; set; }

        public event EventHandler<string> Selected = delegate { };
        public event EventHandler<ReasonViewModel> DefaultSelected = delegate { };
        public event EventHandler NotSelected = delegate { };

        public void Select()
        {
            if (!string.IsNullOrEmpty(SelectedReason?.Text))
            {
                Selected?.Invoke(this, SelectedReason.Text);
                DefaultSelected?.Invoke(this, SelectedReason);
            }
            else
            {
                NotSelected?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
