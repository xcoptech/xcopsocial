using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PageDemo1.Controllers
{
    public class OpenAIService
    {
        private readonly string _apiKey;

        public OpenAIService(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new ArgumentException("OpenAI API key is not configured. Please set it in appsettings.json or environment variables.");
            }
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);

            var generatedText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return generatedText ?? string.Empty;
        }
    }
}
