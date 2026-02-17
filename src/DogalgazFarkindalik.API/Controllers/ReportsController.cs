using DogalgazFarkindalik.Application.DTOs.Report;
using DogalgazFarkindalik.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.API.Controllers;

/// <summary>
/// Raporlama endpoint'leri (sadece Admin yetkisi)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
        => _reportService = reportService;

    /// <summary>
    /// Genel platform ozet raporu getirir (toplam kullanici, deneme sayisi, ortalama puan, segment kirilimlari)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Toplam kullanici, deneme, ortalama puan ve segment bazli istatistikler</returns>
    /// <response code="200">Rapor basariyla getirildi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="403">Yetersiz yetki (Admin gerekli)</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(SummaryReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SummaryReportDto>> GetSummary(CancellationToken ct)
    {
        var report = await _reportService.GetSummaryAsync(ct);
        return Ok(report);
    }

    /// <summary>
    /// Yas grubu ve abonelik tipine gore segment bazli rapor getirir
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Segment bazli kullanici, deneme ve puan istatistikleri</returns>
    /// <response code="200">Segment raporu basariyla getirildi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="403">Yetersiz yetki (Admin gerekli)</response>
    [HttpGet("by-segment")]
    [ProducesResponseType(typeof(SummaryReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SummaryReportDto>> GetBySegment(CancellationToken ct)
    {
        var report = await _reportService.GetSummaryAsync(ct);
        return Ok(report);
    }
}
