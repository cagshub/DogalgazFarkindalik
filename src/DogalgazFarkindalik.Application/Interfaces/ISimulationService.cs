using DogalgazFarkindalik.Application.DTOs.Simulation;
using DogalgazFarkindalik.Domain.Enums;

namespace DogalgazFarkindalik.Application.Interfaces;

public interface ISimulationService
{
    Task<List<SimulationListDto>> GetSimulationsAsync(AgeGroup? ageGroup, SubscriptionType? subType, CancellationToken ct = default);
    Task<SimulationDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SimulationResultDto> SubmitAnswersAsync(Guid simulationId, Guid userId, List<SubmitSimulationAnswerDto> answers, CancellationToken ct = default);
    Task<SimulationDetailDto> CreateAsync(CreateSimulationDto dto, CancellationToken ct = default);
    Task<SimulationDetailDto> UpdateAsync(Guid id, CreateSimulationDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
