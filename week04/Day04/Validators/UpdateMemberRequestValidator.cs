using Day03.DTO;
using FluentValidation;

namespace Day03.Validators
{
    public class UpdateMemberRequestValidator : AbstractValidator<UpdateMemberRequest>
    {
        public UpdateMemberRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("The name is required.")
                .MaximumLength(100).WithMessage("The name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("The email format is incorrect.");

            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("The email format is incorrect.")
            .MaximumLength(150).WithMessage("Email address cannot exceed 150 characters.");
        }
    }
}
