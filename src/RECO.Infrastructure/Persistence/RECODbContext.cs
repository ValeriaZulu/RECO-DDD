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

            modelBuilder.Entity<Profile>(b => {
                b.HasKey(p => p.Id);
                b.OwnsMany(p => p.GenrePreferences, gp => {
                    gp.WithOwner().HasForeignKey("ProfileId");
                    gp.Property<int>(nameof(Domain.Entities.GenrePreference.GenreId));
                    gp.Property<string>(nameof(Domain.Entities.GenrePreference.Name)).IsRequired();
                    gp.HasKey("ProfileId", nameof(Domain.Entities.GenrePreference.GenreId));
                });

                b.OwnsMany(p => p.PersonPreferences, pp => {
                    pp.WithOwner().HasForeignKey("ProfileId");
                    pp.Property<int>(nameof(Domain.Entities.PersonPreference.PersonId));
                    pp.Property<string>(nameof(Domain.Entities.PersonPreference.Name)).IsRequired();
                    pp.HasKey("ProfileId", nameof(Domain.Entities.PersonPreference.PersonId));
                });
            });

            modelBuilder.Entity<Title>(b => {
                b.HasKey(t => t.Id);
                b.Property<int>("TmdbId").HasColumnName("TmdbId");
                b.HasIndex("TmdbId").IsUnique();
                // Map Reviews as regular entity relationship
                b.HasMany(t => t.Reviews).WithOne().HasForeignKey("TitleId");
            });
        }
    }
}
