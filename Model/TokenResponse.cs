using System.Text.Json.Serialization;

namespace Server.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string Access_Token { get; set; }

        [JsonPropertyName("token_type")]
        public string Token_Type { get; set; }

        [JsonPropertyName("expires_in")]
        public int Expires_In { get; set; }

        [JsonPropertyName("refresh_token")]
        public string Refresh_Token { get; set; }
    }


}
