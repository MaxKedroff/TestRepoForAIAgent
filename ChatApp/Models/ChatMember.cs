namespace ChatApp.Models;

public class ChatMember
{
    public int ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = default!;
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
}
