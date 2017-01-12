using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MileEyes.Services.Models
{
    public class RegisterResponse : RestResponse
    {
        public RegisterModelstate ModelState { get; set; }
    }

    public class RegisterModelstate
    {
        [JsonProperty("model.FirstName")]
        public string[] FirstName { get; set; }
        [JsonProperty("model.LastName")]
        public string[] LastName { get; set; }
        [JsonProperty("model.Email")]
        public string[] Email { get; set; }
        [JsonProperty("model.Password")]
        public string[] Password { get; set; }
        [JsonProperty("model.PlaceId")]
        public string[] PlaceId { get; set; }
    }
}
