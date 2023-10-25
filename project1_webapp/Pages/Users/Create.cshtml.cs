using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using project1_webapp.Data;
using project1_webapp.Models;

namespace project1_webapp.Pages.Users
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public string UserPassword { get; set; }
        [BindProperty]
        public bool NameIsValid { get; set; } = true;
        [BindProperty]
        public bool PasswordIsValid { get; set; } = true;
        [BindProperty]
        public bool NameIsFree { get; set; } = true;

        private readonly project1_webapp.Data.project1_UserContext _context;

        public CreateModel(project1_webapp.Data.project1_UserContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public User User { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            (User.PasswordHash, User.Salt) = HashPassword(UserPassword);

            // check that username is valid, password is valid, username not already taken
            NameIsValid = CheckNameValidity(User.UserName);
            PasswordIsValid = CheckPasswordValidity(UserPassword);
            if (NameIsValid) {
                NameIsFree = await CheckIfNameIsFreeAsync(User.UserName);
            }

            if (!NameIsValid || !PasswordIsValid || !NameIsFree || _context.Users == null || User == null)
            {
                return Page();
            }

            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login");
        }

        // validate length =< 30 and length >= 5, also name can only contain alphanumeric values
        bool CheckNameValidity(string name) {
            return (name.Length <= 30 && name.Length >= 5 && name.All(char.IsLetterOrDigit));
        }

        // validate length <= 20 and length >= 5, also pass can only contain alphanumeric values and special chars
        bool CheckPasswordValidity(string pass) {
            var specialChars = """~`! @#$%^&*()_-+={[}]|\:;"'<,>.?/""".ToHashSet();
            return pass.Length <= 20 && pass.Length >= 5 && pass.All(x => (char.IsLetterOrDigit(x) || specialChars.Contains(x)));
        }
        
        // check database for copy of name
        async Task<bool> CheckIfNameIsFreeAsync(string name) {
            return !(await _context.Users.AnyAsync(x => x.UserName.ToLower() == name.ToLower()));
        }

        (byte[], byte[]) HashPassword(string password) {
            const int keySize = 64;
            const int iterations = 350000;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
            var salt = RandomNumberGenerator.GetBytes(keySize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                hashAlgorithm,
                keySize);
            return (hash, salt);
        }
    }
}
