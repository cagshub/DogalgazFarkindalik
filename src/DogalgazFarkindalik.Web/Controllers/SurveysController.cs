using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.Web.Controllers;

public class SurveysController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SurveysController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/surveys/active");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            ViewBag.Surveys = JsonSerializer.Deserialize<JsonElement>(json);
        }
        else
        {
            ViewBag.Surveys = JsonSerializer.Deserialize<JsonElement>("[]");
        }

        return View();
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/surveys/{id}");

        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var json = await response.Content.ReadAsStringAsync();
        ViewBag.Survey = JsonSerializer.Deserialize<JsonElement>(json);
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
                var value = form[key].ToString();

                if (key.Contains("_scale"))
                {
                    var qId = questionId.Replace("_scale", "");
                    answers.Add(new { questionId = qId, numericValue = int.Parse(value) });
                }
                else
                {
                    answers.Add(new { questionId, selectedOptionId = value });
                }
            }
        }

        var client = _httpClientFactory.CreateClient("DogalgazAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = JsonSerializer.Serialize(new { answers });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/surveys/{id}/responses", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Yanıtlarınız başarıyla kaydedildi!";
            return RedirectToAction("Index");
        }

        ViewBag.Error = "Yanıtlar gönderilemedi.";
        return RedirectToAction("Detail", new { id });
    }
}
