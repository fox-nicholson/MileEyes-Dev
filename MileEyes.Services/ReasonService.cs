using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    class ReasonService : IReasonService
    {
        public async Task<IEnumerable<Reason>> GetReasons()
        {
            return DatabaseService.Realm.All<Reason>();
        }

        public async Task<Reason> GetReason(string id)
        {
            return DatabaseService.Realm.ObjectForPrimaryKey<Reason>(id);            
        }

        public async Task<Reason> SaveReason(Reason r)
        {
            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                var reason = DatabaseService.Realm.CreateObject<Reason>();

                reason.Id = Guid.NewGuid().ToString();
                reason.Text = r.Text;

                transaction.Commit();
                transaction.Dispose();

                return reason;
            }
        }

        public async Task<Reason> DeleteReason(string id)
        {
            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                var reason = DatabaseService.Realm.ObjectForPrimaryKey<Reason>(id);

                DatabaseService.Realm.Remove(reason);

                transaction.Commit();
                transaction.Dispose();

                return reason;
            }
        }

        public async Task SetDefault(string id)
        {
            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                var reasons = DatabaseService.Realm.All<Reason>();

                foreach (var reason in reasons)
                {
                    reason.Default = reason.Id == id;
                }

                transaction.Commit();
                transaction.Dispose();
            }
        }
    }
}