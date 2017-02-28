using Realms;

namespace MileEyes.Services.Models
{
    public class Reason : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Text { get; set; }
        public bool Default { get; set; }
    }
}