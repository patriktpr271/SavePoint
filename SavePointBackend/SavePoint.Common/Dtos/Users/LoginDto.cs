using System.ComponentModel.DataAnnotations;

namespace SavePoint.Common.Dtos.Users
{
    public class LoginDto
    {
        [Required]
        [Display(Name = "Email or Username")]
        public string EmailOrUsername { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; } = false;
    }
}