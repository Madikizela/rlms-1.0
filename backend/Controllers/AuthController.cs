using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using backend.Services;
using backend.Services.Interfaces;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            IPasswordHashingService passwordHashingService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _context = context;
            _passwordHashingService = passwordHashingService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                // Find user by email
                var user = await _context.Users
                    .Include(u => u.Client)
                    .Include(u => u.SkillsDevelopmentProvider)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Verify password
                if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for user: {Email}", request.Email);
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Check if user is active
                if (user.Status != UserStatus.Active)
                {
                    _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
                    return Unauthorized(new { message = "Account is not active" });
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                _logger.LogInformation("Successful login for user: {Email}", request.Email);

                return Ok(new LoginResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        Role = user.Role.ToString(),
                        Status = user.Status.ToString(),
                        ClientId = user.ClientId,
                        ClientName = user.Client?.Name,
                        SkillsDevelopmentProviderId = user.SkillsDevelopmentProviderId,
                        SkillsDevelopmentProviderName = user.SkillsDevelopmentProvider?.Name,
                        DepartmentId = user.DepartmentId,
                        DepartmentName = user.Department?.Name
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for email: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            var issuer = jwtSettings["Issuer"] ?? "YourAppName";
            var audience = jwtSettings["Audience"] ?? "YourAppUsers";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("ClientId", user.ClientId?.ToString() ?? ""),
                new Claim("SkillsDevelopmentProviderId", user.SkillsDevelopmentProviderId?.ToString() ?? ""),
                new Claim("DepartmentId", user.DepartmentId?.ToString() ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto();
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public int? SkillsDevelopmentProviderId { get; set; }
        public string? SkillsDevelopmentProviderName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}