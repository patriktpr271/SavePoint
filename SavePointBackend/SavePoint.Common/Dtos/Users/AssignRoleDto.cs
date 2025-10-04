using System.ComponentModel.DataAnnotations;

namespace SavePoint.Common.Dtos.Users
{
    public class AssignRoleDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}