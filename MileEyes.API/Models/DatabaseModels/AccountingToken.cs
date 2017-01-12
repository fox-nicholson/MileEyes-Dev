using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class AccountingToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual Company Company { get; set; }
    }
}