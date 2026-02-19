using System.ComponentModel.DataAnnotations;

namespace AuthenticationAuthorizationDemo.Models
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me? (persistent login)")]
        public bool RememberMe { get; set; } = false;  // default false → session cookie
    }
}