using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SavePoint.Entities.Users;
using SavePoint.Entities.Games;
using SavePoint.Entities.Lists;
using SavePoint.Entities.Reviews;

namespace SavePoint.DAL.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //API Entitites
        public DbSet<Game> Games { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<GameGenre> GameGenres { get; set; }
        public DbSet<GamePlatform> GamePlatforms { get; set; }
        public DbSet<GameCompany> GameCompanies { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Company> Companies { get; set; }

        //Internal Entitites
        public DbSet<Review> Reviews { get; set; }
        public DbSet<UserList> UserLists { get; set; }
        public DbSet<UserListItem> UserListItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========= API Entities =========
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.HasIndex(g => g.ExternalId).IsUnique();
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.HasIndex(g => g.ExternalId).IsUnique();
            });

            modelBuilder.Entity<Platform>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.ExternalId).IsUnique();
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.ExternalId).IsUnique();
            });

            // ========= Many-to-Many =========
            modelBuilder.Entity<GameGenre>(entity =>
            {
                entity.HasKey(gg => gg.Id);
                entity.HasIndex(gg => new { gg.GameId, gg.GenreId }).IsUnique();
            });

            modelBuilder.Entity<GamePlatform>(entity =>
            {
                entity.HasKey(gp => gp.Id);
                entity.HasIndex(gp => new { gp.GameId, gp.PlatformId }).IsUnique();
            });

            modelBuilder.Entity<GameCompany>(entity =>
            {
                entity.HasKey(gc => gc.Id);
                entity.HasIndex(gc => new { gc.GameId, gc.CompanyId }).IsUnique();
            });

            // ========= Review =========
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => new { r.GameId, r.UserId }).IsUnique();
            });

            // ========= User List =========
            modelBuilder.Entity<UserList>(entity =>
            {
                entity.HasKey(ul => ul.Id);
            });

            modelBuilder.Entity<UserListItem>(entity =>
            {
                entity.HasKey(uli => uli.Id);
                entity.HasIndex(uli => new { uli.UserListId, uli.GameId }).IsUnique();
            });
        }
    }
}
