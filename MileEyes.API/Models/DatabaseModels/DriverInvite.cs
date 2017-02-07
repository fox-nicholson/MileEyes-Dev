using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class DriverInvite : Invite
    {
        public virtual Company Company { get; set; }
    }
}