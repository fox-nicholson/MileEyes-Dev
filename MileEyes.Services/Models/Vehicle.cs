using Realms;

namespace MileEyes.Services.Models
{
    public class Vehicle : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string CloudId { get; set; }
        public string Registration { get; set; }
        public EngineType EngineType { get; set; }
        public bool Default { get; set; }
        public bool MarkedForDeletion { get; set; }
        public bool Deleted { get; set; }
    }
}