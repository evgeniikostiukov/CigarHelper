using System.ComponentModel.DataAnnotations;

namespace CigarHelper.Data.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Электронная почта обязательна")]
    [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Пароль обязателен")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Имя пользователя может содержать только буквы, цифры, подчеркивания и дефисы")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Электронная почта обязательна")]
    [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
    [StringLength(100, ErrorMessage = "Длина адреса электронной почты не должна превышать 100 символов")]
    public string Email { get; set; } = string.Empty;
    
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
} 