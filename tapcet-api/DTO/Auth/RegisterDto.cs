using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50,MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is requred")]
        [EmailAddress(ErrorMessage ="Invalid email format")]
        public string Email { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "Password is requred")]
        [StringLength(100, MinimumLength =6, ErrorMessage ="Password must be atleast 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
