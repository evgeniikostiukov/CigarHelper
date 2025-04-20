using CigarHelper.Data.Data;
using CigarHelper.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CigarsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CigarsController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cigar>>> GetCigars()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cigars = await _context.Cigars
            .Where(c => c.UserId == userId)
            .ToListAsync();
            
        return Ok(cigars);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Cigar>> GetCigar(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cigar = await _context.Cigars
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (cigar == null)
            return NotFound();
            
        return Ok(cigar);
    }
    
    [HttpPost]
    public async Task<ActionResult<Cigar>> CreateCigar(Cigar cigar)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        cigar.UserId = userId;
        cigar.CreatedAt = DateTime.UtcNow;
        
        _context.Cigars.Add(cigar);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetCigar), new { id = cigar.Id }, cigar);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCigar(int id, Cigar cigar)
    {
        if (id != cigar.Id)
            return BadRequest();
            
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var existingCigar = await _context.Cigars
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (existingCigar == null)
            return NotFound();
            
        existingCigar.Name = cigar.Name;
        existingCigar.Brand = cigar.Brand;
        existingCigar.Country = cigar.Country;
        existingCigar.Description = cigar.Description;
        existingCigar.Strength = cigar.Strength;
        existingCigar.Size = cigar.Size;
        existingCigar.Price = cigar.Price;
        existingCigar.Rating = cigar.Rating;
        existingCigar.ImageUrl = cigar.ImageUrl;
        existingCigar.UpdatedAt = DateTime.UtcNow;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CigarExists(id))
                return NotFound();
            else
                throw;
        }
        
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCigar(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cigar = await _context.Cigars
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (cigar == null)
            return NotFound();
            
        _context.Cigars.Remove(cigar);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    private bool CigarExists(int id)
    {
        return _context.Cigars.Any(e => e.Id == id);
    }
} 