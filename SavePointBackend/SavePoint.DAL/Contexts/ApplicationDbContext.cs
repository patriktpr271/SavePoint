using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SavePoint.Entities.Users;
using SavePoint.Entities.Games;
using SavePoint.Entities.Lists;
using SavePoint.Entities.Reviews;
using SavePoint.Common.Enums;
using SavePoint.Entities.Popularity;

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
        public DbSet<Popularity> Popularities { get; set; }

		//Internal Entitites
		public DbSet<Review> Reviews { get; set; }
        public DbSet<UserList> UserLists { get; set; }
        public DbSet<UserListItem> UserListItems { get; set; }
        public DbSet<UserListVote> UserListVotes { get; set; }

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

            // ========= Many-to-Many Relationships =========
            modelBuilder.Entity<GameGenre>(entity =>
            {
                entity.HasKey(gg => gg.Id);
                entity.HasIndex(gg => new { gg.GameId, gg.GenreId }).IsUnique();
                
                entity.HasOne(gg => gg.Game)
                    .WithMany(g => g.GameGenres)
                    .HasForeignKey(gg => gg.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(gg => gg.Genre)
                    .WithMany(g => g.GameGenres)
                    .HasForeignKey(gg => gg.GenreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GamePlatform>(entity =>
            {
                entity.HasKey(gp => gp.Id);
                entity.HasIndex(gp => new { gp.GameId, gp.PlatformId }).IsUnique();
                
                entity.HasOne(gp => gp.Game)
                    .WithMany(g => g.GamePlatforms)
                    .HasForeignKey(gp => gp.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(gp => gp.Platform)
                    .WithMany(p => p.GamePlatforms)
                    .HasForeignKey(gp => gp.PlatformId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GameCompany>(entity =>
            {
                entity.HasKey(gc => gc.Id);
                entity.HasIndex(gc => new { gc.GameId, gc.CompanyId});
                entity.Property(gc => gc.Role)
                    .HasConversion<string>();
                
                entity.HasOne(gc => gc.Game)
                    .WithMany(g => g.GameCompanies)
                    .HasForeignKey(gc => gc.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(gc => gc.Company)
                    .WithMany(c => c.GameCompanies)
                    .HasForeignKey(gc => gc.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========= Review =========
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => new { r.GameId, r.UserId }).IsUnique();
                
                entity.HasOne(r => r.Game)
                    .WithMany(g => g.Reviews)
                    .HasForeignKey(r => r.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========= User List =========
            modelBuilder.Entity<UserList>(entity =>
            {
                entity.HasKey(ul => ul.Id);
                
                entity.HasOne(ul => ul.User)
                    .WithMany(u => u.UserLists)
                    .HasForeignKey(ul => ul.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserListItem>(entity =>
            {
                entity.HasKey(uli => uli.Id);
                entity.HasIndex(uli => new { uli.UserListId, uli.GameId }).IsUnique();
                
                entity.HasOne(uli => uli.UserList)
                    .WithMany(ul => ul.UserListItems)
                    .HasForeignKey(uli => uli.UserListId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(uli => uli.Game)
                    .WithMany(g => g.UserListItems)
                    .HasForeignKey(uli => uli.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========= User List Vote =========
            modelBuilder.Entity<UserListVote>(entity =>
            {
                entity.HasKey(ulv => ulv.Id);
                entity.HasIndex(ulv => new { ulv.UserListId, ulv.UserId }).IsUnique();
                
                entity.HasOne(ulv => ulv.UserList)
                    .WithMany(ul => ul.Votes)
                    .HasForeignKey(ulv => ulv.UserListId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(ulv => ulv.User)
                    .WithMany(u => u.UserListVotes)
                    .HasForeignKey(ulv => ulv.UserId)
                    .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete conflicts
            });

			// ========= Popularity =========
			modelBuilder.Entity<Popularity>(entity =>
			{
				entity.HasKey(p => p.Id);
				entity.HasIndex(p => p.ExternalId);

				// Configure PopularityScore with higher precision (18 total digits, 10 decimal places)
				entity.Property(p => p.PopularityScore)
					.HasColumnType("decimal(18,10)");

				entity.HasOne(p => p.Game)
					.WithMany(g => g.Popularities)
					.HasForeignKey(p => p.GameId)
					.OnDelete(DeleteBehavior.Cascade);
			});
		}
	}
}
