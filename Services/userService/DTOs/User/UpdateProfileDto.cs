using System.ComponentModel.DataAnnotations;

namespace userService.DTOs.User
{
    public class UpdateProfileDto
    {
        [Required]
        public string Username { get; set; } = null!;
    }
}
