using DogalgazFarkindalik.Application.DTOs.Report;

namespace DogalgazFarkindalik.Application.Interfaces;

public interface IReportService
{
    Task<SummaryReportDto> GetSummaryAsync(CancellationToken ct = default);
}
