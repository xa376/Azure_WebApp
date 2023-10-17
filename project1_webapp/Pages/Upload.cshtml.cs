using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Drawing;
using System.Net.Sockets;
using System.Drawing.Imaging;

namespace project1_webapp.Pages {
	public class UploadModel : PageModel {

		[BindProperty]
		public ImageAnalysis? visionResponse { get; set; }

		[BindProperty]
		public IFormFile UploadFile { get; set; }

		[BindProperty]
		public string imageDataUrl { get; set; }
        [BindProperty]
        public string imageDataWithObjectUrl { get; set; }
        private readonly IMemoryCache _cache;
		private readonly ILogger<UploadModel> _logger;
		private readonly IConfiguration _configuration;

		public UploadModel(ILogger<UploadModel> logger, IConfiguration configuration, IMemoryCache cache) {
			_logger = logger;
			_configuration = configuration;
			_cache = cache;
		}

		public void OnGet() {

		}

		public async Task<IActionResult> OnPostAsync() {

			// if upload button was pressed with a file verify valid file
			if (UploadFile != null && UploadFile.Length > 0 && (UploadFile.ContentType == "image/png" || UploadFile.ContentType == "image/jpeg")) {
				
				// required to access vision service
				ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(_configuration["AIKey"]);
				var visionClient = new ComputerVisionClient(credentials) { Endpoint = _configuration["AIEndpoint"] };

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
					using (var memoryStream = new MemoryStream())
					{
						imageData.CopyTo(memoryStream);
						// analyzes image
						
						var analysis = await visionClient.AnalyzeImageInStreamAsync(new MemoryStream(memoryStream.ToArray()), features);
					
						var thumbnailStream = await visionClient.GenerateThumbnailInStreamAsync(400, 400, new MemoryStream(memoryStream.ToArray()), true);
						// converts to Image
						
						System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(memoryStream.ToArray()));
					
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
						
						using (var ms = new MemoryStream()) {
                            new Bitmap(image, 400, 400).Save(ms, ImageFormat.Jpeg);
                            var base64String = Convert.ToBase64String(ms.ToArray());
                            imageDataWithObjectUrl = $"data:image/jpg;base64,{base64String}";
                        }

                        using (var ms = new MemoryStream())
                        {
                            thumbnailStream.CopyTo(ms);
                            var base64String = Convert.ToBase64String(ms.ToArray());
                            imageDataUrl = $"data:image/jpg;base64,{base64String}";
                        }
                        visionResponse = analysis;
                    }

                    
                  
                    
					// attempts to save the new image and turn it into a url for display in page
					//using (var memoryStream = new MemoryStream())
					//{
					//	try
					//	{
					//		image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
					//	}
					//	catch { }
					//	var bytes = memoryStream.ToArray();

					//}

					// option to ensure timely deletion from cache

					// adds visionResponse to TempData, and adds imageurl to cache because too large for TempData
					;
					
				}
				
			}

			return Page();
		}
       
    }
}