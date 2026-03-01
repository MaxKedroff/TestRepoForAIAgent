using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Hubs;

[Authorize]
public class ChatHub(ChatService chatService) : Hub
{
    public async Task JoinChat(int chatId)
    {
        await EnsureMembership(chatId);
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(chatId));
    }

    public async Task SendMessage(int chatId, string text)
    {
        var userId = CurrentUserId();
        if (userId is null || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var saved = await chatService.SaveMessageAsync(chatId, userId.Value, text);
        if (saved is null)
        {
            return;
        }

        await Clients.Group(Group(chatId)).SendAsync("ReceiveMessage", new
        {
            id = saved.Id,
            sender = saved.SenderUser.Nickname,
            senderId = saved.SenderUserId,
            text = saved.Text,
            sentAt = saved.SentAtUtc.ToLocalTime().ToString("HH:mm:ss")
        });
    }

    // Дополнительная фича: живой индикатор "печатает..."
    public async Task Typing(int chatId, bool isTyping)
    {
        var userId = CurrentUserId();
        if (userId is null) return;

        await EnsureMembership(chatId);
        var nickname = Context.User?.Identity?.Name ?? "Пользователь";

        await Clients.OthersInGroup(Group(chatId)).SendAsync("Typing", new
        {
            userId = userId.Value,
            nickname,
            isTyping
        });
    }

    private int? CurrentUserId()
    {
        var idClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    private async Task EnsureMembership(int chatId)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            throw new HubException("Not authenticated");
        }

        var members = await chatService.GetMemberIdsAsync(chatId);
        if (!members.Contains(userId.Value))
        {
            throw new HubException("No chat access");
        }
    }

    private static string Group(int chatId) => $"chat-{chatId}";
}
