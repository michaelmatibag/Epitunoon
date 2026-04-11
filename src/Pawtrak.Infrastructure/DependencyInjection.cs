using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pawtrak.Core.Interfaces;
using Pawtrak.Core.Interfaces.Repositories;
using Pawtrak.Core.Interfaces.Services;
using Pawtrak.Infrastructure.Data;
using Pawtrak.Infrastructure.Data.Repositories;
using Pawtrak.Infrastructure.Services;

namespace Pawtrak.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Dapper — match C# PascalCase properties to column names automatically
        DefaultTypeMap.MatchNamesWithUnderscores = false;

        // Database connection
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

        // Repositories
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<IWalkerRepository, WalkerRepository>();
        services.AddScoped<IDogRepository, DogRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IRoutineTaskRepository, RoutineTaskRepository>();
        services.AddScoped<IVisitReportRepository, VisitReportRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // External services
        services.AddSingleton<IPaymentService, StripePaymentService>();
        services.AddSingleton<IFileStorageService, AzureBlobStorageService>();
        services.AddScoped<INotificationService, EmailNotificationService>();

        return services;
    }
}
