using System.Security.Claims;
using DogalgazFarkindalik.Application.DTOs.Survey;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.API.Controllers;

/// <summary>
/// Anket yonetimi (listeleme, yanit gonderme, CRUD)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SurveysController : ControllerBase
{
    private readonly ISurveyService _surveyService;

    public SurveysController(ISurveyService surveyService)
        => _surveyService = surveyService;

    /// <summary>
    /// Aktif anketleri listeler (yas grubu ve abonelik tipine gore filtreleme destekler)
    /// </summary>
    /// <param name="ageGroup">Yas grubu filtresi (Child, Adult, Senior)</param>
    /// <param name="subscriptionType">Abonelik tipi filtresi (Bireysel, Merkezi, Endustriyel)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Aktif anket listesi</returns>
    /// <response code="200">Anket listesi basariyla getirildi</response>
    /// <response code="401">Yetkisiz erisim - giris yapilmali</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<SurveyListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SurveyListDto>>> GetActive(
        [FromQuery] AgeGroup? ageGroup,
        [FromQuery] SubscriptionType? subscriptionType,
        CancellationToken ct)
    {
        var list = await _surveyService.GetActiveSurveysAsync(ageGroup, subscriptionType, ct);
        return Ok(list);
    }

    /// <summary>
    /// Anket detayini sorular ve seceneklerle birlikte getirir
    /// </summary>
    /// <param name="id">Anket ID (GUID)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Anket detayi (sorular ve secenekler dahil)</returns>
    /// <response code="200">Anket bulundu</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="404">Anket bulunamadi</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SurveyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var survey = await _surveyService.GetByIdAsync(id, ct);
        return survey is null ? NotFound() : Ok(survey);
    }

    /// <summary>
    /// Anket yanitlarini kaydeder ve puanlama yapar
    /// </summary>
    /// <param name="id">Anket ID</param>
    /// <param name="dto">Yanit listesi (her soru icin secilen secenek veya sayisal deger)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Basari mesaji</returns>
    /// <response code="200">Yanitlar basariyla kaydedildi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="404">Anket bulunamadi</response>
    [HttpPost("{id:guid}/responses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitResponses(
        Guid id,
        [FromBody] SubmitSurveyResponseDto dto,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _surveyService.SubmitResponsesAsync(id, userId, dto, ct);
        return Ok(new { message = "Yanitlariniz basariyla kaydedildi." });
    }

    /// <summary>
    /// Yeni anket olusturur (sorular ve seceneklerle birlikte). Editor veya Admin yetkisi gerektirir.
    /// </summary>
    /// <param name="dto">Anket bilgileri (baslik, aciklama, aktiflik durumu, sorular, secenekler)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Olusturulan anket detayi</returns>
    /// <response code="201">Anket basariyla olusturuldu</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="403">Yetersiz yetki (Editor veya Admin gerekli)</response>
    [HttpPost]
    [Authorize(Roles = "Editor,Admin")]
    [ProducesResponseType(typeof(SurveyDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SurveyDetailDto>> Create(
        [FromBody] CreateSurveyDto dto,
        CancellationToken ct)
    {
        var survey = await _surveyService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = survey.Id }, survey);
    }

    /// <summary>
    /// Mevcut anketi gunceller (tum sorular ve secenekler yeniden olusturulur). Editor veya Admin yetkisi gerektirir.
    /// </summary>
    /// <param name="id">Guncellenecek anket ID'si</param>
    /// <param name="dto">Yeni anket bilgileri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Guncellenmis anket detayi</returns>
    /// <response code="200">Anket basariyla guncellendi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="404">Anket bulunamadi</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Editor,Admin")]
    [ProducesResponseType(typeof(SurveyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDetailDto>> Update(
        Guid id,
        [FromBody] CreateSurveyDto dto,
        CancellationToken ct)
    {
        var survey = await _surveyService.UpdateAsync(id, dto, ct);
        return Ok(survey);
    }

    /// <summary>
    /// Anketi kalici olarak siler (cascade: sorular ve secenekler de silinir). Sadece Admin yetkisi.
    /// </summary>
    /// <param name="id">Silinecek anket ID'si</param>
    /// <param name="ct">Cancellation token</param>
    /// <response code="204">Anket basariyla silindi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="403">Yetersiz yetki (Admin gerekli)</response>
    /// <response code="404">Anket bulunamadi</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _surveyService.DeleteAsync(id, ct);
        return NoContent();
    }
}
