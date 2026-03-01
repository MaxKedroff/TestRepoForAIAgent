using System.ComponentModel.DataAnnotations;

namespace ChatApp.ViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Токен")]
    public string Token { get; set; } = string.Empty;
}
