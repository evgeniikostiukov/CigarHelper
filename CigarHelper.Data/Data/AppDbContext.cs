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
    public DbSet<Cigar> Cigars { get; set; } = null!;
    public DbSet<Humidor> Humidors { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<ReviewImage> ReviewImages { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This is a fallback if the context is created without options
            var connectionString = GetConnectionStringFromConfig();
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }
    
    private string GetConnectionStringFromConfig()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true);
            
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
            
        // Configure Cigar entity
        modelBuilder.Entity<Cigar>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure Humidor entity
        modelBuilder.Entity<Humidor>()
            .HasOne(h => h.User)
            .WithMany(u => u.Humidors)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Cigar>()
            .HasOne(c => c.Humidor)
            .WithMany(h => h.Cigars)
            .HasForeignKey(c => c.HumidorId)
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
    }
} 