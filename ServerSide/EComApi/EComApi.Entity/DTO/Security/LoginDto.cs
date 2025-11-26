using System.ComponentModel.DataAnnotations;

namespace EComApi.Entity.DTO.Security
{
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
