using System.ComponentModel.DataAnnotations;

namespace ChatApp.ViewModels;

public class AdminLoginViewModel
{
    [Required]
    [Display(Name = "Логин")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;
}
