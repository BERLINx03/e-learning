using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ELearning.Services
{
    public class RecommendationService
    {
        private readonly HttpClient _client;

        public RecommendationService(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> GetRecommendationsAsync(int userId)
        {
            var payload = new { user_id = userId };
            var jsonPayload = JsonConvert.SerializeObject(payload);

            var response = await _client.PostAsync(
                "http://127.0.0.1:5000/recommend", // Python service URL
                new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            );

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }
    }
}
