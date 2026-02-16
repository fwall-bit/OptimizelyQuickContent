using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OptimizelyQuickContent {
    public class ConnectionService {

        private readonly HttpClient _httpClient;

        public ConnectionService() {
            _httpClient = new();
        }

        public async Task<string> GetAccessTokenAsync(string baseUrl, string clientId, string clientSecret) {
            var requestBody = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            ]);

            var response = await _httpClient.PostAsync(
                $"{baseUrl.TrimEnd('/')}/api/episerver/connect/token", requestBody);

            await EnsureSuccessAsync(response);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("access_token").GetString()?.Trim()
                ?? throw new InvalidOperationException("Failed to retrieve access token.");

            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Received an empty access token.");

            return token;
        }

        public async Task<string> CreateContentAsync(string baseUrl, string accessToken, string jsonBody) {
            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{baseUrl.TrimEnd('/')}/api/episerver/v3.0/contentmanagement") {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);

            return await response.Content.ReadAsStringAsync();
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response) {
            if (!response.IsSuccessStatusCode) {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"{(int)response.StatusCode} {response.ReasonPhrase}\n{body}");
            }
        }
    }
}
