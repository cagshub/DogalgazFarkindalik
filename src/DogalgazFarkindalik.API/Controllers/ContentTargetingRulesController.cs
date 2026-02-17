using DogalgazFarkindalik.Application.DTOs.ContentTargeting;
using DogalgazFarkindalik.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogalgazFarkindalik.API.Controllers;

/// <summary>
/// Icerik hedefleme kurallari yonetimi (Admin)
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
    /// Tum hedefleme kurallarini listeler
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ContentTargetingRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ContentTargetingRuleDto>>> GetAll(CancellationToken ct)
    {
        var rules = await _service.GetAllAsync(ct);
        return Ok(rules);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip hedefleme kuralini getirir
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContentTargetingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentTargetingRuleDto>> GetById(Guid id, CancellationToken ct)
    {
        var rule = await _service.GetByIdAsync(id, ct);
        return rule is null ? NotFound() : Ok(rule);
    }

    /// <summary>
    /// Yeni hedefleme kurali olusturur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContentTargetingRuleDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ContentTargetingRuleDto>> Create(
        [FromBody] CreateContentTargetingRuleDto dto, CancellationToken ct)
    {
        var rule = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Mevcut hedefleme kuralini gunceller
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContentTargetingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentTargetingRuleDto>> Update(
        Guid id, [FromBody] CreateContentTargetingRuleDto dto, CancellationToken ct)
    {
        var rule = await _service.UpdateAsync(id, dto, ct);
        return Ok(rule);
    }

    /// <summary>
    /// Hedefleme kuralini siler
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
