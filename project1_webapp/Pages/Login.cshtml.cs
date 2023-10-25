using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project1_webapp.Models;
using System.Security.Cryptography;
using System.Text;

namespace project1_webapp.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public User User { get; set; } = default!;
        [BindProperty]
        public string LoginName { get; set; } = "";
        [BindProperty]
        public string LoginPassword { get; set; } = "";
        [BindProperty]
        public bool LoginFailed { get; set; } = false;

        private readonly project1_webapp.Data.project1_UserContext _context;

        // need this to make it choose this constructor (yes even though its the only one)
        [ActivatorUtilitiesConstructor]
        public LoginModel(project1_webapp.Data.project1_UserContext context) {
            _context = context;
        }

        public void OnGet()
        {
            
        }
        public async Task<IActionResult> OnPostAsync() {
            LoginFailed = !await AttemptLoginAsync();
            return Page();
        }
        async Task<bool> AttemptLoginAsync() {
            User realUser;

            try {
                realUser = await _context.Users.FirstAsync(x => x.UserName.ToLower() == LoginName.ToLower());
            } catch {
                return false;
            }

            var salt = realUser.Salt;

            var hashResult = HashPassword(LoginPassword, salt).Item1;

            if (Convert.ToBase64String(hashResult) == Convert.ToBase64String(realUser.PasswordHash)) {
                HttpContext.Session.SetString("LoggedIn", "true");
                HttpContext.Session.SetString("LoggedInName", realUser.UserName);
                return true;
            }

            return false;
            

        }

        (byte[], byte[]) HashPassword(string password, byte[] oldSalt) {
            const int keySize = 64;
            const int iterations = 350000;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
            var salt = RandomNumberGenerator.GetBytes(keySize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                oldSalt,
                iterations,
                hashAlgorithm,
                keySize);
            return (hash, salt);
        }
    }
}
