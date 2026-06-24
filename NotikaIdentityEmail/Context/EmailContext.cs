using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Entities;

namespace NotikaIdentityEmail.Context
{
    public class EmailContext : IdentityDbContext<AppUser>
    {
        public EmailContext(DbContextOptions<EmailContext> options) : base(options)
        {
            
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API: Code-First yaklaşımında veritabanı tabloları arasındaki karmaşık ilişkileri ve kısıtlamaları belirler
            modelBuilder.Entity<Message>(entity =>
            {
                // Bir mesajın bir göndericisi (HasOne), bir kullanıcının ise birden fazla gönderdiği mesajı (WithMany) olabilir.
                // DeleteBehavior.Restrict: Kullanıcı silindiğinde ona ait mesajların silinmesini (Cascade delete) engelleyerek veri bütünlüğünü korur.
                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
