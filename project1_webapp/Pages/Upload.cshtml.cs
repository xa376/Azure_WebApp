using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Drawing;
using System.Net.Sockets;
using System.Drawing.Imaging;

// TODO
// create images out of objects and put them next to object text

namespace project1_webapp.Pages {
	public class UploadModel : PageModel {

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
            Console.WriteLine(HttpContext.Session.GetString("test"));
        }

        public async Task<IActionResult> OnPostAsync() {
            HttpContext.Session.SetString("test", "The test string was gotten.");

            // if upload button was pressed with a file verify valid file
            if (UploadFile != null && UploadFile.Length > 0 && (UploadFile.ContentType == "image/png" || UploadFile.ContentType == "image/jpeg")) {

				// required to access vision service
				ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(_configuration.GetConnectionString("AIKey"));
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
						
						
						var thumbnailStream = await visionClient.GenerateThumbnailInStreamAsync(800, 800, new MemoryStream(memoryStream.ToArray()), true);
						// converts to Image
						MemoryStream ms = new MemoryStream();
						await thumbnailStream.CopyToAsync(ms);

                        var analysis = await visionClient.AnalyzeImageInStreamAsync(new MemoryStream(ms.ToArray()), features);
						//visionResponse = analysis;
						HttpContext.Session.SetString("visionResponse", JsonConvert.SerializeObject(analysis));
                        System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(ms.ToArray()));
					
					// settings for boxes to draw around image objects
					Graphics graphics = Graphics.FromImage(image);
					Pen pen = new Pen(Color.Cyan, 8);
					Font font = new Font("Arial", 16);
					SolidBrush brush = new SolidBrush(Color.Black);

					// draws boxes and labels around image objects
					foreach (var detectedObject in analysis.Objects) {
						var r = detectedObject.Rectangle;
						Rectangle rect = new Rectangle(r.X, r.Y, r.W, r.H);
						graphics.DrawRectangle(pen, rect);
						graphics.DrawString(detectedObject.ObjectProperty, font, brush, r.X, r.Y);
					}

						using (var ms2 = new MemoryStream())
						{ 
							image.Save(ms2, ImageFormat.Jpeg);
							var base64String = Convert.ToBase64String(ms2.ToArray());
							imageDataWithObjectUrl = $"data:image/jpg;base64,{base64String}";
                        }

                        using (var ms3 = new MemoryStream())
                        {
                            var base64String = Convert.ToBase64String(ms.ToArray());
                            imageDataUrl = $"data:image/jpg;base64,{base64String}";
                        }
                        
                    };
					
				}
				
			}

			return Page();
		}
       
    }
}