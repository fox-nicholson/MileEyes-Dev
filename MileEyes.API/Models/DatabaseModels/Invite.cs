using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class Invite
    {
        public Guid Id { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string Email { get; set; }
    }
}