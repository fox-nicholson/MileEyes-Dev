using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MileEyes.PublicModels.Company;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    public class CompanyService : ICompanyService
    {
        public static bool CompanySyncing;

        public event EventHandler SyncFailed = delegate { };

        public async Task<IEnumerable<Company>> GetCompanies()
        {
            return DatabaseService.Realm.All<Company>();
        }

        public async Task<Company> GetCompany(string id)
        {
            return DatabaseService.Realm.All<Company>().FirstOrDefault(c => c.Id == id);
        }

        public async Task Sync()
        {
            if (!TrackerService.IsTracking && !JourneyService.JourneySyncing && !VehicleService.VehicleSyncing && !VehicleTypeService.VehicleTypeSyncing && !EngineTypeService.EngineTypeSyncing)
            {
                try
                {
                    CompanySyncing = true;

                    RestService.Client.Timeout = new TimeSpan(0, 0, 30);

                    var response = await RestService.Client.GetAsync("api/DriverCompanies/");

                    if (response == null)
                    {
                        CompanySyncing = false;
                        SyncFailed?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        CompanySyncing = false;
                        SyncFailed?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    var result =
                        JsonConvert.DeserializeObject<IEnumerable<CompanyViewModel>>(
                            await response.Content.ReadAsStringAsync()).Select(c => new Company()
                            {
                                CloudId = c.Id.ToString(),
                                Name = c.Name,
                            });

                    var existing = await GetCompanies();

                    var existingEnumerable = existing as Company[] ?? existing.ToArray();

                    var resultEnumerable = result as Company[] ?? result.ToArray();

                    foreach (var company in resultEnumerable)
                    {
                        var currentCompany = existingEnumerable.FirstOrDefault(c => c.CloudId == company.CloudId);

                        if (currentCompany == null)
                        {
                                using (var transaction = DatabaseService.Realm.BeginWrite())
                                {
                                    var newCompany = DatabaseService.Realm.CreateObject<Company>();

                                    newCompany.CloudId = company.CloudId;
                                    newCompany.Id = Guid.NewGuid().ToString();

                                    newCompany.Name = company.Name;

                                    transaction.Commit();
                                    transaction.Dispose();
                                }
                        }
                        else
                        {
                                using (var transaction = DatabaseService.Realm.BeginWrite())
                                {
                                    var existingCompany = DatabaseService.Realm.Find<Company>(currentCompany.Id);

                                    existingCompany.Name = company.Name;

                                    transaction.Commit();
                                    transaction.Dispose();
                                }
                        }
                    }

                    foreach (var company in existingEnumerable)
                    {
                        var currentCompany = resultEnumerable.FirstOrDefault(c => c.CloudId == company.CloudId);

                        if (currentCompany != null) continue;

                        var existingCompany = await Host.CompanyService.GetCompany(company.Id);

                        DatabaseService.Realm.Remove(existingCompany);
                    }
                    CompanySyncing = false;
                } catch (Exception)
                {
                    CompanySyncing = false;
                    SyncFailed?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }

        public async Task SetDefault(string id)
        {
                using (var transaction = DatabaseService.Realm.BeginWrite())
                {
                    var companies = DatabaseService.Realm.All<Company>();

                    if (!(companies.Where(d => d.Default).Any()) && companies.Any())
                    {
                        companies.FirstOrDefault().Default = true;
                        transaction.Commit();
                        transaction.Dispose();
                        return;
                    }

                    foreach (var company in companies)
                    {
                        company.Default = company.Id == id;
                    }

                    transaction.Commit();
                    transaction.Dispose();
            }
        }
    }
}