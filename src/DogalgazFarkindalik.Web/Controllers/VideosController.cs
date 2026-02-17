using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.Web.Controllers;

public class VideosController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public VideosController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> Index(string? ageGroup, string? subscriptionType)
    {
        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        var query = "/api/videos?";

        if (!string.IsNullOrEmpty(ageGroup))
            query += $"ageGroup={ageGroup}&";
        if (!string.IsNullOrEmpty(subscriptionType))
            query += $"subscriptionType={subscriptionType}&";

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

        ViewBag.SelectedAgeGroup = ageGroup;
        ViewBag.SelectedSubscriptionType = subscriptionType;
        return View();
    }
}
