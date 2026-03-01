using ChatApp.Services;
using ChatApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers;

[Authorize]
public class AdminController(AuthService authService, ChatService chatService) : Controller
{
    private bool IsAdmin() => User.Claims.Any(c => c.Type == "role" && c.Value == "admin");

    public async Task<IActionResult> Index()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        ViewBag.Users = await chatService.GetAllUsersAsync();
        return View(new IssueTokenViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IssueToken(IssueTokenViewModel model)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        ViewBag.Users = await chatService.GetAllUsersAsync();

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var (_, token) = await authService.IssueUserTokenAsync(model.Nickname);
        model.IssuedToken = token;
        return View("Index", model);
    }

    [HttpGet]
    public async Task<IActionResult> CreateChat()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        return View(new CreateChatViewModel
        {
            AvailableUsers = await chatService.GetAllUsersAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChat(CreateChatViewModel model)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        model.AvailableUsers = await chatService.GetAllUsersAsync();

        if (!ModelState.IsValid || model.SelectedUserIds.Count == 0)
        {
            if (model.SelectedUserIds.Count == 0)
            {
                ModelState.AddModelError(nameof(model.SelectedUserIds), "Выберите минимум одного участника.");
            }
            return View(model);
        }

        await chatService.CreateChatAsync(model.Name, model.SelectedUserIds);
        TempData["success"] = "Чат создан.";
        return RedirectToAction(nameof(CreateChat));
    }
}
