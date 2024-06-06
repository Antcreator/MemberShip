using System.ComponentModel.DataAnnotations;

namespace MemberShip.Models
{
    public class RegisterModel
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }

        [Required]
        [RegularExpression("^(admin|user)$", ErrorMessage = "Role must be either 'admin' or 'user'.")]
        public required string Role { get; set; }
    }
}
