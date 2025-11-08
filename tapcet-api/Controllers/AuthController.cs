using Microsoft.AspNetCore.Mvc;

using tapcet_api.DTO.Auth;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);

            if(result == null)
            {
                return BadRequest(new { message = "Registration failed. Email may already be in use." });
            }

            _logger.LogInformation("User registered: {Email}", registerDto.Email);
            return Ok(result);

        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            _logger.LogInformation("User logged: {Email}", loginDto.Email);

            return Ok(result);

        }

        [HttpPost("check-email")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var exists = await _authService.UserExistsAsync(email);
            return Ok(new { exists });
        }


    }
}
