using System.ComponentModel.DataAnnotations;
using ChatApp.Models;

namespace ChatApp.ViewModels;

public class CreateChatViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Название чата")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Участники")]
    public List<int> SelectedUserIds { get; set; } = new();

    public List<User> AvailableUsers { get; set; } = new();
}
