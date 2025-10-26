using System.Security.Claims;
using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CigarHelper.Api.Services;

public interface IHumidorService
{
    Task<List<HumidorResponseDto>> GetUserHumidors(int userId);
    Task<HumidorDetailResponseDto?> GetHumidorById(int humidorId, int userId);
    Task<HumidorResponseDto> CreateHumidor(HumidorCreateDto dto, int userId);
    Task<bool> UpdateHumidor(int humidorId, HumidorUpdateDto dto, int userId);
    Task<bool> DeleteHumidor(int humidorId, int userId);
    Task<bool> AddCigarToHumidor(int humidorId, int cigarId, int userId);
    Task<bool> RemoveCigarFromHumidor(int humidorId, int cigarId, int userId);
    Task<List<CigarBriefDto>> GetCigarsInHumidor(int humidorId, int userId);
}

public class HumidorService : IHumidorService
{
    private readonly AppDbContext _context;

    public HumidorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<HumidorResponseDto>> GetUserHumidors(int userId)
    {
        var humidors = await _context.Humidors
            .Where(h => h.UserId == userId)
            .Select(h => new HumidorResponseDto
            {
                Id = h.Id,
                Name = h.Name,
                Description = h.Description,
                Capacity = h.Capacity,
                Humidity = h.Humidity,
                CurrentCount = h.Cigars.Count,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            })
            .ToListAsync();

        return humidors;
    }

    public async Task<HumidorDetailResponseDto?> GetHumidorById(int humidorId, int userId)
    {
        var humidor = await _context.Humidors
            .Include(h => h.Cigars)
            .ThenInclude(uc => uc.CigarBase)
            .Where(h => h.Id == humidorId && h.UserId == userId)
            .Select(h => new HumidorDetailResponseDto
            {
                Id = h.Id,
                Name = h.Name,
                Description = h.Description,
                Capacity = h.Capacity,
                Humidity = h.Humidity,
                Cigars = h.Cigars.Select(c => new CigarBriefDto
                {
                    Id = c.Id,
                    Name = c.CigarBase.Name,
                    Brand = new BrandDto()
                    {
                        Id = c.CigarBase.Brand.Id,
                        Name = c.CigarBase.Brand.Name,
                        Description = c.CigarBase.Brand.Description,
                        UpdatedAt = c.CigarBase.Brand.UpdatedAt,
                        CreatedAt = c.CigarBase.Brand.CreatedAt,
                        Country = c.CigarBase.Brand.Country,
                        IsModerated = c.CigarBase.Brand.IsModerated,
                        LogoBytes = c.CigarBase.Brand.LogoBytes,
                    },
                    BrandName = c.CigarBase.Brand.Name,
                    Size = c.CigarBase.Size,
                    Strength = c.CigarBase.Strength,
                    Price = c.Price,
                    Rating = c.Rating,
                }).ToList(),
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return humidor;
    }

    public async Task<HumidorResponseDto> CreateHumidor(HumidorCreateDto dto, int userId)
    {
        var humidor = new Humidor
        {
            Name = dto.Name,
            Description = dto.Description,
            Capacity = dto.Capacity,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Humidors.Add(humidor);
        await _context.SaveChangesAsync();

        return new HumidorResponseDto
        {
            Id = humidor.Id,
            Name = humidor.Name,
            Description = humidor.Description,
            Capacity = humidor.Capacity,
            Humidity = humidor.Humidity,
            CurrentCount = 0,
            CreatedAt = humidor.CreatedAt,
            UpdatedAt = humidor.UpdatedAt
        };
    }

    public async Task<bool> UpdateHumidor(int humidorId, HumidorUpdateDto dto, int userId)
    {
        var humidor = await _context.Humidors
            .FirstOrDefaultAsync(h => h.Id == humidorId && h.UserId == userId);

        if (humidor == null)
            return false;

        humidor.Name = dto.Name;
        humidor.Description = dto.Description;
        humidor.Capacity = dto.Capacity;
        humidor.Humidity = dto.Humidity;
        humidor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteHumidor(int humidorId, int userId)
    {
        var humidor = await _context.Humidors
            .FirstOrDefaultAsync(h => h.Id == humidorId && h.UserId == userId);

        if (humidor == null)
            return false;

        _context.Humidors.Remove(humidor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCigarToHumidor(int humidorId, int cigarId, int userId)
    {
        var humidor = await _context.Humidors
            .Include(h => h.Cigars)
            .FirstOrDefaultAsync(h => h.Id == humidorId && h.UserId == userId);

        if (humidor == null)
            return false;

        var cigar = await _context.UserCigars
            .FirstOrDefaultAsync(c => c.Id == cigarId && c.UserId == userId);

        if (cigar == null)
            return false;

        if (cigar.HumidorId == humidorId)
            return true; // Already in this humidor

        if (humidor.Cigars.Count >= humidor.Capacity)
            return false; // Capacity exceeded

        cigar.HumidorId = humidorId;
        cigar.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveCigarFromHumidor(int humidorId, int cigarId, int userId)
    {
        var cigar = await _context.UserCigars
            .FirstOrDefaultAsync(c => c.Id == cigarId && c.UserId == userId && c.HumidorId == humidorId);

        if (cigar == null)
            return false;

        cigar.HumidorId = null;
        cigar.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CigarBriefDto>> GetCigarsInHumidor(int humidorId, int userId)
    {
        // First verify the humidor exists and belongs to the user
        var humidorExists = await _context.Humidors
            .AnyAsync(h => h.Id == humidorId && h.UserId == userId);

        if (!humidorExists)
            return new List<CigarBriefDto>();

        // Get cigars in the humidor
        var cigars = await _context.UserCigars
            .Include(c => c.CigarBase)
            .Where(c => c.HumidorId == humidorId && c.UserId == userId)
            .Select(c => new CigarBriefDto
            {
                Id = c.Id,
                Name = c.CigarBase.Name,
                Brand = new BrandDto()
                {
                    Id = c.CigarBase.Brand.Id,
                    Name = c.CigarBase.Brand.Name,
                    Description = c.CigarBase.Brand.Description,
                    UpdatedAt = c.CigarBase.Brand.UpdatedAt,
                    CreatedAt = c.CigarBase.Brand.CreatedAt,
                    Country = c.CigarBase.Brand.Country,
                    IsModerated = c.CigarBase.Brand.IsModerated,
                    LogoBytes = c.CigarBase.Brand.LogoBytes,
                },
                BrandName = c.CigarBase.Brand.Name,
                Size = c.CigarBase.Size,
                Strength = c.CigarBase.Strength,
                Price = c.Price,
                Rating = c.Rating,
            })
            .ToListAsync();

        return cigars;
    }
}