using FluentValidation;
using TodoApi.Domain.Entities;

namespace TodoApi.Application.Validators;

/// <summary>
/// Validator for Todo entity. Ensures title and description meet requirements.
/// </summary>
public class TodoValidator : AbstractValidator<Todo>
{
    public TodoValidator()
    {
        ApplyCommonRules();
    }

    private void ApplyCommonRules()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Length(1, 200).WithMessage("Title must be between 1 and 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
    }
}
