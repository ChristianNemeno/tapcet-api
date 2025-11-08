using tapcet_api.DTO.Auth;

namespace tapcet_api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<bool> UserExistsAsync(string email);
    }
}
