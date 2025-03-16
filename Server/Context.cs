using Library;
using Microsoft.EntityFrameworkCore;

namespace ServerChat;

public class Context : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Message> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(Console.WriteLine)
            .UseLazyLoadingProxies()
            .UseNpgsql("Host=localhost;Port=5432;Database=chatdb;Username=postgres;Password=postgres");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("messages_pkey");
        
            entity.ToTable("Messages");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateTime).IsRequired().HasColumnName("data_time");
            entity.Property(e => e.Content).IsRequired().HasColumnName("content");
            entity.Property(e => e.IsReceived).IsRequired().HasColumnName("is_received");
            
            entity.Property(e => e.FromUserId).IsRequired().HasColumnName("from_user_id");
            entity.Property(e => e.ToUserId).IsRequired().HasColumnName("to_user_id");
            entity.Property(e => e.Type).IsRequired().HasColumnName("type");

            entity.HasOne(e => e.FromUser)
                .WithMany(u => u.MessagesFromUser)
                .HasForeignKey(e => e.FromUserId)
                .HasConstraintName("messages_from_user_id_fkey");

                entity.HasOne(e => e.ToUser)
                    .WithMany(u => u.MessagesToUser)
                    .HasForeignKey(e => e.ToUserId)
                .HasConstraintName("messages_to_user_id_fkey");
        });
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");
        
            entity.ToTable("Users");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nick).IsRequired().HasMaxLength(255).HasColumnName("nick");
        });
        
        base.OnModelCreating(modelBuilder);
    }
}