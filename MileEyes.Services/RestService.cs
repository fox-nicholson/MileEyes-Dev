using System;
using System.Net.Http;

namespace MileEyes.Services
{
    class RestService
    {
        public static HttpClient Client { get; set; } = new HttpClient()
        {
            Timeout = new TimeSpan(0,0,30),
            BaseAddress = new Uri("http://mileeyesdevelopment.azurewebsites.net/")
        };
    }
}