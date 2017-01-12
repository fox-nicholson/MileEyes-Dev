using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.ViewModels
{
    class EngineTypeViewModel : ViewModel
    {
        private EngineType _engineType;
        public EngineType EngineType => _engineType;

        private string _id;

        public string Id => _id;

        private string _name;

        public string Name => _name;

        public EngineTypeViewModel(EngineType et)
        {
            _engineType = et;

            _id = _engineType.Id;
            _name = _engineType.Name;
        }
    }
}
