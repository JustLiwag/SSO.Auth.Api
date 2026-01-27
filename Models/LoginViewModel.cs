using System.ComponentModel.DataAnnotations;

namespace SSO.Auth.Api.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "The Username field is required.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "The Password field is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
