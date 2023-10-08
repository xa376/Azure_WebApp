using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Drawing;


namespace project1_webapp.Pages {
	public class UploadModel : PageModel {

		[BindProperty]
		public ImageAnalysis? visionResponse { get; set; }

		[BindProperty]
		public IFormFile UploadFile { get; set; }

		[BindProperty]
		public string imageDataUrl { get; set; }

		private readonly IMemoryCache _cache;
		private readonly ILogger<UploadModel> _logger;
		private readonly IConfiguration _configuration;

		public UploadModel(ILogger<UploadModel> logger, IConfiguration configuration, IMemoryCache cache) {
			_logger = logger;
			_configuration = configuration;
			_cache = cache;
		}

		public void OnGet() {

			// check for image
			if (_cache.TryGetValue("ImageUrl", out string cachedImageDataUrl)) {
				_cache.Remove("ImageUrl");
				imageDataUrl = cachedImageDataUrl;
			}

			// check for image analysis results
			if (TempData["VisionResponse"] != null) {
				visionResponse = JsonConvert.DeserializeObject<ImageAnalysis?>((string)TempData["VisionResponse"]);
			}

		}

		public async Task<IActionResult> OnPostAsync() {

			// if upload button was pressed with a file verify valid file
			if (UploadFile != null && UploadFile.Length > 0 && (UploadFile.ContentType == "image/png" || UploadFile.ContentType == "image/jpeg")) {
				
				// required to access vision service
				ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(_configuration["VisionKey"]);
				var visionClient = new ComputerVisionClient(credentials) { Endpoint = _configuration["VisionEndpoint"] };

				// all features to get from vision service
				List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>() {
					VisualFeatureTypes.Description,
					VisualFeatureTypes.Tags,
					VisualFeatureTypes.Categories,
					VisualFeatureTypes.Brands,
					VisualFeatureTypes.Objects,
					VisualFeatureTypes.Adult
				};

				// TODO resize file if too large
				// code within using analyzes image, draws boxes on found objects, and saves results
				using (var imageData = UploadFile.OpenReadStream()) {

					// analyzes image
					var analysis = await visionClient.AnalyzeImageInStreamAsync(imageData, features);

					// converts to Image
					System.Drawing.Image image = System.Drawing.Image.FromStream(UploadFile.OpenReadStream());

					// settings for boxes to draw around image objects
					Graphics graphics = Graphics.FromImage(image);
					Pen pen = new Pen(Color.Cyan, 3);
					Font font = new Font("Arial", 16);
					SolidBrush brush = new SolidBrush(Color.Black);

					// draws boxes and labels around image objects
					foreach (var detectedObject in analysis.Objects) {
						var r = detectedObject.Rectangle;
						Rectangle rect = new Rectangle(r.X, r.Y, r.W, r.H);
						graphics.DrawRectangle(pen, rect);
						graphics.DrawString(detectedObject.ObjectProperty, font, brush, r.X, r.Y);
					}

					// attempts to save the new image and turn it into a url for display in page
					using (var memoryStream = new MemoryStream()) {
						try {
							image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
						} catch {}
							var bytes = memoryStream.ToArray();
							var base64String = Convert.ToBase64String(bytes);
							imageDataUrl = $"data:image/jpg;base64,{base64String}";
					}

					// option to ensure timely deletion from cache
					var cacheEntryOptions = new MemoryCacheEntryOptions {
						AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
					};

					// adds visionResponse to TempData, and adds imageurl to cache because too large for TempData
					_cache.Set("ImageUrl", imageDataUrl, cacheEntryOptions);
					TempData["VisionResponse"] = JsonConvert.SerializeObject(analysis);

				}

				return RedirectToPage("/Upload");
			}

			return Page();
		}

	}
}