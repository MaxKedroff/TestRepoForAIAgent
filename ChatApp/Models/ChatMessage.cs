namespace ChatApp.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = default!;
    public int SenderUserId { get; set; }
    public User SenderUser { get; set; } = default!;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}
