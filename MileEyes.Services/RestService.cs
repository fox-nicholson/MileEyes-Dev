using System;
using System.Net.Http;

namespace MileEyes.Services
{
    class RestService
    {
        public static HttpClient Client { get; set; } = new HttpClient()
        {
            BaseAddress = new Uri("http://mileyesapi.azurewebsites.net/")
        };
    }
}