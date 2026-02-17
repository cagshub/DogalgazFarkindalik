using System.Security.Claims;
using DogalgazFarkindalik.Application.DTOs.Video;
using DogalgazFarkindalik.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.API.Controllers;

/// <summary>
/// Video izlenme ilerleme takibi
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
    /// Kullanicinin tum video izlenme ilerlemelerini getirir
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<VideoProgressDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VideoProgressDto>>> GetMyProgress(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var progress = await _service.GetUserProgressAsync(userId, ct);
        return Ok(progress);
    }

    /// <summary>
    /// Video izlenme ilerlemesini gunceller
    /// </summary>
    [HttpPut("{videoId:guid}")]
    [ProducesResponseType(typeof(VideoProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VideoProgressDto>> UpdateProgress(
        Guid videoId, [FromBody] UpdateVideoProgressDto dto, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var progress = await _service.UpdateProgressAsync(userId, videoId, dto, ct);
        return Ok(progress);
    }
}
