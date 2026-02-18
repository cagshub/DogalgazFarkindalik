using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.Web.Controllers;

public class SimulationsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SimulationsController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var query = "/api/simulations?";

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
            ViewBag.Simulations = JsonSerializer.Deserialize<JsonElement>(json);
        }
        else
        {
            ViewBag.Simulations = JsonSerializer.Deserialize<JsonElement>("[]");
        }

        return View();
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        var response = await client.GetAsync($"/api/simulations/{id}");

        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var json = await response.Content.ReadAsStringAsync();
        ViewBag.Simulation = JsonSerializer.Deserialize<JsonElement>(json);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Submit(Guid id, IFormCollection form)
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        var answers = new List<object>();
        foreach (var key in form.Keys)
        {
            if (key.StartsWith("question_"))
            {
                var questionId = key.Replace("question_", "");
                var optionId = form[key].ToString();
                answers.Add(new { questionId, selectedOptionId = optionId });
            }
        }

        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = JsonSerializer.Serialize(answers);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/simulations/{id}/answers", content);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            ViewBag.Result = JsonSerializer.Deserialize<JsonElement>(json);
            return View("Result");
        }

        ViewBag.Error = "Cevaplar gönderilemedi.";
        return RedirectToAction("Detail", new { id });
    }
}
