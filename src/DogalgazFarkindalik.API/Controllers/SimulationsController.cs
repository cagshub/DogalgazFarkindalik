using System.Security.Claims;
using DogalgazFarkindalik.Application.DTOs.Simulation;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.API.Controllers;

/// <summary>
/// Interaktif simulasyon yonetimi (listeleme, cevap gonderme, CRUD)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SimulationsController : ControllerBase
{
    private readonly ISimulationService _simulationService;

    public SimulationsController(ISimulationService simulationService)
        => _simulationService = simulationService;

    /// <summary>
    /// Yas grubu ve abonelik tipine gore filtrelenmis simulasyon listesi getirir
    /// </summary>
    /// <param name="ageGroup">Yas grubu filtresi (Child, Adult, Senior)</param>
    /// <param name="subscriptionType">Abonelik tipi filtresi (Bireysel, Merkezi, Endustriyel)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Filtrelenmis simulasyon listesi</returns>
    /// <response code="200">Simulasyon listesi basariyla getirildi</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<SimulationListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SimulationListDto>>> GetAll(
        [FromQuery] AgeGroup? ageGroup,
        [FromQuery] SubscriptionType? subscriptionType,
        CancellationToken ct)
    {
        var list = await _simulationService.GetSimulationsAsync(ageGroup, subscriptionType, ct);
        return Ok(list);
    }

    /// <summary>
    /// Simulasyon detayini sorular ve seceneklerle birlikte getirir
    /// </summary>
    /// <param name="id">Simulasyon ID (GUID)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Simulasyon detayi (sorular dahil)</returns>
    /// <response code="200">Simulasyon bulundu</response>
    /// <response code="404">Simulasyon bulunamadi</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SimulationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SimulationDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var sim = await _simulationService.GetByIdAsync(id, ct);
        return sim is null ? NotFound() : Ok(sim);
    }

    /// <summary>
    /// Simulasyon cevaplarini gonderir ve puanlanmis sonucu doner (Giris yapilmis olmali)
    /// </summary>
    /// <param name="id">Simulasyon ID</param>
    /// <param name="answers">Her soru icin secilen secenek ID'leri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Puan, dogru/yanlis detaylari ve aciklamalar</returns>
    /// <response code="200">Cevaplar degerlendi, sonuc doner</response>
    /// <response code="401">Yetkisiz erisim - giris yapilmali</response>
    /// <response code="404">Simulasyon bulunamadi</response>
    [HttpPost("{id:guid}/answers")]
    [Authorize]
    [ProducesResponseType(typeof(SimulationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SimulationResultDto>> SubmitAnswers(
        Guid id,
        [FromBody] List<SubmitSimulationAnswerDto> answers,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _simulationService.SubmitAnswersAsync(id, userId, answers, ct);
        return Ok(result);
    }

    /// <summary>
    /// Yeni simulasyon olusturur (sorular ve seceneklerle birlikte). Editor veya Admin yetkisi gerektirir.
    /// </summary>
    /// <param name="dto">Simulasyon bilgileri (baslik, aciklama, sorular, secenekler)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Olusturulan simulasyon detayi</returns>
    /// <response code="201">Simulasyon basariyla olusturuldu</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="403">Yetersiz yetki (Editor veya Admin gerekli)</response>
    [HttpPost]
    [Authorize(Roles = "Editor,Admin")]
    [ProducesResponseType(typeof(SimulationDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SimulationDetailDto>> Create(
        [FromBody] CreateSimulationDto dto,
        CancellationToken ct)
    {
        var sim = await _simulationService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = sim.Id }, sim);
    }

    /// <summary>
    /// Mevcut simulasyonu gunceller (tum sorular ve secenekler yeniden olusturulur). Editor veya Admin yetkisi gerektirir.
    /// </summary>
    /// <param name="id">Guncellenecek simulasyon ID'si</param>
    /// <param name="dto">Yeni simulasyon bilgileri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Guncellenmis simulasyon detayi</returns>
    /// <response code="200">Simulasyon basariyla guncellendi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="404">Simulasyon bulunamadi</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Editor,Admin")]
    [ProducesResponseType(typeof(SimulationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SimulationDetailDto>> Update(
        Guid id,
        [FromBody] CreateSimulationDto dto,
        CancellationToken ct)
    {
        var sim = await _simulationService.UpdateAsync(id, dto, ct);
        return Ok(sim);
    }

    /// <summary>
    /// Simulasyonu kalici olarak siler (cascade: sorular ve secenekler de silinir). Sadece Admin yetkisi.
    /// </summary>
    /// <param name="id">Silinecek simulasyon ID'si</param>
    /// <param name="ct">Cancellation token</param>
    /// <response code="204">Simulasyon basariyla silindi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="403">Yetersiz yetki (Admin gerekli)</response>
    /// <response code="404">Simulasyon bulunamadi</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _simulationService.DeleteAsync(id, ct);
        return NoContent();
    }
}
