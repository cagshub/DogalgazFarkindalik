using DogalgazFarkindalik.Application.DTOs.Auth;
using FluentValidation;

namespace DogalgazFarkindalik.Application.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Doğum tarihi zorunludur.")
            .LessThan(DateTime.UtcNow).WithMessage("Doğum tarihi geçmişte olmalıdır.");

        RuleFor(x => x.SubscriptionType)
            .IsInEnum().WithMessage("Geçerli bir abonelik tipi seçiniz.");
    }
}
