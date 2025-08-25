using Core.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class MailboxDbContext : DbContext
{
    public MailboxDbContext(DbContextOptions<MailboxDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageRecipient> MessageRecipients { get; set; }
    public DbSet<UserCredential> UserCredentials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relacja MessageRecipient (wiele odbiorców do jednej wiadomości / użytkownika)
        modelBuilder.Entity<MessageRecipient>()
            .HasOne(mr => mr.Message)
            .WithMany(m => m.Recipients)
            .HasForeignKey(mr => mr.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MessageRecipient>()
            .HasOne(mr => mr.User)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(mr => mr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacja 1-1 User - UserCredential
        modelBuilder.Entity<User>()
            .HasOne(u => u.UserCredential)
            .WithOne(uc => uc.User)
            .HasForeignKey<UserCredential>(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacja Message - Sender (User)
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Usunięto konfigurację Attachment i wątków (Parent/Replies) – uproszczenie modelu

        base.OnModelCreating(modelBuilder);
    }
}
