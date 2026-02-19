using System.Security.Claims;
using DogalgazFarkindalik.Application.DTOs.Video;
using DogalgazFarkindalik.Application.Interfaces;
using DogalgazFarkindalik.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.API.Controllers;

/// <summary>
/// Egitim videolari yonetimi (listeleme, filtreleme, CRUD)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly IVideoService _videoService;

    public VideosController(IVideoService videoService) => _videoService = videoService;

    /// <summary>
    /// Yas grubu ve abonelik tipine gore filtrelenmis video listesi getirir
    /// </summary>
    /// <param name="ageGroup">Yas grubu filtresi (Child, Adult, Senior)</param>
    /// <param name="subscriptionType">Abonelik tipi filtresi (Bireysel, Merkezi, Endustriyel)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Filtrelenmis video listesi</returns>
    /// <response code="200">Video listesi basariyla getirildi</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<VideoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VideoDto>>> GetVideos(
        [FromQuery] AgeGroup? ageGroup,
        [FromQuery] SubscriptionType? subscriptionType,
        CancellationToken ct)
    {
        var videos = await _videoService.GetVideosAsync(ageGroup, subscriptionType, ct);
        return Ok(videos);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip videonun detay bilgilerini getirir
    /// </summary>
    /// <param name="id">Video ID (GUID)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Video detay bilgileri</returns>
    /// <response code="200">Video bulundu</response>
    /// <response code="404">Video bulunamadi</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VideoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VideoDto>> GetById(Guid id, CancellationToken ct)
    {
        var video = await _videoService.GetByIdAsync(id, ct);
        return video is null ? NotFound() : Ok(video);
    }

    /// <summary>
    /// Yeni egitim videosu ekler (Editor veya Admin yetkisi gerektirir)
    /// </summary>
    /// <param name="dto">Video bilgileri (baslik, URL, sure, etiketler, filtreler)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Olusturulan video bilgileri</returns>
    /// <response code="201">Video basariyla olusturuldu</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="403">Yetersiz yetki (Editor veya Admin gerekli)</response>
    [HttpPost]
    [Authorize(Roles = "Editor,Admin")]
    [ProducesResponseType(typeof(VideoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VideoDto>> Create([FromBody] CreateVideoDto dto, CancellationToken ct)
    {
        var video = await _videoService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = video.Id }, video);
    }

    /// <summary>
    /// Mevcut bir videoyu gunceller (Editor veya Admin yetkisi gerektirir)
    /// </summary>
    /// <param name="id">Guncellenecek video ID'si</param>
    /// <param name="dto">Yeni video bilgileri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Guncellenmis video bilgileri</returns>
    /// <response code="200">Video basariyla guncellendi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="404">Video bulunamadi</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Editor,Admin")]
    [ProducesResponseType(typeof(VideoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VideoDto>> Update(Guid id, [FromBody] CreateVideoDto dto, CancellationToken ct)
    {
        var video = await _videoService.UpdateAsync(id, dto, ct);
        return Ok(video);
    }

    /// <summary>
    /// Videoyu kalici olarak siler (Sadece Admin yetkisi)
    /// </summary>
    /// <param name="id">Silinecek video ID'si</param>
    /// <param name="ct">Cancellation token</param>
    /// <response code="204">Video basariyla silindi</response>
    /// <response code="401">Yetkisiz erisim</response>
    /// <response code="403">Yetersiz yetki (Admin gerekli)</response>
    /// <response code="404">Video bulunamadi</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _videoService.DeleteAsync(id, ct);
        return NoContent();
    }
}
