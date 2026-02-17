using DogalgazFarkindalik.Application.DTOs.ContentTargeting;

namespace DogalgazFarkindalik.Application.Interfaces;

public interface IContentTargetingRuleService
{
    Task<List<ContentTargetingRuleDto>> GetAllAsync(CancellationToken ct = default);
    Task<ContentTargetingRuleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ContentTargetingRuleDto> CreateAsync(CreateContentTargetingRuleDto dto, CancellationToken ct = default);
    Task<ContentTargetingRuleDto> UpdateAsync(Guid id, CreateContentTargetingRuleDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
