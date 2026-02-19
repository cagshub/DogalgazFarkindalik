using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.Web.Controllers;

public class VideosController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public VideosController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    private HttpClient CreateAuthorizedClient()
    {
        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        var token = HttpContext.Session.GetString("Token");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();

        var query = "/api/videos?";

        var role = HttpContext.Session.GetString("Role");
        var ageGroup = HttpContext.Session.GetString("AgeGroup");
        var subscriptionType = HttpContext.Session.GetString("SubscriptionType");

        if (role != "Admin")
        {
            if (!string.IsNullOrEmpty(ageGroup))
                query += $"ageGroup={ageGroup}&";
            if (!string.IsNullOrEmpty(subscriptionType))
                query += $"subscriptionType={subscriptionType}&";
        }

        var response = await client.GetAsync(query.TrimEnd('&', '?'));

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            ViewBag.Videos = JsonSerializer.Deserialize<JsonElement>(json);
        }
        else
        {
            ViewBag.Videos = JsonSerializer.Deserialize<JsonElement>("[]");
        }

        // Kullanicinin video ilerleme verilerini cek
        var progressResponse = await client.GetAsync("/api/video-progress");
        if (progressResponse.IsSuccessStatusCode)
        {
            var progressJson = await progressResponse.Content.ReadAsStringAsync();
            ViewBag.Progress = JsonSerializer.Deserialize<JsonElement>(progressJson);
        }

        ViewBag.AgeGroup = ageGroup;
        ViewBag.SubscriptionType = subscriptionType;
        return View();
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        var client = CreateAuthorizedClient();

        var response = await client.GetAsync($"/api/videos/{id}");
        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var json = await response.Content.ReadAsStringAsync();
        ViewBag.Video = JsonSerializer.Deserialize<JsonElement>(json);

        // Bu videonun ilerleme durumunu cek
        var progressResponse = await client.GetAsync("/api/video-progress");
        if (progressResponse.IsSuccessStatusCode)
        {
            var progressJson = await progressResponse.Content.ReadAsStringAsync();
            var progressList = JsonSerializer.Deserialize<JsonElement>(progressJson);
            foreach (var p in progressList.EnumerateArray())
            {
                if (p.GetProperty("videoId").GetGuid() == id)
                {
                    ViewBag.VideoProgress = p;
                    break;
                }
            }
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProgress(Guid id, [FromBody] JsonElement body)
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
            return Unauthorized();

        var client = CreateAuthorizedClient();
        var content = new StringContent(body.GetRawText(), Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"/api/video-progress/{id}", content);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        return StatusCode((int)response.StatusCode);
    }
}
