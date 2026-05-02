using System.ComponentModel.DataAnnotations;

namespace AutenticationWeb.API.Models
{
    public class RegisterModel
    {
        [Required]       
        public string Username { get; set; }

        [Required]       
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
