using System.Security.Claims;
using DogalgazFarkindalik.Domain.Entities;
using DogalgazFarkindalik.Infrastructure.Data;

namespace DogalgazFarkindalik.API.Middleware;

public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLogMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Request.Method is "POST" or "PUT" or "DELETE"
            && context.Response.StatusCode is >= 200 and < 300)
        {
            try
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? userId = Guid.TryParse(userIdClaim, out var uid) ? uid : null;

                db.AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    Action = context.Request.Method,
                    Entity = context.Request.Path.Value ?? "",
                    EntityId = null,
                    Meta = $"StatusCode: {context.Response.StatusCode}"
                });

                await db.SaveChangesAsync();
            }
            catch
            {
                // Audit log hatası uygulamayı durdurmamalı
            }
        }
    }
}
