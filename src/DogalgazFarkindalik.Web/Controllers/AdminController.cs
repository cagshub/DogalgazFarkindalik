using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.Web.Controllers;

public class AdminController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AdminController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    private HttpClient CreateAuthorizedClient()
    {
        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        var token = HttpContext.Session.GetString("Token");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private bool IsAdmin()
    {
        var token = HttpContext.Session.GetString("Token");
        var role = HttpContext.Session.GetString("Role");
        return !string.IsNullOrEmpty(token) && role == "Admin";
    }

    // ========== DASHBOARD ==========

    public async Task<IActionResult> Dashboard()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var response = await client.GetAsync("/api/reports/summary");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            ViewBag.Report = JsonSerializer.Deserialize<JsonElement>(json);
        }

        return View();
    }

    // ========== VIDEO CRUD ==========

    public async Task<IActionResult> Videos()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var response = await client.GetAsync("/api/videos");
        var videos = new List<JsonElement>();

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            videos = JsonSerializer.Deserialize<List<JsonElement>>(json, JsonOpts) ?? [];
        }

        return View(videos);
    }

    [HttpGet]
    public IActionResult CreateVideo()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateVideo(
        string title, string description, string url, int durationSec,
        string tags, string minAgeGroup, string? subscriptionFilter, string thumbnailUrl)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var body = new
        {
            title, description, url, durationSec, tags,
            minAgeGroup = Enum.Parse<Domain.Enums.AgeGroup>(minAgeGroup),
            subscriptionFilter = string.IsNullOrEmpty(subscriptionFilter) ? (object?)null : Enum.Parse<Domain.Enums.SubscriptionType>(subscriptionFilter),
            thumbnailUrl
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/videos", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Video basariyla eklendi.";
            return RedirectToAction("Videos");
        }

        TempData["Error"] = "Video eklenirken hata olustu.";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditVideo(Guid id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var response = await client.GetAsync($"/api/videos/{id}");

        if (!response.IsSuccessStatusCode) return RedirectToAction("Videos");

        var json = await response.Content.ReadAsStringAsync();
        var video = JsonSerializer.Deserialize<JsonElement>(json, JsonOpts);
        return View(video);
    }

    [HttpPost]
    public async Task<IActionResult> EditVideo(
        Guid id, string title, string description, string url, int durationSec,
        string tags, string minAgeGroup, string? subscriptionFilter, string thumbnailUrl)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var body = new
        {
            title, description, url, durationSec, tags,
            minAgeGroup = Enum.Parse<Domain.Enums.AgeGroup>(minAgeGroup),
            subscriptionFilter = string.IsNullOrEmpty(subscriptionFilter) ? (object?)null : Enum.Parse<Domain.Enums.SubscriptionType>(subscriptionFilter),
            thumbnailUrl
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"/api/videos/{id}", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Video basariyla guncellendi.";
            return RedirectToAction("Videos");
        }

        TempData["Error"] = "Video guncellenirken hata olustu.";
        return RedirectToAction("EditVideo", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteVideo(Guid id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        await client.DeleteAsync($"/api/videos/{id}");

        TempData["Success"] = "Video silindi.";
        return RedirectToAction("Videos");
    }

    // ========== SIMULATION CRUD ==========

    public async Task<IActionResult> Simulations()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var response = await client.GetAsync("/api/simulations");
        var simulations = new List<JsonElement>();

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            simulations = JsonSerializer.Deserialize<List<JsonElement>>(json, JsonOpts) ?? [];
        }

        return View(simulations);
    }

    [HttpGet]
    public IActionResult CreateSimulation()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateSimulation([FromForm] string formData)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var content = new StringContent(formData, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/simulations", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Simulasyon basariyla olusturuldu.";
            return RedirectToAction("Simulations");
        }

        TempData["Error"] = "Simulasyon olusturulurken hata olustu.";
        return RedirectToAction("CreateSimulation");
    }

    [HttpGet]
    public async Task<IActionResult> EditSimulation(Guid id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var response = await client.GetAsync($"/api/simulations/{id}");

        if (!response.IsSuccessStatusCode) return RedirectToAction("Simulations");

        var json = await response.Content.ReadAsStringAsync();
        var simulation = JsonSerializer.Deserialize<JsonElement>(json, JsonOpts);
        return View(simulation);
    }

    [HttpPost]
    public async Task<IActionResult> EditSimulation(Guid id, [FromForm] string formData)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var content = new StringContent(formData, Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"/api/simulations/{id}", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Simulasyon basariyla guncellendi.";
            return RedirectToAction("Simulations");
        }

        TempData["Error"] = "Simulasyon guncellenirken hata olustu.";
        return RedirectToAction("EditSimulation", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSimulation(Guid id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        await client.DeleteAsync($"/api/simulations/{id}");

        TempData["Success"] = "Simulasyon silindi.";
        return RedirectToAction("Simulations");
    }

    // ========== SURVEY CRUD ==========

    public async Task<IActionResult> Surveys()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var response = await client.GetAsync("/api/surveys/active");
        var surveys = new List<JsonElement>();

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            surveys = JsonSerializer.Deserialize<List<JsonElement>>(json, JsonOpts) ?? [];
        }

        return View(surveys);
    }

    [HttpGet]
    public IActionResult CreateSurvey()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateSurvey([FromForm] string formData)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var content = new StringContent(formData, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/surveys", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Anket basariyla olusturuldu.";
            return RedirectToAction("Surveys");
        }

        TempData["Error"] = "Anket olusturulurken hata olustu.";
        return RedirectToAction("CreateSurvey");
    }

    [HttpGet]
    public async Task<IActionResult> EditSurvey(Guid id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var response = await client.GetAsync($"/api/surveys/{id}");

        if (!response.IsSuccessStatusCode) return RedirectToAction("Surveys");

        var json = await response.Content.ReadAsStringAsync();
        var survey = JsonSerializer.Deserialize<JsonElement>(json, JsonOpts);
        return View(survey);
    }

    [HttpPost]
    public async Task<IActionResult> EditSurvey(Guid id, [FromForm] string formData)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        var content = new StringContent(formData, Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"/api/surveys/{id}", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Anket basariyla guncellendi.";
            return RedirectToAction("Surveys");
        }

        TempData["Error"] = "Anket guncellenirken hata olustu.";
        return RedirectToAction("EditSurvey", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSurvey(Guid id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();
        await client.DeleteAsync($"/api/surveys/{id}");

        TempData["Success"] = "Anket silindi.";
        return RedirectToAction("Surveys");
    }
}
