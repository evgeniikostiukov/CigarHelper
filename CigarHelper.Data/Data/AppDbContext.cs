using System.Reflection;
using CigarHelper.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CigarHelper.Data.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Brand> Brands { get; set; } = null!;
    public DbSet<CigarBase> CigarBases { get; set; } = null!;
    public DbSet<UserCigar> UserCigars { get; set; } = null!;
    public DbSet<Humidor> Humidors { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<ReviewImage> ReviewImages { get; set; } = null!;
    public DbSet<CigarImage> CigarImages { get; set; } = null!;
    public DbSet<CigarComment> CigarComments { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var connectionString = GetConnectionStringFromConfig();
        if (!string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.EnableDetailedErrors(true);
        }
    }
    
    private string GetConnectionStringFromConfig()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables();

        var config = configBuilder.Build();
        return config.GetConnectionString("DefaultConnection") ?? "";
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure User entity
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
            
        // Configure Brand entity
        modelBuilder.Entity<Brand>()
            .HasIndex(b => b.Name)
            .IsUnique();
            
        // Configure CigarBase entity
        modelBuilder.Entity<CigarBase>()
            .HasIndex(c => new { c.Name, c.BrandId })
            .IsUnique();
            
        modelBuilder.Entity<CigarBase>()
            .HasOne(cb => cb.Brand)
            .WithMany(b => b.Cigars)
            .HasForeignKey(cb => cb.BrandId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure UserCigar entity
        modelBuilder.Entity<UserCigar>()
            .HasOne(uc => uc.CigarBase)
            .WithMany(cb => cb.UserCigars)
            .HasForeignKey(uc => uc.CigarBaseId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<UserCigar>()
            .HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure Humidor entity
        modelBuilder.Entity<Humidor>()
            .HasOne(h => h.User)
            .WithMany(u => u.Humidors)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<UserCigar>()
            .HasOne(uc => uc.Humidor)
            .WithMany(h => h.Cigars)
            .HasForeignKey(uc => uc.HumidorId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Configure Review entity
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Cigar)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CigarId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure ReviewImage entity
        modelBuilder.Entity<ReviewImage>()
            .HasOne(ri => ri.Review)
            .WithMany(r => r.Images)
            .HasForeignKey(ri => ri.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CigarImage>()
            .HasOne(ci => ci.CigarBase)
            .WithMany(cb => cb.Images)
            .HasForeignKey(ci => ci.CigarBaseId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<CigarImage>()
            .HasOne(ci => ci.UserCigar)
            .WithMany(uc => uc.Images)
            .HasForeignKey(ci => ci.UserCigarId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CigarComment>(entity =>
        {
            entity.HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.CigarBase)
                .WithMany()
                .HasForeignKey(c => c.CigarBaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.UserCigar)
                .WithMany()
                .HasForeignKey(c => c.UserCigarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => c.CigarBaseId);
            entity.HasIndex(c => c.UserCigarId);
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_CigarComments_SingleTarget",
                "(\"CigarBaseId\" IS NOT NULL AND \"UserCigarId\" IS NULL) OR (\"CigarBaseId\" IS NULL AND \"UserCigarId\" IS NOT NULL)"));
        });
    }
} 