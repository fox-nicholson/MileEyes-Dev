using MileEyes.Services.Models;

namespace MileEyes.ViewModels
{
    internal class EngineTypeViewModel : ViewModel
    {
        private EngineType _engineType;

        public EngineType EngineType
        {
            get { return _engineType; }
            set
            {
                if (_engineType == value) return;
                _engineType = value;
                OnPropertyChanged(nameof(EngineType));
            }
        }

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

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public EngineTypeViewModel(EngineType et)
        {
            EngineType = et;

            Id = _engineType.Id;
            Name = _engineType.Name;
        }
    }
}