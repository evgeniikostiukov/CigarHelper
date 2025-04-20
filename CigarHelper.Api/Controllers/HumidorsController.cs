using CigarHelper.Api.Services;
using CigarHelper.Data.Models;
using CigarHelper.Data.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CigarHelper.Api.Controllers;

/// <summary>
/// API для управления хьюмидорами и сигарами в них
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class HumidorsController : ControllerBase
{
    private readonly IHumidorService _humidorService;

    public HumidorsController(IHumidorService humidorService)
    {
        _humidorService = humidorService;
    }
    
    /// <summary>
    /// Получает список всех хьюмидоров текущего пользователя
    /// </summary>
    /// <returns>Список хьюмидоров</returns>
    /// <response code="200">Успешное получение списка хьюмидоров</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<HumidorResponseDto>>> GetHumidors()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var humidors = await _humidorService.GetUserHumidors(userId);
        return Ok(humidors);
    }
    
    /// <summary>
    /// Получает детальную информацию о хьюмидоре по ID
    /// </summary>
    /// <param name="id">ID хьюмидора</param>
    /// <returns>Детальная информация о хьюмидоре и сигарах в нем</returns>
    /// <response code="200">Хьюмидор найден</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Хьюмидор не найден</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HumidorDetailResponseDto>> GetHumidor(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var humidor = await _humidorService.GetHumidorById(id, userId);
            
        if (humidor == null)
            return NotFound();
            
        return Ok(humidor);
    }
    
    /// <summary>
    /// Создает новый хьюмидор
    /// </summary>
    /// <param name="humidorDto">Данные хьюмидора</param>
    /// <returns>Созданный хьюмидор</returns>
    /// <response code="201">Хьюмидор успешно создан</response>
    /// <response code="400">Некорректные данные</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<HumidorResponseDto>> CreateHumidor(HumidorCreateDto humidorDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var humidor = await _humidorService.CreateHumidor(humidorDto, userId);
        
        return CreatedAtAction(nameof(GetHumidor), new { id = humidor.Id }, humidor);
    }
    
    /// <summary>
    /// Обновляет существующий хьюмидор
    /// </summary>
    /// <param name="id">ID хьюмидора</param>
    /// <param name="humidorDto">Новые данные хьюмидора</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Хьюмидор успешно обновлен</response>
    /// <response code="400">Некорректные данные</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Хьюмидор не найден</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHumidor(int id, HumidorUpdateDto humidorDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _humidorService.UpdateHumidor(id, humidorDto, userId);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }
    
    /// <summary>
    /// Удаляет хьюмидор
    /// </summary>
    /// <param name="id">ID хьюмидора</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Хьюмидор успешно удален</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Хьюмидор не найден</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHumidor(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _humidorService.DeleteHumidor(id, userId);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }
    
    /// <summary>
    /// Добавляет сигару в хьюмидор
    /// </summary>
    /// <param name="humidorId">ID хьюмидора</param>
    /// <param name="cigarId">ID сигары</param>
    /// <returns>Результат операции</returns>
    /// <response code="200">Сигара успешно добавлена в хьюмидор</response>
    /// <response code="400">Ошибка при добавлении (например, превышена вместимость)</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpPost("{humidorId}/cigars/{cigarId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddCigarToHumidor(int humidorId, int cigarId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _humidorService.AddCigarToHumidor(humidorId, cigarId, userId);
        
        if (!result)
            return BadRequest("Failed to add cigar to humidor. Either the humidor or cigar does not exist, or the humidor is at capacity.");
            
        return Ok();
    }
    
    /// <summary>
    /// Удаляет сигару из хьюмидора
    /// </summary>
    /// <param name="humidorId">ID хьюмидора</param>
    /// <param name="cigarId">ID сигары</param>
    /// <returns>Результат операции</returns>
    /// <response code="200">Сигара успешно удалена из хьюмидора</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Сигара не найдена в указанном хьюмидоре</response>
    [HttpDelete("{humidorId}/cigars/{cigarId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCigarFromHumidor(int humidorId, int cigarId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _humidorService.RemoveCigarFromHumidor(humidorId, cigarId, userId);
        
        if (!result)
            return NotFound("Cigar not found in the specified humidor");
            
        return Ok();
    }
    
    /// <summary>
    /// Получает список сигар в хьюмидоре
    /// </summary>
    /// <param name="humidorId">ID хьюмидора</param>
    /// <returns>Список сигар в хьюмидоре</returns>
    /// <response code="200">Успешное получение списка сигар</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpGet("{humidorId}/cigars")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<CigarBriefDto>>> GetCigarsInHumidor(int humidorId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cigars = await _humidorService.GetCigarsInHumidor(humidorId, userId);
        
        return Ok(cigars);
    }
} 