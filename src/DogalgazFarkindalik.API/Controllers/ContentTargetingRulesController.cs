using DogalgazFarkindalik.Application.DTOs.ContentTargeting;
using DogalgazFarkindalik.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.API.Controllers;

/// <summary>
/// Icerik hedefleme kurallari yonetimi — yas grubu ve abonelik tipine gore puan carpanlari (Sadece Admin)
/// </summary>
[ApiController]
[Route("api/admin/content-targeting-rules")]
[Authorize(Roles = "Admin")]
public class ContentTargetingRulesController : ControllerBase
{
    private readonly IContentTargetingRuleService _service;

    public ContentTargetingRulesController(IContentTargetingRuleService service)
        => _service = service;

    /// <summary>
    /// Tum icerik hedefleme kurallarini listeler
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Hedefleme kurallari listesi (modul, yas grubu, abonelik tipi, puan carpani)</returns>
    /// <response code="200">Kurallar basariyla getirildi</response>
    /// <response code="401">Yetkisiz erisim - giris yapilmali</response>
    /// <response code="403">Yetersiz yetki (Admin gerekli)</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ContentTargetingRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ContentTargetingRuleDto>>> GetAll(CancellationToken ct)
    {
        var rules = await _service.GetAllAsync(ct);
        return Ok(rules);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip hedefleme kuralini getirir
    /// </summary>
    /// <param name="id">Kural ID (GUID)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Hedefleme kurali detayi</returns>
    /// <response code="200">Kural bulundu</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="404">Kural bulunamadi</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContentTargetingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentTargetingRuleDto>> GetById(Guid id, CancellationToken ct)
    {
        var rule = await _service.GetByIdAsync(id, ct);
        return rule is null ? NotFound() : Ok(rule);
    }

    /// <summary>
    /// Yeni icerik hedefleme kurali olusturur (ornegin: 65+ yas grubu icin x1.2 puan carpani)
    /// </summary>
    /// <param name="dto">Kural bilgileri (modul tipi, yas grubu, abonelik tipi, puan carpani, aciklama)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Olusturulan hedefleme kurali</returns>
    /// <response code="201">Kural basariyla olusturuldu</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="403">Yetersiz yetki (Admin gerekli)</response>
    [HttpPost]
    [ProducesResponseType(typeof(ContentTargetingRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ContentTargetingRuleDto>> Create(
        [FromBody] CreateContentTargetingRuleDto dto, CancellationToken ct)
    {
        var rule = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Mevcut hedefleme kuralini gunceller
    /// </summary>
    /// <param name="id">Guncellenecek kural ID'si</param>
    /// <param name="dto">Yeni kural bilgileri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Guncellenmis hedefleme kurali</returns>
    /// <response code="200">Kural basariyla guncellendi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="404">Kural bulunamadi</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContentTargetingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentTargetingRuleDto>> Update(
        Guid id, [FromBody] CreateContentTargetingRuleDto dto, CancellationToken ct)
    {
        var rule = await _service.UpdateAsync(id, dto, ct);
        return Ok(rule);
    }

    /// <summary>
    /// Hedefleme kuralini kalici olarak siler
    /// </summary>
    /// <param name="id">Silinecek kural ID'si</param>
    /// <param name="ct">Cancellation token</param>
    /// <response code="204">Kural basariyla silindi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="404">Kural bulunamadi</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
