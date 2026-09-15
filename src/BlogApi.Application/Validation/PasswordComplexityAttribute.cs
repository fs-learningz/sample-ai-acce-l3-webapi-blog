using System.ComponentModel.DataAnnotations;

namespace BlogApi.Application.Validation;

public class PasswordComplexityAttribute : ValidationAttribute
{
    public PasswordComplexityAttribute()
        : base("Password must be at least 10 characters long and contain both letters and numbers.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not string password)
        {
            return false;
        }

        return password.Length >= 10
            && password.Any(char.IsLetter)
            && password.Any(char.IsDigit);
    }
}
