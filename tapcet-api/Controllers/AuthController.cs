using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using tapcet_api.DTO.Auth;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration validation failed for {Email}", registerDto.Email);
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for {Email}: {Message}",
                    registerDto.Email, result.ErrorMessage);

                return BadRequest(new
                {
                    message = result.ErrorMessage,
                    errors = result.Errors
                });
            }

            _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
            return Ok(result.Data);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login validation failed for {Email}", loginDto.Email);
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed for {Email}: {Message}",
                    loginDto.Email, result.ErrorMessage);

                return Unauthorized(new
                {
                    message = result.ErrorMessage,
                    errors = result.Errors
                });
            }

            _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
            return Ok(result.Data);
        }

        [HttpPost("check-email")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var exists = await _authService.UserExistsAsync(email);
            return Ok(new { exists });
        }
    }
}
