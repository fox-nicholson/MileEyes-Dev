using Realms;

namespace MileEyes.Services
{
    class DatabaseService
    {
        public static Realm Realm { get; set; } = Realm.GetInstance(new RealmConfiguration()
        {
            SchemaVersion = 1
        });
    }
}