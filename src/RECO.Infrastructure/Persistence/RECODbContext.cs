using Microsoft.EntityFrameworkCore;
using RECO.Domain.Entities;

namespace RECO.Infrastructure.Persistence
{
    // EF Core DbContext lives in Infrastructure per Clean Architecture
    public class RECODbContext : DbContext
    {
        public RECODbContext(DbContextOptions<RECODbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Minimal mappings to enable initial migrations
            modelBuilder.Entity<User>(b => {
                b.HasKey(u => u.Id);
                b.Property(u => u.Email).IsRequired();
            });

            modelBuilder.Entity<Title>(b => {
                b.HasKey(t => t.Id);
                b.Property<int>("TmdbId").HasColumnName("TmdbId");
                b.HasIndex("TmdbId").IsUnique();
            });
        }
    }
}
