using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Server.Models;

namespace Server.Service
{
    public class TokenService
    {
        private readonly HttpClient _httpClient;

        public TokenService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetTokenAsync(string apiUrl, string username, string password)
        {
            try
            {
                // Prepare form-encoded data dynamically
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", username),  
                    new KeyValuePair<string, string>("password", password) 
                });

                Console.WriteLine("Sending request with content:");
                foreach (var pair in content.Headers)
                {
                    Console.WriteLine($"{pair.Key}: {string.Join(",", pair.Value)}");
                }

                var requestBody = await content.ReadAsStringAsync();
                Console.WriteLine($"Request Body: {requestBody}");

                var response = await _httpClient.PostAsync(apiUrl, content);

                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Body: {responseBody}");

                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
                if (tokenResponse != null)
                {
                    Console.WriteLine("i am token "+tokenResponse.Access_Token);
                    return tokenResponse.Access_Token;
                }
                else
                {
                    throw new Exception("Token response deserialization failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                throw;
            }
        }
    }

}
