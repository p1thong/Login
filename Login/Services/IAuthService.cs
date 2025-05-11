using Login.Models;

namespace Login.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> AuthenticateGoogleUserAsync(string idToken);
        Task<AuthResponseDTO> RegisterUserAsync(RegisterDTO model);
        Task<AuthResponseDTO> LoginAsync(LoginDTO model);
        string GenerateJwtToken(User user);
    }
} 