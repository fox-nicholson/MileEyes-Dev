using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels;
using Newtonsoft.Json;

namespace MileEyes.Services.Models
{
    public class UserInfoResponse : RestResponse
    {
        public UserInfoViewModel Result { get; set; }
        public UserInfoModelstate ModelState { get; set; }
    }

    public class UserInfoModelstate
    {
        [JsonProperty("model.FirstName")]
        public string[] FirstName { get; set; }
        [JsonProperty("model.LastName")]
        public string[] LastName { get; set; }
        [JsonProperty("model.Email")]
        public string[] Email { get; set; }
        [JsonProperty("model.PlaceId")]
        public string[] PlaceId { get; set; }
    }
}
