using FluentValidation;
using JsonPlaceholderAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;  // LINQ işlemleri için bu ad alanını eklemeniz gerekebilir

public class UserValidator : AbstractValidator<User>
{
    private readonly AppDbContext _context;

    public UserValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(user => user.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(6).WithMessage("Username must be at least 6 characters long.");

        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .Must(BeUniqueEmail).WithMessage("Email is already in use.");
    }

    private bool BeUniqueEmail(string email)
    {
        // Kullanıcıların e-posta adreslerini senkron olarak kontrol eder
        return !_context.Users.Any(u => u.Email == email);
    }
}
