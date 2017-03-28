using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class PasswordReset
    {
        public Guid Id { get; set; }

        public string Email { get; set; 
}
        public DateTimeOffset Date { get; set; }
    }
}