using Auth.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Auth.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(Constrains.MaxStringLength)]
        public string Login { get; set; }

        [DataType(DataType.Password)]
        [MaxLength(Constrains.MaxStringLength)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [MaxLength(Constrains.MaxStringLength)]
        public string Email { get; set; }
    }
}
