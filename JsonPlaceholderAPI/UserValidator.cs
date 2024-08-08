using FluentValidation;
using JsonPlaceholderAPI.Models;
using Microsoft.EntityFrameworkCore;

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
            .MustAsync(BeUniqueEmail).WithMessage("Email is already in use.");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AllAsync(u => u.Email != email, cancellationToken);
    }
}
