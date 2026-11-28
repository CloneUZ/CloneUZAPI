using System.Net.Mail;
using System.Text.RegularExpressions;
using CloneUZ.Api.Contracts.Auth;

namespace CloneUZ.Api.Services;

public static partial class AuthValidator
{
    public static List<string> ValidateRegister(RegisterRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Trim().Length < 2)
            errors.Add("Ім'я має містити щонайменше 2 символи.");

        if (string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Trim().Length < 2)
            errors.Add("Прізвище має містити щонайменше 2 символи.");

        if (!IsValidEmail(request.Email))
            errors.Add("Некоректний формат електронної пошти.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            errors.Add("Пароль має містити щонайменше 6 символів.");

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !PhoneRegex().IsMatch(request.PhoneNumber.Trim()))
            errors.Add("Некоректний формат номера телефону.");

        if (request.BirthDate.HasValue && request.BirthDate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            errors.Add("Дата народження не може бути в майбутньому.");

        return errors;
    }

    public static List<string> ValidateLogin(LoginRequest request)
    {
        var errors = new List<string>();

        if (!IsValidEmail(request.Email))
            errors.Add("Вкажіть коректний email.");

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Введіть пароль.");

        return errors;
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new MailAddress(email.Trim());
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    [GeneratedRegex(@"^\+?[0-9\s\-()]{10,20}$")]
    private static partial Regex PhoneRegex();
}