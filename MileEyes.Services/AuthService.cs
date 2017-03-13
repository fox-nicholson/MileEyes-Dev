using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    public class AuthService : IAuthService
    {
        public bool Authenticated { get; private set; }

        public void Logout()
        {
            Authenticated = false;

            using (var transaction = DatabaseService.Realm.BeginWrite())
            {
                DatabaseService.Realm.RemoveAll();

                transaction.Commit();
                transaction.Dispose();
            }

            RestService.Client.DefaultRequestHeaders.Authorization = null;
        }

        public AuthService()
        {
            var authTokens = DatabaseService.Realm.All<AuthToken>();

            if (authTokens.Any())
            {
                var authToken = authTokens.FirstOrDefault();
                RestService.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    authToken.access_token);

                Authenticated = true;
            }
            else
            {
                Authenticated = false;
            }
        }

        public async Task<AuthResponse> Authenticate(string email, string password)
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", email),
                new KeyValuePair<string, string>("password", password)
            });

            try
            {
                var response = await RestService.Client.PostAsync("token", data);

                if (!response.IsSuccessStatusCode)
                {
                    var result =
                        JsonConvert.DeserializeObject<AuthResponse>(await response.Content.ReadAsStringAsync());
                    result.Success = false;
                    return result;
                }

                var tokenResult =
                    JsonConvert.DeserializeObject<AuthResponse>(await response.Content.ReadAsStringAsync());

                // Write the access token to the database for use in future
                using (var transaction = DatabaseService.Realm.BeginWrite())
                {
                    // Remove any tokens that already exist
                    DatabaseService.Realm.RemoveAll<AuthToken>();

                    // Create new Realm database object
                    var authToken = DatabaseService.Realm.CreateObject<AuthToken>();

                    // Setup the access tokens properties
                    authToken.access_token = tokenResult.access_token;
                    authToken.expires = tokenResult.expires;
                    authToken.expires_in = tokenResult.expires_in;
                    authToken.token_type = tokenResult.token_type;
                    authToken.issued = tokenResult.issued;
                    authToken.userName = tokenResult.userName;

                    // Commit the transaction
                    transaction.Commit();
                    transaction.Dispose();
                }

                // Set the request headers bearer token to the access token.
                RestService.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    tokenResult.access_token);

                // Obvious
                Authenticated = true;

                // Set the token to successful
                tokenResult.Success = true;

                await Host.EngineTypeService.Sync();
                await Host.VehicleTypeService.Sync();
                await Host.VehicleService.Sync();
                await Host.CompanyService.Sync();
                Host.JourneyService.Sync();

                // Return the token
                return tokenResult;
            }
            catch (Exception ex)
            {
                return new AuthResponse()
                {
                    error = ex.Message
                };
            }
        }

        public async Task<RegisterResponse> Register(string firstName, string lastName, string email, string password,
            string confirmPassword,
            string addressPlaceId)
        {
            var data = new StringContent(JsonConvert.SerializeObject(new RegisterBindingModel()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                PlaceId = addressPlaceId
            }), Encoding.UTF8, "application/json");

            try
            {
                var response = await RestService.Client.PostAsync("api/Account/Register", data);

                if (response == null) return new RegisterResponse()
                {
                    Error = true
                };

                if (response.IsSuccessStatusCode) return new RegisterResponse();

                var resultString = await response.Content.ReadAsStringAsync();

                var result =
                    JsonConvert.DeserializeObject<RegisterResponse>(resultString);
                result.Error = true;

                if (result.ModelState._ == null) return result;

                var temp = result.ModelState._[0];

                result.ModelState._ = new[] {temp};

                return result;
            }
            catch (Exception ex)
            {
                return new RegisterResponse()
                {
                    Error = true,
                    Message = ex.Message
                };
            }
        }
    }
}