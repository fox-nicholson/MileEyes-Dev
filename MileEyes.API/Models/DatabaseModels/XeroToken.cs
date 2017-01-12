using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class XeroToken : AccountingToken
    {
        public string UserId { get; set; }
        public string OrganisationId { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string TokenKey { get; set; }
        public string TokenSecret { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public string Session { get; set; }
        public DateTimeOffset SessionExpiresAt { get; set; }

        public bool HasExpired => ExpiresAt > DateTime.Now;

        public bool HasSessionExpired => SessionExpiresAt > DateTime.Now;
    }
}