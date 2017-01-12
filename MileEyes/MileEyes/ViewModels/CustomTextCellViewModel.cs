using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MileEyes.ViewModels
{
    class CustomTextCellViewModel : ViewModel
    {
        private string _icon;

        public string Icon
        {
            get { return _icon; }
            set
            {
                if (_icon == value) return;
                _icon = value;
                OnPropertyChanged(nameof(Icon));
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

        private string _detail;

        public string Detail
        {
            get { return _detail; }
            set
            {
                if (_detail == value) return;
                _detail = value;
                OnPropertyChanged(nameof(Detail));
            }
        }
    }
}
