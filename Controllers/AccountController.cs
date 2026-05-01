using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutenticationWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManger, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManger;
            _roleManager = roleManager;

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(string userName, string password, string role)
        {
            var user = new IdentityUser { UserName = userName };
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            await _userManager.AddToRoleAsync(user, role);

            return Ok($"User {user} registered successfully with role {role}");
        }
    }
}
