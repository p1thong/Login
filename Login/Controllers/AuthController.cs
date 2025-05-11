using Login.Models;
using Login.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Login.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDTO model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.IdToken))
                {
                    return BadRequest(new { message = "IdToken không được để trống" });
                }

                var response = await _authService.AuthenticateGoogleUserAsync(model.IdToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng nhập với Google");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu đăng nhập" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { message = "Thiếu thông tin đăng ký" });
                }

                var response = await _authService.RegisterUserAsync(model);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng ký người dùng");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu đăng ký" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { message = "Thiếu tên đăng nhập hoặc mật khẩu" });
                }

                var response = await _authService.LoginAsync(model);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng nhập");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu đăng nhập" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                return Ok(new UserDTO
                {
                    Id = int.Parse(userId ?? "0"),
                    Email = email ?? "",
                    DisplayName = name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng hiện tại");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
            }
        }
    }
} 