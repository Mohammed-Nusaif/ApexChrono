namespace EComApi.Entity.DTO.User
{
    public class UserDto
    {
        public string Id { get; set; } = default!;
        public string? FullName { get; set; }
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
