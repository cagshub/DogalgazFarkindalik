using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using DogalgazFarkindalik.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DogalgazFarkindalik.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string fullName, string verificationToken, CancellationToken ct = default)
    {
        var apiBaseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5001";
        var verificationUrl = $"{apiBaseUrl}/Auth/VerifyEmail?token={verificationToken}";
        var senderName = _configuration["Email:SenderName"] ?? "Dogalgaz Farkindalik Platformu";
        var senderEmail = _configuration["Email:SenderEmail"] ?? "noreply@dogalgaz.com";

        var htmlBody = BuildVerificationHtml(fullName, verificationUrl);

        // 1. Brevo API key varsa Brevo kullan (herkese gonderebilir)
        var brevoApiKey = _configuration["Email:BrevoApiKey"];
        if (!string.IsNullOrEmpty(brevoApiKey))
        {
            await SendViaBrevoAsync(brevoApiKey, toEmail, senderEmail, senderName, htmlBody, ct);
            return;
        }

        // 2. Resend API key varsa Resend kullan
        var resendApiKey = _configuration["Email:ResendApiKey"];
        if (!string.IsNullOrEmpty(resendApiKey))
        {
            await SendViaResendAsync(resendApiKey, toEmail, senderName, htmlBody, ct);
            return;
        }

        // 3. Yoksa SMTP kullan (localhost icin)
        await SendViaSmtpAsync(toEmail, senderName, htmlBody, ct);
    }

    private async Task SendViaBrevoAsync(string apiKey, string toEmail, string senderEmail, string senderName, string htmlBody, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("api-key", apiKey);

        var payload = new
        {
            sender = new { name = senderName, email = senderEmail },
            to = new[] { new { email = toEmail } },
            subject = "E-posta Adresinizi Dogrulayin - Dogalgaz Farkindalik Platformu",
            htmlContent = htmlBody
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content, ct);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Brevo ile dogrulama e-postasi gonderildi: {Email}", toEmail);
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Brevo e-posta hatasi: {Status} - {Body}", response.StatusCode, errorBody);
            throw new InvalidOperationException($"Brevo e-posta gonderilemedi: {response.StatusCode}");
        }
    }

    private async Task SendViaResendAsync(string apiKey, string toEmail, string senderName, string htmlBody, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            from = $"{senderName} <onboarding@resend.dev>",
            to = new[] { toEmail },
            subject = "E-posta Adresinizi Dogrulayin - Dogalgaz Farkindalik Platformu",
            html = htmlBody
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.resend.com/emails", content, ct);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Resend ile dogrulama e-postasi gonderildi: {Email}", toEmail);
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Resend e-posta hatasi: {Status} - {Body}", response.StatusCode, errorBody);
            throw new InvalidOperationException($"Resend e-posta gonderilemedi: {response.StatusCode}");
        }
    }

    private async Task SendViaSmtpAsync(string toEmail, string senderName, string htmlBody, CancellationToken ct)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var senderEmail = _configuration["Email:SenderEmail"];
        var senderPassword = _configuration["Email:SenderPassword"];

        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
        {
            _logger.LogWarning("SMTP ayarlari yapilandirilmamis. E-posta gonderilemedi: {Email}", toEmail);
            throw new InvalidOperationException("SMTP ayarlari eksik.");
        }

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true,
            Timeout = 5000
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "E-posta Adresinizi Dogrulayin - Dogalgaz Farkindalik Platformu",
            Body = htmlBody,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await smtpClient.SendMailAsync(mailMessage, timeoutCts.Token);
            _logger.LogInformation("SMTP ile dogrulama e-postasi gonderildi: {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP e-posta gonderimi basarisiz: {Email}", toEmail);
            throw;
        }
    }

    private static string BuildVerificationHtml(string fullName, string verificationUrl)
    {
        return $@"
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
    }
}
