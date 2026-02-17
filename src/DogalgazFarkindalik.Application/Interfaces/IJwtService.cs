using DogalgazFarkindalik.Domain.Entities;

namespace DogalgazFarkindalik.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateRefreshToken(string token);
}
