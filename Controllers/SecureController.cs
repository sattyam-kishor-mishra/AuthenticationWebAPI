using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutenticationWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecureController : ControllerBase
    {
        private readonly ILogger<SecureController> _logger;

        public SecureController(ILogger<SecureController> logger)
        {
            _logger = logger;
        }

        [HttpGet("debug-claims")]
        [Authorize]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            _logger.LogInformation("User claims: {@Claims}", claims);
            _logger.LogInformation("Role claims: {@RoleClaims}", User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList());

            return Ok(new 
            { 
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Name = User.Identity?.Name,
                AuthType = User.Identity?.AuthenticationType,
                AllClaims = claims,
                RoleClaims = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList(),
                IsInAdminRole = User.IsInRole("Admin")
            });
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            _logger.LogInformation("Admin endpoint called by: {User}", User.Identity?.Name);
            return Ok("This endpoint is only accessible to Admins.");
        }

        [HttpGet("user-only")]
        [Authorize(Roles = "User")]
        public IActionResult UserOnly()
        {
            return Ok("This endpoint is only accessible to Users.");
        }

        [HttpGet("any-authenticated")]
        [Authorize]
        public IActionResult AnyAuthenticated()
        {
            return Ok("This endpoint is accessible to any authenticated user.");
        }
    }
}
