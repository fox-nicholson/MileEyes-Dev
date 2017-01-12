using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services.Models;
using Realms;

namespace MileEyes.Services
{
    class DatabaseService
    {
        public static Realm Realm { get; set; } = Realm.GetInstance(new RealmConfiguration()
        {
            SchemaVersion = 1,
            MigrationCallback = (migration, oldSchemaVersion) =>
            {

            }
        });
    }
}
