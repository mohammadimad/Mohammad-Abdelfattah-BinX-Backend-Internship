using System.ComponentModel.DataAnnotations;

namespace Day03.DTO
{
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
