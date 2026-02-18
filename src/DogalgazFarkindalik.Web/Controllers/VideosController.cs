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
        var client = _httpClientFactory.CreateClient("DogalgazAPI");

        var token = HttpContext.Session.GetString("Token");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var query = "/api/videos?";

        var ageGroup = HttpContext.Session.GetString("AgeGroup");
        var subscriptionType = HttpContext.Session.GetString("SubscriptionType");

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

        ViewBag.IsLoggedIn = !string.IsNullOrEmpty(token);
        ViewBag.AgeGroup = ageGroup;
        ViewBag.SubscriptionType = subscriptionType;
        return View();
    }
}
