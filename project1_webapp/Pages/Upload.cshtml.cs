using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel;

namespace project1_webapp.Pages {
	public class UploadModel : PageModel {

		[BindProperty]
		public IFormFile UploadFile { get; set; }

		private readonly ILogger<UploadModel> _logger;

		public UploadModel(ILogger<UploadModel> logger) {
			_logger = logger;
		}

		// no result yet, let them upload
		public void OnGet() {
			//FileUploaded = JsonSerializer.Deserialize<IFormFile>(TempData["UploadedFile"] as string);

			if (TempData["UploadedFile"] != null) {
				_logger.LogInformation("passed success");
				Console.WriteLine(TempData["UploadedFile"]);
			}
		}

		public async Task<IActionResult> OnPostAsync() {

			if (UploadFile != null && UploadFile.Length > 0) {

				// Specify the folder where you want to store uploaded files
				//var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

				// Create the folder if it doesn't exist
				//Directory.CreateDirectory(uploadsFolder);

				// Generate a unique file name (you can customize this)
				//var uniqueFileName = Guid.NewGuid().ToString() + "_" + UploadFile.FileName;

				// Combine the folder path and file name
				//var filePath = Path.Combine(uploadsFolder, uniqueFileName);

				// Save the uploaded file
				//using (var stream = new FileStream(filePath, FileMode.Create)) {
				//	await UploadFile.CopyToAsync(stream);
				//}

				// send file for processing
				// retrieve and upload page, passing returned json
				//TempData["UploadedFile"] = "Testing Transfer";

				using (var httpClient = new HttpClient()) {

					var queryParams = new Dictionary<string, string> { 
						{ "visualFeatures", "Description,Tags,Objects" } // Replace with the desired features.
					};

					var uriBuilder = new UriBuilder(@"https://multiaiservices-xh1.cognitiveservices.azure.com/vision/v3.1/analyze");
					uriBuilder.Query = string.Join("&", queryParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
					//var requestString = "{\"authCode\": \"0000000001Nk1EEhZ3pZ73z700271891\" }";
					httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "79788f0317cd43c7931dbd5affb63db6");
					// Setup the HttpClient and make the call and get the relevant data.
					//httpClient.BaseAddress = new Uri("https://multiaiservices-xh1.cognitiveservices.azure.com");
					//string theUrl = @"https://multiaiservices-xh1.cognitiveservices.azure.com/vision/v3.1/analyze";
					//var request = new HttpRequestMessage(HttpMethod.Post, $"/vision/v3.1/analyze");
					var content = new MultipartFormDataContent();
					content.Add(new StreamContent(UploadFile.OpenReadStream()), "Image");
					//request.Content = content;
					
					var response = await httpClient.PostAsync(uriBuilder.Uri, content);
					var responseContent = await response.Content.ReadAsStringAsync();
					dynamic theObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
					Console.WriteLine(JsonConvert.SerializeObject(response.Headers.ToList()));
					Console.WriteLine(theObj);

					foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(theObj)) {
						Console.WriteLine("PROP: " + prop.Name);
					}
					Console.WriteLine(theObj.description["captions"][0]["text"]);

					if (response.IsSuccessStatusCode) {
						Console.WriteLine("Successful");
						Console.WriteLine(responseContent);
					} else {
						Console.WriteLine("Not successful");
					}

					// TODO
					// now place data in object
					// pass data to next page for display
				}

				// Redirect to a success page or display a success message
				//return RedirectToPage("/Upload", new { FileUploaded = true });
				return Page();
			}

			// Handle invalid file uploads
			return Page();
		}

	}
}