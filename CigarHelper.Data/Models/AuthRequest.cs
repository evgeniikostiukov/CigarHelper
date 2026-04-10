using System.ComponentModel.DataAnnotations;
using CigarHelper.Data.Models.Enums;

namespace CigarHelper.Data.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Логин обязателен")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Логин должен быть от 1 до 50 символов")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Имя пользователя может содержать только буквы, цифры, подчеркивания и дефисы")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z0-9]).*$", ErrorMessage = "Пароль должен содержать минимум одну строчную букву и одну заглавную букву или цифру")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public Role Role { get; set; } // enum вместо string
}