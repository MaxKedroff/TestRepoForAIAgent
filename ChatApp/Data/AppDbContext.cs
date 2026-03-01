using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatMember> ChatMembers => Set<ChatMember>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.AuthToken)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Nickname)
            .IsUnique();

        modelBuilder.Entity<ChatMember>()
            .HasKey(cm => new { cm.ChatRoomId, cm.UserId });

        modelBuilder.Entity<ChatMember>()
            .HasOne(cm => cm.ChatRoom)
            .WithMany(cr => cr.Members)
            .HasForeignKey(cm => cm.ChatRoomId);

        modelBuilder.Entity<ChatMember>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.ChatMembers)
            .HasForeignKey(cm => cm.UserId);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.ChatRoom)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.SenderUser)
            .WithMany()
            .HasForeignKey(m => m.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
