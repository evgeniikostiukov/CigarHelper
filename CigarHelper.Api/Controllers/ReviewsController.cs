using System.Security.Claims;
using CigarHelper.Api.Services;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    
    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }
    
    /// <summary>
    /// Получает список всех обзоров
    /// </summary>
    /// <param name="userId">Фильтр по пользователю (опционально)</param>
    /// <param name="cigarId">Фильтр по сигаре (опционально)</param>
    /// <returns>Список обзоров</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReviewListItemDto>>> GetReviews(
        [FromQuery] int? userId = null,
        [FromQuery] int? cigarId = null)
    {
        var reviews = await _reviewService.GetReviewsAsync(userId, cigarId);
        return Ok(reviews);
    }
    
    /// <summary>
    /// Получает детальную информацию об обзоре по его ID
    /// </summary>
    /// <param name="id">ID обзора</param>
    /// <returns>Детальная информация об обзоре</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> GetReviewById(int id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        
        if (review == null)
            return NotFound(new { message = $"Обзор с ID {id} не найден" });
            
        return Ok(review);
    }
    
    /// <summary>
    /// Создает новый обзор
    /// </summary>
    /// <param name="request">Данные для создания обзора</param>
    /// <returns>Созданный обзор</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var createdReview = await _reviewService.CreateReviewAsync(currentUserId, request);
            
            return CreatedAtAction(
                nameof(GetReviewById),
                new { id = createdReview.Id },
                createdReview);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// Обновляет существующий обзор
    /// </summary>
    /// <param name="id">ID обзора</param>
    /// <param name="request">Данные для обновления обзора</param>
    /// <returns>Обновленный обзор</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> UpdateReview(int id, UpdateReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updatedReview = await _reviewService.UpdateReviewAsync(id, currentUserId, request);
            
            if (updatedReview == null)
                return NotFound(new { message = $"Обзор с ID {id} не найден" });
                
            return Ok(updatedReview);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
    
    /// <summary>
    /// Удаляет обзор
    /// </summary>
    /// <param name="id">ID обзора</param>
    /// <returns>Результат операции</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteReview(int id)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _reviewService.DeleteReviewAsync(id, currentUserId);
            
            if (!result)
                return NotFound(new { message = $"Обзор с ID {id} не найден" });
                
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
} 