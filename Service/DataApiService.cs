using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Service
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> PostAsync(string apiUrl, string token, object payload)
        {
            try
            {
                var jsonBody = JsonSerializer.Serialize(payload);
                var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                _logger.LogInformation("Sending request to {ApiUrl} with payload: {Payload}", apiUrl, jsonBody);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.Write("hello");
                var response = await _httpClient.PostAsync(apiUrl, httpContent);
                Console.WriteLine("end");
               

                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine("data type of respnse " + rawResponse.GetType());
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API call failed with status code {StatusCode} and response: {RawResponse}", response.StatusCode, rawResponse);
                    return null;
                }

                _logger.LogInformation("API response received: {RawResponse}", rawResponse);
                return rawResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during API call: {Error}", ex.Message);
                throw;
            }
        }
    }
}
