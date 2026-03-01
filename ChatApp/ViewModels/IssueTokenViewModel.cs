using System.ComponentModel.DataAnnotations;

namespace ChatApp.ViewModels;

public class IssueTokenViewModel
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [Display(Name = "Никнейм")]
    public string Nickname { get; set; } = string.Empty;

    public string? IssuedToken { get; set; }
}
