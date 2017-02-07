using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
