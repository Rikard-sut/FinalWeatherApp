using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Helper
{
    public class ApiCaller
    {
        public static async Task<ApiResponse> Get(string url, string authId = null)
        {
            using(var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(authId))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", authId);
                }
                var request = await client.GetAsync(url);
                if (request.IsSuccessStatusCode)
                {
                    return new ApiResponse { Response = await request.Content.ReadAsStringAsync() };
                }
                else
                {
                    return new ApiResponse { ErrorMessage = request.ReasonPhrase };
                }
            }
        } 

    }
    public class ApiResponse
    {
        public bool Success => ErrorMessage == null; //Om errorMessage är null sätt success till true. annars false.
        public string ErrorMessage { get; set; }
        public string Response { get; set; }
    }
}
