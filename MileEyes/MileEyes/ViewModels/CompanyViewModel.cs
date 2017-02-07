using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.ViewModels
{
    class CompanyViewModel : ViewModel
    {
        private Company _company;

        public Company Company => _company;

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
        
        private string _cloudId;

        public string CloudId
        {
            get { return _cloudId; }
            set
            {
                if (_cloudId == value) return;
                _cloudId = value;
                OnPropertyChanged(nameof(CloudId));
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

        private bool _personal;

        public bool Personal
        {
            get { return _personal; }
            set
            {
                if (_personal == value) return;
                _personal = value;
                OnPropertyChanged(nameof(Personal));
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
        
        public CompanyViewModel(Company company)
        {
            _company = company;

            Id = company.Id;
            CloudId = company.CloudId;
            Name = company.Name;
            Personal = company.Personal;
            Default = company.Default;
        }
    }
}
