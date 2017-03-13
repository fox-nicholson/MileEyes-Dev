﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MileEyes.Services
{
    public class HttpHelper : IHttpHelper
    {
        private HttpClient _client;

        public async Task<string> FileGetContents(string url)
        {
            _client = new HttpClient();
            var uri = new Uri(url);
            
            var response = await _client.GetAsync(uri);

            if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();
            

            return null;
        }
    }
}