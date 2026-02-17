using DogalgazFarkindalik.Application.DTOs.Auth;
using FluentValidation;

namespace DogalgazFarkindalik.Application.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
