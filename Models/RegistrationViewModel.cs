using System.ComponentModel.DataAnnotations;

namespace iBorrow.Models
{
    public class RegistrationViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Re-enter Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}