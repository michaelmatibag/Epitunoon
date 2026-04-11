using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pawtrak.Application.Services;
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
        // Dapper: map snake_case DB columns → PascalCase C# properties automatically
        DefaultTypeMap.MatchNamesWithUnderscores = true;

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

        // External infrastructure services
        services.AddSingleton<IPaymentService, StripePaymentService>();
        services.AddSingleton<IFileStorageService, AzureBlobStorageService>();
        services.AddScoped<INotificationService, EmailNotificationService>();
        services.AddScoped<IGpsTrackingService, GpsTrackingService>();

        // Application services (use cases) — concrete implementations live here
        services.AddScoped<IOwnerService, OwnerService>();
        services.AddScoped<IWalkerService, WalkerService>();
        services.AddScoped<IDogService, DogService>();
        services.AddScoped<IRoutineTaskService, RoutineTaskService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}
