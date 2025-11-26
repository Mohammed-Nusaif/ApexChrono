namespace EComApi.Entity.DTO.User
{
    public class CreateUserDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? FullName { get; set; }
        public List<string>? Roles { get; set; }
    }
}
