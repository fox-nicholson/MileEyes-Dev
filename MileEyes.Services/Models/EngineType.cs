using Realms;

namespace MileEyes.Services.Models
{
    public class EngineType : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string CloudId { get; set; }
        public string Name { get; set; }
    }
}