using tapcet_api.DTO.Auth;

namespace tapcet_api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> UserExistsAsync(string email);
        Task<AuthResult> RegisterAsync(RegisterDto registerDto);
        Task<AuthResult> LoginAsync(LoginDto loginDto);
    }
}
