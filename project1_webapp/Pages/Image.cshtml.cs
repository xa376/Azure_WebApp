using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Protocol;
using System.Net.Http.Headers;
using System.Text;
using project1_webapp.Models;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Http;
using Azure.AI.TextAnalytics;

namespace project1_webapp.Pages {
    public class ImageModel : PageModel {

        [BindProperty]
        public string userText { get; set; }
        private readonly IConfiguration _configuration;

        public ImageModel(IConfiguration configuration) {
            _configuration = configuration;
        }

        public void OnGet() {

        }

        public async Task<IActionResult> OnPostAsync() {

            if (userText != null) {

                // set up API request for image generation
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var request = new HttpRequestMessage(HttpMethod.Post, _configuration["DalleEndpoint"])) {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetConnectionString("OpenAIKey"));
                    string serializedImageRequest = JsonConvert.SerializeObject(new DalleImageRequest() { prompt = userText });
                    request.Content = new StringContent(serializedImageRequest,
                                    Encoding.UTF8,
                                    "application/json");
                    var response = await client.SendAsync(request);

                    // if image generation request succeeded save result, else save flag denoting rate limit hit
                    if (response.IsSuccessStatusCode) {
                        var dalleImageResult = JsonConvert.DeserializeObject<DalleImageResult>(await response.Content.ReadAsStringAsync());
                        HttpContext.Session.SetString("DalleImageURL", dalleImageResult.data[0]["url"]);
                    } else {
                        HttpContext.Session.SetString("RateLimitHit", "true");
                    }

                }

            }

            return Page();
        }
    }
}
