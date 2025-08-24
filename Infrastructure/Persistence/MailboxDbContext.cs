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
        modelBuilder.Entity<MessageRecipient>()
            .HasOne(mr => mr.Message)
            .WithMany(m => m.Recipients)
            .HasForeignKey(mr => mr.MessageId);

        modelBuilder.Entity<MessageRecipient>()
            .HasOne(mr => mr.User)
            .WithMany()
            .HasForeignKey(mr => mr.UserId);
        
        modelBuilder.Entity<Message>()
            .HasOne(m => m.ParentMessage)
            .WithMany(m => m.Replies)
            .HasForeignKey(m => m.ParentMessageId);

        base.OnModelCreating(modelBuilder);
    }
}
