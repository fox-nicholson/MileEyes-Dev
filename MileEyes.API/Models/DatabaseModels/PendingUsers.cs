using System;

namespace MileEyes.API.Models.DatabaseModels
{
    public class PendingUsers
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public virtual Guid SubscriptionId { get; set; }
        public DateTimeOffset SignUpDate { get; set; }
    }
}