using System.Reflection;
using FishClubAlginet.Application.Behaviors;
using FishClubAlginet.Application.Services;
using FishClubAlginet.Core.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FishClubAlginet.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services:
    /// - MediatR handlers (scanned from this assembly)
    /// - ValidationPipelineBehavior (auto-validates commands before handlers)
    /// - FluentValidation validators (scanned from this assembly)
    /// - Domain services (IPointsCalculator → PointsCalculatorService)
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IPointsCalculator, PointsCalculatorService>();

        return services;
    }
}
