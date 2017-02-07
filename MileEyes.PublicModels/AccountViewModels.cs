using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MileEyes.PublicModels
{
    public class UserInfoViewModel
    {
        public string Email { get; set; }

        public bool HasRegistered { get; set; }

        public string LoginProvider { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PlaceId { get; set; }

        public bool EmailConfirmed { get; set; }
    }
}
