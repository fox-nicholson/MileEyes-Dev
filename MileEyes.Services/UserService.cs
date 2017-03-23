using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    class UserService : IUserService
    {
        public async Task<UserInfoResponse> UpdateDetails(string firstName, string lastName, string email,
            string addressPlaceId)
        {
            var data = new StringContent(JsonConvert.SerializeObject(new UserInfoBindingModel()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
            }), Encoding.UTF8, "application/json");

            try
            {
                RestService.Client.Timeout = new TimeSpan(0, 0, 30);

                var response = await RestService.Client.PostAsync("account/userinfo", data);

                if (response == null) return new UserInfoResponse()
                {
                    Error = true
                };

                if (response.IsSuccessStatusCode) return new UserInfoResponse();
                var errorResult =
                    JsonConvert.DeserializeObject<UserInfoResponse>(await response.Content.ReadAsStringAsync());

                errorResult.Error = true;

                return errorResult;
            }
            catch (Exception ex)
            {
                return new UserInfoResponse()
                {
                    Error = true,
                    Message = ex.Message
                };
            }
        }

        public async Task<UserInfoResponse> GetDetails()
        {
            try
            {
                RestService.Client.Timeout = new TimeSpan(0, 0, 30);

                var response = await RestService.Client.GetAsync("api/account/userinfo");

                if (response == null) return new UserInfoResponse()
                {
                    Error = true
                };

                if (!response.IsSuccessStatusCode)
                {
                    var errorResult =
                        JsonConvert.DeserializeObject<UserInfoResponse>(await response.Content.ReadAsStringAsync());

                    errorResult.Error = true;

                    return errorResult;
                }

                var result = JsonConvert.DeserializeObject<UserInfoViewModel>(await response.Content.ReadAsStringAsync());

                return new UserInfoResponse()
                {
                    Result = result
                };
            } catch (Exception)
            {
                return new UserInfoResponse();
            }
        }
    }
}