using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.Web.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        var payload = JsonSerializer.Serialize(new { email, password });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/auth/login", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        if (response.IsSuccessStatusCode)
        {
            HttpContext.Session.SetString("Token", result.GetProperty("accessToken").GetString()!);
            HttpContext.Session.SetString("FullName", result.GetProperty("fullName").GetString()!);
            HttpContext.Session.SetString("Role", result.GetProperty("role").GetString()!);
            HttpContext.Session.SetString("Email", result.GetProperty("email").GetString()!);

            if (result.TryGetProperty("ageGroup", out var ag) && ag.ValueKind != JsonValueKind.Null)
                HttpContext.Session.SetString("AgeGroup", ag.GetString()!);
            if (result.TryGetProperty("subscriptionType", out var st) && st.ValueKind != JsonValueKind.Null)
                HttpContext.Session.SetString("SubscriptionType", st.GetString()!);

            var role = result.GetProperty("role").GetString();
            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Index", "Home");
        }

        // Check if email is not verified
        if (result.TryGetProperty("emailNotVerified", out var emailNotVerified) && emailNotVerified.GetBoolean())
        {
            ViewBag.EmailNotVerified = true;
            ViewBag.Email = email;
            ViewBag.Error = result.GetProperty("message").GetString();
            return View();
        }

        ViewBag.Error = result.TryGetProperty("message", out var msg) ? msg.GetString() : "E-posta veya sifre hatali.";
        return View();
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string email, string password, string fullName, DateTime birthDate, string subscriptionType)
    {
        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        var payload = JsonSerializer.Serialize(new
        {
            email,
            password,
            fullName,
            birthDate = birthDate.ToString("yyyy-MM-ddTHH:mm:ss"),
            subscriptionType = int.Parse(subscriptionType)
        });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/auth/register", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["VerificationEmail"] = email;
            return RedirectToAction("VerificationPending");
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);
        ViewBag.Error = result.TryGetProperty("message", out var msg) ? msg.GetString() : "Kayit basarisiz. Lutfen bilgilerinizi kontrol edin.";
        return View();
    }

    [HttpGet]
    public IActionResult VerificationPending()
    {
        ViewBag.Email = TempData["VerificationEmail"] as string;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            ViewBag.Success = false;
            ViewBag.Message = "Gecersiz dogrulama linki.";
            return View();
        }

        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        var response = await client.GetAsync($"/api/auth/verify-email?token={token}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        ViewBag.Success = response.IsSuccessStatusCode;
        ViewBag.Message = result.TryGetProperty("message", out var msg) ? msg.GetString() : "Bir hata olustu.";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ResendVerification(string email)
    {
        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        var payload = JsonSerializer.Serialize(new { email });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/auth/resend-verification", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        TempData["VerificationEmail"] = email;
        TempData["ResendMessage"] = result.TryGetProperty("message", out var msg) ? msg.GetString() : "Islem tamamlandi.";
        TempData["ResendSuccess"] = response.IsSuccessStatusCode;

        return RedirectToAction("VerificationPending");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
