using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ByteBank.Forum.App_Start.Identity
{
    public class ValidatePassword : IIdentityValidator<string>
    {
        public int RequiredLength { get; set; }
        public bool RequireSpecialCharacter { get; set; }
        public bool RequireLowerCaseCharacter { get; set; }
        public bool RequireUpperCaseCharacter { get; set; }
        public bool RequireDigit { get; set; }

        public async Task<IdentityResult> ValidateAsync(string item)
        {
           var errors = new List<string>();

            if (RequireSpecialCharacter && !ValidateRequiredSpecialCharacter(item))
                errors.Add("A senha deve conter caracteres especiais!");

            if (!ValidateRequiredLength(item))
                errors.Add($"A senha deve conter no mínimo {RequiredLength} caracteres.");

            if (RequireLowerCaseCharacter && !ValidateRquiredLowerCase(item))
                errors.Add($"A senha deve conter no mínimo uma letra minúscula.");

            if (RequireUpperCaseCharacter && !ValidateRequiredUpperCase(item))
                errors.Add($"A senha deve conter no mínimo uma letra maiúscula.");

            if (RequireDigit && !ValidateRequiredDigit(item))
                errors.Add($"A senha deve conter no mínimo um dígito.");

            if (errors.Any())
                return IdentityResult.Failed(errors.ToArray());
            else
                return IdentityResult.Success;
        }

        private bool ValidateRequiredLength(string password) => password.Length >= RequiredLength;

        private bool ValidateRequiredSpecialCharacter(string password) => Regex.IsMatch(password, @"[~`!@#$%^&*()+=|\\{}':;.,<>/?[\]""_-]");

        private bool ValidateRquiredLowerCase(string password) => password.Any(char.IsLower);

        private bool ValidateRequiredUpperCase(string password) => password.Any(char.IsUpper);

        private bool ValidateRequiredDigit(string password) => password.Any(char.IsDigit);
    }
}