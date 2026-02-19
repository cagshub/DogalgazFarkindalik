using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.Web.Controllers;

public class VideosController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public VideosController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var query = "/api/videos?";

        var role = HttpContext.Session.GetString("Role");
        var ageGroup = HttpContext.Session.GetString("AgeGroup");
        var subscriptionType = HttpContext.Session.GetString("SubscriptionType");

        // Admin tum icerikleri gorur, filtreleme yapilmaz
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

        ViewBag.AgeGroup = ageGroup;
        ViewBag.SubscriptionType = subscriptionType;
        return View();
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/videos/{id}");
        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var json = await response.Content.ReadAsStringAsync();
        ViewBag.Video = JsonSerializer.Deserialize<JsonElement>(json);
        return View();
    }
}
