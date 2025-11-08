using tapcet_api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using tapcet_api.DTO.Auth;
using tapcet_api.Models;

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

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // check first if no existing email with that entry
                if (await UserExistsAsync(registerDto.Email){

                    _logger.LogWarning("Registration failed: {Email} already exists", registerDto.Email);
                    return null;
                }

                // then make a new user through the received dto
                // note that in our frontend , well be handling that
                // validation is best on both sides( still thinking about it )
                var user = new User
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    CreatedDate = DateTime.UtcNow
                };


                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    _logger.LogError("User creation failed: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }

                await _userManager.AddToRoleAsync(user, "User");

                _logger.LogInformation("User {Email} registered successfully", user.Email);

                var token = await GenerateJwtToken(user);

                return new AuthResponseDto
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        _configuration.GetValue<int>("JwtSettings:ExpiryInMinutes")),
                    Roles = new List<string> { "User" }
                };

            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error during registration for {Email}", registerDto.Email);
                return null;
            }

        }

        public Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            throw new NotImplementedException();
        }
    }

}
