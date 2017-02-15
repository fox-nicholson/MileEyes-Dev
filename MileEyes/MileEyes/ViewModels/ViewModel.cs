using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Annotations;

namespace MileEyes.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        private bool _authenticated;

        public bool Authenticated
        {
            get { return _authenticated; }
            set
            {
                if (_authenticated == value) return;
                _authenticated = value;
                OnPropertyChanged(nameof(Authenticated));
                OnPropertyChanged(nameof(NotAuthenticated));
            }
        }

        public bool NotAuthenticated => !_authenticated;

        private bool _busy;
        public bool Busy
        {
            get { return _busy; }
            set
            {
                if (_busy == value) return;
                _busy = value;
                OnPropertyChanged(nameof(Busy));
                OnPropertyChanged(nameof(NotBusy));
            }
        }
        public bool NotBusy => !_busy;

        private bool _refreshing;
        public bool Refreshing
        {
            get { return _refreshing; }
            set
            {
                if (_refreshing == value) return;
                _refreshing = value;
                OnPropertyChanged(nameof(Refreshing));
                OnPropertyChanged(nameof(Refreshed));
            }
        }
        public bool Refreshed => !_refreshing;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            Refresh();
        }

        public virtual void Refresh()
        {
            Authenticated = Services.Host.AuthService.Authenticated;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
