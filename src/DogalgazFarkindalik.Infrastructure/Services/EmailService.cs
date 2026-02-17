using System.Net;
using System.Net.Mail;
using DogalgazFarkindalik.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DogalgazFarkindalik.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string fullName, string verificationToken, CancellationToken ct = default)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var senderEmail = _configuration["Email:SenderEmail"];
        var senderPassword = _configuration["Email:SenderPassword"];
        var senderName = _configuration["Email:SenderName"] ?? "Dogalgaz Farkindalik Platformu";

        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
        {
            _logger.LogWarning("SMTP ayarlari yapilandirilmamis. E-posta gonderilemedi: {Email}, Token: {Token}", toEmail, verificationToken);
            return;
        }

        var apiBaseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5001";
        var verificationUrl = $"{apiBaseUrl}/Auth/VerifyEmail?token={verificationToken}";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8""/>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 30px auto; background: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #0d6efd, #0b5ed7); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .body {{ padding: 30px; }}
        .body h2 {{ color: #333; }}
        .body p {{ color: #555; line-height: 1.6; }}
        .btn {{ display: inline-block; background: #0d6efd; color: white !important; text-decoration: none; padding: 14px 30px; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Dogalgaz Farkindalik Platformu</h1>
        </div>
        <div class=""body"">
            <h2>Merhaba {fullName},</h2>
            <p>Dogalgaz Farkindalik Platformu'na kaydoldugunuz icin tesekkur ederiz!</p>
            <p>Hesabinizi aktif hale getirmek icin asagidaki butona tiklayarak e-posta adresinizi dogrulayin:</p>
            <p style=""text-align: center;"">
                <a href=""{verificationUrl}"" class=""btn"">E-posta Adresimi Dogrula</a>
            </p>
            <p>Buton calismiyorsa asagidaki linki tarayiciniza yapistiriniz:</p>
            <p style=""word-break: break-all; font-size: 13px; color: #0d6efd;"">{verificationUrl}</p>
            <p><strong>Bu link 24 saat gecerlidir.</strong></p>
            <p>Eger bu kaydi siz yapmadiyseniz, bu e-postayi gormezden gelebilirsiniz.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 Dogalgaz Farkindalik Platformu. Tum haklari saklidir.</p>
        </div>
    </div>
</body>
</html>";

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "E-posta Adresinizi Dogrulayin - Dogalgaz Farkindalik Platformu",
            Body = htmlBody,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        try
        {
            await smtpClient.SendMailAsync(mailMessage, ct);
            _logger.LogInformation("Dogrulama e-postasi gonderildi: {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gonderimi basarisiz: {Email}", toEmail);
            throw;
        }
    }
}
