namespace tapcet_api.DTO.Auth
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public AuthResponseDto? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string>? Errors { get; set; }

        public static AuthResult Success(AuthResponseDto data) => new()
        {
            Succeeded = true,
            Data = data
        };

        public static AuthResult Failure(string message, List<string>? errors = null) => new()
        {
            Succeeded = false,
            ErrorMessage = message,
            Errors = errors
        };
    }
}
