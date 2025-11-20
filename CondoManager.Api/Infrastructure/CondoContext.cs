using CondoManager.Entity.Models;
using Microsoft.EntityFrameworkCore;

namespace CondoManager.Api.Infrastructure
{
    public class CondoContext : DbContext
    {
        public CondoContext(DbContextOptions<CondoContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<ApartmentUser> ApartmentUsers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApartmentUser>()
                .HasKey(au => new { au.ApartmentId, au.UserId });

            modelBuilder.Entity<ApartmentUser>()
                .HasOne(au => au.Apartment)
                .WithMany(a => a.Residents)
                .HasForeignKey(au => au.ApartmentId);

            modelBuilder.Entity<ApartmentUser>()
                .HasOne(au => au.User)
                .WithOne(u => u.Apartment)
                .HasForeignKey<ApartmentUser>(au => au.UserId);
        }
    }
}
