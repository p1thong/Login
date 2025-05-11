using Login.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace Login.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.GoogleClientId = _configuration["Authentication:Google:ClientId"];
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Route("/login-google")]
        public IActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback"),
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme }
                }
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [Route("/signin-google")]
        public async Task<IActionResult> GoogleCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            
            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Index");
            }

            var googleUser = authenticateResult.Principal;
            var emailClaim = googleUser.FindFirst(ClaimTypes.Email);
            var nameClaim = googleUser.FindFirst(ClaimTypes.Name);
            
            // Tại đây bạn có thể thực hiện thêm các thao tác xử lý, 
            // như lưu thông tin người dùng vào cơ sở dữ liệu

            ViewBag.Email = emailClaim?.Value;
            ViewBag.Name = nameClaim?.Value;

            return RedirectToAction("Dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
