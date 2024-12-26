using SoruCevap.Models;
using AspNetCoreHero.ToastNotification.Notyf.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NETCore.Encrypt.Extensions;

namespace SoruCevap.Models
{
    public class AppDbContext : IdentityDbContext <AppUser,AppRole,string>
    {
        private readonly IConfiguration _config;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config) : base(options)
        {
            _config = config;
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<QuestionVote> QuestionVotes { get; set; }
        public DbSet<QuestionReport> QuestionReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var adminRolId = Guid.NewGuid().ToString(); // Admin rolü ID
            var adminUserId = Guid.NewGuid().ToString(); // Admin kullanıcı ID
            modelBuilder.Entity<Category>(entity =>
              {
                  entity.ToTable("Categories");
                  entity.HasKey(e => e.Id);

                  entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                  entity.Property(e => e.Description)
                      .HasMaxLength(500);

                  entity.Property(e => e.ImageUrl)
                      .HasMaxLength(250);

                  entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                  entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
              });
            modelBuilder.Entity<AppRole>().HasData(
              new AppRole
              {
                  Id = adminRolId,
                  Name = "Admin",
                  NormalizedName = "ADMIN"
              },
              new AppRole
              {
                  Id = Guid.NewGuid().ToString(),
                  Name = "Kullanici",
                  NormalizedName = "KULLANICI"
              },
              new AppRole
              {
                  Id = Guid.NewGuid().ToString(),
                  Name = "Personel",
                  NormalizedName = "PERSONEL"
              });
            // Question Configuration
            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("Questions");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Content)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Status)
                    .HasDefaultValue(QuestionStatus.Pending);

                entity.Property(e => e.ViewCount)
                    .HasDefaultValue(0);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Questions)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReviewedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
   
            // Answer Configuration
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.ToTable("Answers");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Content)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.IsAccepted)
                    .HasDefaultValue(false);

                entity.HasOne(e => e.Question)
                    .WithMany(q => q.Answers)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });



            // AppUser Configuration
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .HasMaxLength(50);
            });

          

          

            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = adminUserId,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "ahmetadmin@example.com",
                    NormalizedEmail = "AHMETADMIN@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = new PasswordHasher<AppUser>().HashPassword(null, "AhmetAdmin123."),
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    FirstName = "Admin",
                    LastName = "User"
                });

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = adminUserId,
                    RoleId = adminRolId
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}
