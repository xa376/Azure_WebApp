using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace project1_webapp.Pages {
	public class UploadModel : PageModel {

		[BindProperty]
		public IFormFile UploadFile { get; set; }

		private readonly ILogger<UploadModel> _logger;

		public UploadModel(ILogger<UploadModel> logger) {
			_logger = logger;
		}

		public void OnGet() {
		}

		public async Task<IActionResult> OnPostAsync() {

			if (UploadFile != null && UploadFile.Length > 0) {

				// Specify the folder where you want to store uploaded files
				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

				// Create the folder if it doesn't exist
				Directory.CreateDirectory(uploadsFolder);

				// Generate a unique file name (you can customize this)
				var uniqueFileName = Guid.NewGuid().ToString() + "_" + UploadFile.FileName;

				// Combine the folder path and file name
				var filePath = Path.Combine(uploadsFolder, uniqueFileName);

				// Save the uploaded file
				using (var stream = new FileStream(filePath, FileMode.Create)) {
					await UploadFile.CopyToAsync(stream);
				}

				// Redirect to a success page or display a success message
				return Page();//RedirectToPage("UploadSuccess");
			}

			// Handle invalid file uploads
			return Page();
		}

	}
}