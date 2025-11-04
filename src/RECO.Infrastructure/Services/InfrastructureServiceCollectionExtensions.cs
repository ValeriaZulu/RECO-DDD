using Microsoft.Extensions.DependencyInjection;
using RECO.Domain.Interfaces;
using RECO.Infrastructure.Repositories;

namespace RECO.Infrastructure.Services
{
    /// <summary>
    /// Extension method to register Infrastructure services (repositories, adapters).
    /// Call from host startup to wire concrete implementations. Keeps wiring in Infrastructure per constitution.
    /// </summary>
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Repository implementation in Infrastructure - implements Domain interfaces (DIP)
            services.AddScoped<IUserRepository, EFUserRepository>();
            services.AddScoped<ITitleRepository, EFTitleRepository>();
            services.AddScoped<IReviewRepository, EFReviewRepository>();
            services.AddScoped<IGenreRepository, EFGenreRepository>();

            return services;
        }
    }
}
