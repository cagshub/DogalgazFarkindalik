using System.Security.Claims;
using DogalgazFarkindalik.Application.DTOs.Video;
using DogalgazFarkindalik.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.API.Controllers;

/// <summary>
/// Video izlenme ilerleme takibi — kullanicinin hangi videoyu ne kadar izledigini kaydeder ve getirir
/// </summary>
[ApiController]
[Route("api/video-progress")]
[Authorize]
public class VideoProgressController : ControllerBase
{
    private readonly IVideoProgressService _service;

    public VideoProgressController(IVideoProgressService service)
        => _service = service;

    /// <summary>
    /// Giris yapmis kullanicinin tum video izlenme ilerlemelerini getirir
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Her video icin izlenme yuzdesi, izlenen sure ve tamamlanma durumu</returns>
    /// <response code="200">Ilerleme listesi basariyla getirildi</response>
    /// <response code="401">Yetkisiz erisim - giris yapilmali</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<VideoProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<VideoProgressDto>>> GetMyProgress(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var progress = await _service.GetUserProgressAsync(userId, ct);
        return Ok(progress);
    }

    /// <summary>
    /// Belirtilen video icin izlenme ilerlemesini gunceller (izlenen sure ve yuzde)
    /// </summary>
    /// <param name="videoId">Video ID (GUID)</param>
    /// <param name="dto">Ilerleme bilgileri (izlenen sure saniye cinsinden, ilerleme yuzdesi)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Guncellenmis ilerleme bilgileri</returns>
    /// <response code="200">Ilerleme basariyla guncellendi</response>
    /// <response code="401">Yetkisiz erisim - giris yapilmali</response>
    /// <response code="404">Video bulunamadi</response>
    [HttpPut("{videoId:guid}")]
    [ProducesResponseType(typeof(VideoProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VideoProgressDto>> UpdateProgress(
        Guid videoId, [FromBody] UpdateVideoProgressDto dto, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var progress = await _service.UpdateProgressAsync(userId, videoId, dto, ct);
        return Ok(progress);
    }
}
