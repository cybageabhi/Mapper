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
                httpContent.Headers.ContentType.CharSet = "utf-8";

                _logger.LogInformation("Sending request to {ApiUrl} with payload: {Payload}", apiUrl, jsonBody);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(apiUrl, httpContent);

                if (response.Content == null || response.Content.Headers.ContentLength == 0)
                {
                    _logger.LogWarning("Empty response received from API: {ApiUrl}");
                    return null;
                }

                // Step 1: Print the raw response content before reading it as a string
                try
                {
                    // Read the raw response content as a stream and log it
                    var rawContent = await response.Content.ReadAsStreamAsync();
                    using (var reader = new StreamReader(rawContent))
                    {
                        string rawResponseAsString = await reader.ReadToEndAsync();
                        _logger.LogInformation("Raw Response Content: {RawResponse}", rawResponseAsString);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error reading raw response content: {Error}", ex.Message);
                }

                // Step 2: Now read the response as a string
                string rawResponse = string.Empty;
                try
                {
                    rawResponse = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error reading response content: {Error}", ex.Message);
                    return null;
                }

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
