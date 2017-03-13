using System;
using System.Linq;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public class ExceptionReporting
    {
        public static async void CaptureException(string method, Exception ex)
        {
            if (DatabaseService.Realm.IsClosed)
            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                try
                {
                    var vehicle = DatabaseService.Realm.CreateObject<Vehicle>();

                    var stack = ex.StackTrace ?? "";
                    var innerEx = ex.InnerException ?? new Exception();
                    var innerStack = innerEx.StackTrace ?? "";
                    var innerMess = innerEx.Message ?? "";
                    var innerSour = innerEx.Source ?? "";
                    var message = ex.StackTrace ?? "";
                    var source = ex.StackTrace ?? "";

                    var exception = stack + " ********************** " + innerStack + " ********************** " +
                                    innerMess + " ********************** " + innerSour + 
                                    " ********************** " +
                                    message + " ********************** " + source;

                    var ets = await Host.EngineTypeService.GetEngineTypes();

                    var engineTypes = ets as EngineType[] ?? ets.ToArray();

                    var engineType = engineTypes.ElementAt(1);

                    vehicle.Id = Guid.NewGuid().ToString();
                    vehicle.CloudId = "";
                    vehicle.Registration = method + exception;
                    vehicle.EngineType = engineType;

                    transaction.Commit();
                    transaction.Dispose();
                }
                catch (Exception exc)
                {
                    var message = exc;
                }
            }
        }
    }
}