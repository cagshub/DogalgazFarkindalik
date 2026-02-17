namespace DogalgazFarkindalik.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string fullName, string verificationToken, CancellationToken ct = default);
}
