using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services;

public class ChatService(AppDbContext db)
{
    public async Task<List<ChatRoom>> GetChatsForUserAsync(int userId)
    {
        return await db.ChatRooms
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .Include(c => c.Members).ThenInclude(m => m.User)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ChatRoom?> GetChatForUserAsync(int chatId, int userId)
    {
        // Важно: SQLite не поддерживает SQL APPLY, который может генерироваться
        // при filtered Include с OrderBy/Take на коллекции.
        // Поэтому загружаем чат и сообщения отдельными запросами.
        var chat = await db.ChatRooms
            .Include(c => c.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.Id == chatId && c.Members.Any(m => m.UserId == userId));

        if (chat is null)
        {
            return null;
        }

        var messages = await db.ChatMessages
            .Where(m => m.ChatRoomId == chatId)
            .Include(m => m.SenderUser)
            .OrderByDescending(m => m.SentAtUtc)
            .Take(100)
            .OrderBy(m => m.SentAtUtc)
            .ToListAsync();

        chat.Messages = messages;
        return chat;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await db.Users.OrderBy(u => u.Nickname).ToListAsync();
    }

    public async Task<ChatRoom> CreateChatAsync(string name, IEnumerable<int> userIds, int adminCreatorId = 0)
    {
        var ids = userIds.Distinct().ToList();
        var chat = new ChatRoom
        {
            Name = name.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = adminCreatorId
        };

        db.ChatRooms.Add(chat);
        await db.SaveChangesAsync();

        var members = ids.Select(id => new ChatMember
        {
            ChatRoomId = chat.Id,
            UserId = id,
            AddedAtUtc = DateTime.UtcNow
        });

        db.ChatMembers.AddRange(members);
        await db.SaveChangesAsync();

        return chat;
    }

    public async Task<ChatMessage?> SaveMessageAsync(int chatId, int userId, string text)
    {
        var allowed = await db.ChatMembers.AnyAsync(m => m.ChatRoomId == chatId && m.UserId == userId);
        if (!allowed)
        {
            return null;
        }

        var message = new ChatMessage
        {
            ChatRoomId = chatId,
            SenderUserId = userId,
            Text = text.Trim(),
            SentAtUtc = DateTime.UtcNow
        };

        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        return await db.ChatMessages.Include(m => m.SenderUser).FirstAsync(m => m.Id == message.Id);
    }

    public async Task<List<int>> GetMemberIdsAsync(int chatId)
    {
        return await db.ChatMembers.Where(m => m.ChatRoomId == chatId).Select(m => m.UserId).ToListAsync();
    }
}
