namespace Login.Models
{
    public class GoogleAuthDTO
    {
        public string IdToken { get; set; } = string.Empty;
    }

    public class LoginDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserDTO User { get; set; } = new UserDTO();
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
} 