using System.ComponentModel.DataAnnotations;


namespace EComApi.Entity.DTO.Security
{
    public class RequestDto
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

    }
}
