using AutenticationWeb.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutenticationWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManger, RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManger;
            _roleManager = roleManager;
            _configuration = configuration;


        }

        [HttpGet("debug-user/{username}")]
        public async Task<IActionResult> DebugUser(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound($"User {username} not found");

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { 
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles,
                RoleCount = roles.Count
            });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) || 
                string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.Role))
            {
                return BadRequest("Username, email, password, and role are required.");
            }

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return BadRequest("User already exists!");
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.Role);

            // Create the user
            var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Create the role if it doesn't exist
            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new ApplicationRole { Name = model.Role });
                if (!roleResult.Succeeded)
                {
                    return BadRequest(roleResult.Errors);
                }
            }

            // Add user to role
            var addToRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addToRoleResult.Succeeded)
            {
                return BadRequest(addToRoleResult.Errors);
            }

            return Ok(new { message = $"User {model.Username} registered successfully with role {model.Role}" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized("Invalid username or password");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                //new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add all roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audiences:Web"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: cred
            );

            var returnToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok($"Bearer {returnToken}");
        }
    }
}
