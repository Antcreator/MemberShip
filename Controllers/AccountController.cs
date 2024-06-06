using MemberShip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MemberShip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new UserModel { UserName = model.Email, Email = model.Email, Name = model.Name };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Check if the "Admin" role exists
                var roleExists = await _roleManager.RoleExistsAsync("Admin");

                if (!roleExists)
                {
                    // Create the "Admin" role if it doesn't exist
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Assign the "Admin" role to the registered user
                await _userManager.AddToRoleAsync(user, "Admin");

                return Ok(new { message = "User registered successfully as Admin" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var tokenString = GenerateJwtToken(user);
                return Ok(new { token = tokenString });
            }

            return Unauthorized();
        }

        private string GenerateJwtToken(UserModel user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? throw new ArgumentNullException(nameof(user.Email))),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var issuer = _configuration["Jwt:Issuer"];
            if (issuer == null) throw new ArgumentNullException("Jwt:Issuer");

            var audience = _configuration["Jwt:Audience"];
            if (audience == null) throw new ArgumentNullException("Jwt:Audience");

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
