using Google.Apis.Auth;
using Login.Data;
using Login.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Login.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDTO> AuthenticateGoogleUserAsync(string idToken)
        {
            try
            {
                // Xác thực token từ Google
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                // Kiểm tra nếu người dùng đã tồn tại trong hệ thống
                var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject);

                if (user == null)
                {
                    // Nếu chưa có, tạo người dùng mới
                    user = new User
                    {
                        Email = payload.Email,
                        DisplayName = payload.Name,
                        ProfilePictureUrl = payload.Picture,
                        GoogleId = payload.Subject,
                        CreatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Cập nhật thông tin người dùng nếu cần
                    user.DisplayName = payload.Name;
                    user.ProfilePictureUrl = payload.Picture;
                    user.LastLoginAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Tạo JWT token
                var token = GenerateJwtToken(user);

                // Trả về kết quả
                return new AuthResponseDTO
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Authentication:Jwt:ExpireDays"])),
                    User = new UserDTO
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        DisplayName = user.DisplayName,
                        ProfilePictureUrl = user.ProfilePictureUrl
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực người dùng Google");
                throw;
            }
        }

        public async Task<AuthResponseDTO> RegisterUserAsync(RegisterDTO model)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => 
                u.Username == model.Username || u.Email == model.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Tên người dùng hoặc email đã tồn tại");
            }

            var passwordHash = HashPassword(model.Password);

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHash,
                DisplayName = model.DisplayName ?? model.Username,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Tạo JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Authentication:Jwt:ExpireDays"])),
                User = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    ProfilePictureUrl = user.ProfilePictureUrl
                }
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            
            if (user == null || user.PasswordHash == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Tên đăng nhập hoặc mật khẩu không đúng");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Tạo JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Authentication:Jwt:ExpireDays"])),
                User = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    ProfilePictureUrl = user.ProfilePictureUrl
                }
            };
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Authentication:Jwt:Key"];
            var jwtIssuer = _configuration["Authentication:Jwt:Issuer"];
            var jwtAudience = _configuration["Authentication:Jwt:Audience"];
            var jwtExpireDays = Convert.ToDouble(_configuration["Authentication:Jwt:ExpireDays"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrEmpty(user.Username))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.Username));
            }
            else if (!string.IsNullOrEmpty(user.DisplayName))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(jwtExpireDays);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            var result = new byte[salt.Length + hash.Length];
            salt.CopyTo(result, 0);
            hash.CopyTo(result, salt.Length);

            return Convert.ToBase64String(result);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            var salt = new byte[64]; // HMACSHA512 key size is 64 bytes
            Array.Copy(hashBytes, 0, salt, 0, 64);

            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (hashBytes[64 + i] != computedHash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
} 