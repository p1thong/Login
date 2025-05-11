using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Username { get; set; }

        public string? PasswordHash { get; set; }

        public string? DisplayName { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string? GoogleId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    }
} 