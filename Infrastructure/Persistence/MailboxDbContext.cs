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

    public DbSet<Group> Groups { get; set; }
    public DbSet<UserActivationToken> UserActivationTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<User>()
            .HasOne(u => u.UserCredential)
            .WithOne(uc => uc.User)
            .HasForeignKey<UserCredential>(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Group>()
            .HasMany(u => u.Users);


        base.OnModelCreating(modelBuilder);
    }
}
