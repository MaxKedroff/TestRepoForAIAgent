using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers;

[Authorize]
public class ChatController(ChatService chatService) : Controller
{
    private bool IsAdmin() => User.Claims.Any(c => c.Type == "role" && c.Value == "admin");
    private int? CurrentUserId()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    public async Task<IActionResult> Index()
    {
        if (IsAdmin())
        {
            return RedirectToAction("Index", "Admin");
        }

        var userId = CurrentUserId();
        if (userId is null) return Forbid();

        var chats = await chatService.GetChatsForUserAsync(userId.Value);
        return View(chats);
    }

    [HttpGet("/chat/room/{id:int}")]
    public async Task<IActionResult> Room(int id)
    {
        if (IsAdmin()) return Forbid();

        var userId = CurrentUserId();
        if (userId is null) return Forbid();

        var chat = await chatService.GetChatForUserAsync(id, userId.Value);
        if (chat is null) return NotFound();

        ViewBag.CurrentUserId = userId.Value;
        return View(chat);
    }
}
