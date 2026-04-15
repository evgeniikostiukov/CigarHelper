using System.ComponentModel.DataAnnotations;
using CigarHelper.Data.Models.Dtos;
using CigarHelper.Data.Models.Enums;

namespace CigarHelper.Api.Models;

public class MyProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsProfilePublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Имя пользователя может содержать только буквы, цифры, подчеркивания и дефисы")]
    public string Username { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
    [StringLength(100, ErrorMessage = "Длина адреса электронной почты не должна превышать 100 символов")]
    public string? Email { get; set; }

    public bool IsProfilePublic { get; set; }
}

public class UpdateProfileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MyProfileDto? Profile { get; set; }
    public string? NewToken { get; set; }
}

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Текущий пароль обязателен")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новый пароль обязателен")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z0-9]).*$", ErrorMessage = "Пароль должен содержать минимум одну строчную букву и одну заглавную букву или цифру")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ChangePasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? RetryAfterSeconds { get; set; }
}

public class PublicProfileDto
{
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public IReadOnlyList<HumidorResponseDto> Humidors { get; set; } = [];
}

/// <summary>
/// Лёгкий ответ для префетча: виден ли публичный профиль (пользователь есть и IsProfilePublic).
/// Не различает «нет пользователя» и «профиль закрыт» — в обоих случаях IsVisible = false.
/// </summary>
public class PublicProfileVisibilityDto
{
    public bool IsVisible { get; set; }
}
