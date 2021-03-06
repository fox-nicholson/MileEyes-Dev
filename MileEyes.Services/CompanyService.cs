﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels.Company;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    class CompanyService : ICompanyService
    {
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
            try
            {
                var response = await RestService.Client.GetAsync("api/Companies/");

                if (!response.IsSuccessStatusCode)
                {
                    SyncFailed?.Invoke(this, EventArgs.Empty);
                    return;
                }

                var result =
                    JsonConvert.DeserializeObject<IEnumerable<CompanyViewModel>>(await response.Content.ReadAsStringAsync()).Select(c => new Company()
                    {
                        CloudId = c.Id.ToString(),
                        Name = c.Name,
                        Personal = c.Personal
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
                            newCompany.Personal = company.Personal;

                            transaction.Commit();
                        }
                    }
                    else
                    {
                        using (var transaction = DatabaseService.Realm.BeginWrite())
                        {
                            var existingCompany = DatabaseService.Realm.ObjectForPrimaryKey<Company>(currentCompany.Id);
                            
                            existingCompany.Name = company.Name;
                            existingCompany.Personal = company.Personal;

                            transaction.Commit();
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
            }
            catch
            {
                SyncFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task SetDefault(string id)
        {
            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                var companies = DatabaseService.Realm.All<Company>();

                foreach (var company in companies)
                {
                    company.Default = company.Id == id;
                }

                transaction.Commit();
            }
        }
    }
}
