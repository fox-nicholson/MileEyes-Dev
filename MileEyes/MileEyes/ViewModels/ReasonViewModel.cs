using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    class ReasonViewModel : ViewModel
    {
        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        private bool _default;
        public bool Default
        {
            get { return _default; }
            set
            {
                if (_default == value) return;
                _default = value;
                OnPropertyChanged(nameof(Default));
            }
        }

        public ReasonViewModel()
        {
            Init();
        }

        public ReasonViewModel(Reason model)
        {
            Id = model.Id;
            Text = model.Text;
            Default = model.Default;

            Init();
        }

        private void Init()
        {
            SaveCommand = new Command(Save);
        }

        public ICommand SaveCommand { get; set; }

        public event EventHandler<string> SaveFailed = delegate { };
        public event EventHandler SaveComplete = delegate { };

        public async void Save()
        {
            Busy = true;

            if (string.IsNullOrEmpty(Text))
            {
                SaveFailed?.Invoke(this, "Reason is required.");
                return;
            }

            var reason = await Services.Host.ReasonService.SaveReason(new Reason()
            {
                Text = Text
            });

            if (string.IsNullOrEmpty(reason.Id))
            {
                SaveFailed?.Invoke(this, "Reason was not saved.");

                return;
            }

            SaveComplete?.Invoke(this, EventArgs.Empty);

            Busy = false;
        }
    }
}
