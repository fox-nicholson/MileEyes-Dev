using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace MileEyes.API.Helpers
{
    public class HttpHelper
    {
        private static HttpClient _client = new HttpClient();

        public static async Task<string> FileGetContents(string url)
        {
            var uri = new Uri(url);

            try
            {
                var response = await _client.GetAsync(uri);

                if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}