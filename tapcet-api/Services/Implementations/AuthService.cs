using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using tapcet_api.DTO.Auth;
using tapcet_api.Models;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    jwtSettings.GetValue<int>("ExpiryInMinutes")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Registration attempt for email: {Email}", registerDto.Email);

                // Check if email already exists
                if (await UserExistsAsync(registerDto.Email))
                {
                    _logger.LogWarning("Registration failed: Email {Email} already exists", registerDto.Email);
                    return AuthResult.Failure("Email is already registered", new List<string> { "EMAIL_EXISTS" });
                }

                // Check if username already exists
                var existingUser = await _userManager.FindByNameAsync(registerDto.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Username {Username} already exists", registerDto.Username);
                    return AuthResult.Failure("Username is already taken", new List<string> { "USERNAME_EXISTS" });
                }

                // Create new user
                var user = new User
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    CreatedDate = DateTime.UtcNow
                };

                _logger.LogInformation("Creating user account for {Email}", registerDto.Email);
                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    var errorString = string.Join(", ", errors);
                    
                    _logger.LogError("User creation failed for {Email}. Errors: {Errors}", 
                        registerDto.Email, errorString);
                    
                    return AuthResult.Failure("Password validation failed", errors);
                }

                _logger.LogInformation("User account created for {Email}, assigning role", user.Email);
                await _userManager.AddToRoleAsync(user, "User");

                _logger.LogInformation("Generating JWT token for {Email}", user.Email);
                var token = await GenerateJwtToken(user);

                var response = new AuthResponseDto
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        _configuration.GetValue<int>("JwtSettings:ExpiryInMinutes")),
                    Roles = new List<string> { "User" }
                };

                _logger.LogInformation("User {Email} registered successfully", user.Email);
                return AuthResult.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", registerDto.Email);
                return AuthResult.Failure("An unexpected error occurred during registration", 
                    new List<string> { "INTERNAL_ERROR", ex.Message });
            }
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User {Email} not found", loginDto.Email);
                    return AuthResult.Failure("Invalid email or password", new List<string> { "INVALID_CREDENTIALS" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Login failed: Invalid password for {Email}", loginDto.Email);
                    return AuthResult.Failure("Invalid email or password", new List<string> { "INVALID_CREDENTIALS" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = await GenerateJwtToken(user);

                var response = new AuthResponseDto
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        _configuration.GetValue<int>("JwtSettings:ExpiryInMinutes")),
                    Roles = roles.ToList()
                };

                _logger.LogInformation("User {Email} logged in successfully", user.Email);
                return AuthResult.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", loginDto.Email);
                return AuthResult.Failure("An unexpected error occurred during login", 
                    new List<string> { "INTERNAL_ERROR", ex.Message });
            }
        }
    }
}
