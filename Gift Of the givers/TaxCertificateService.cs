using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GiftOfTheGivers
{
    public class TaxCertificateService
    {
        private readonly HttpClient _httpClient;

        public TaxCertificateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateCertificateAsync(string donorName, decimal amount)
        {
            // Create the payload to send to the Azure Function
            var payload = new { donorName = donorName, donationAmount = amount };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call the local Azure Function URL (Change this to your Azure URL after deployment)
            var response = await _httpClient.PostAsync("http://localhost:7071/api/GenerateTaxCertificate", content);

            // Throw an error if the function fails
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Parse the JSON response to extract the CertificateNumber
            using (JsonDocument doc = JsonDocument.Parse(responseString))
            {
                if (doc.RootElement.TryGetProperty("CertificateNumber", out JsonElement certElement))
                {
                    return certElement.GetString() ?? $"GOTG-{System.DateTime.UtcNow:yyyyMMdd}-FALLBACK";
                }
            }

            // Fallback if parsing fails
            return $"GOTG-{System.DateTime.UtcNow:yyyyMMdd}-FALLBACK";
        }
    }
}