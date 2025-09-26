using CondoManager.Api.Config;
using CondoManager.Api.Infrastructure;
using CondoManager.Api.Interfaces;
using CondoManager.Api.Services;
using CondoManager.Repository;
using CondoManager.Repository.Interfaces;
using CondoManager.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CondoManager.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind config
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
            services.Configure<DatabaseOptions>(configuration.GetSection("Database"));

            // DbContext
            var dbOptions = configuration.GetSection("Database").Get<DatabaseOptions>();
            services.AddDbContext<CondoContext>(opts => opts.UseSqlite(dbOptions?.ConnectionString ?? "Data Source=condomanager.db"));
            
            // Register CondoContext as DbContext for UnitOfWork
            services.AddScoped<DbContext>(provider => provider.GetService<CondoContext>()!);

            // RabbitMQ publisher (singleton)
            services.AddSingleton<RabbitMqPublisher>();

            // Repository layer
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Register services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<IAuthenticationEventService, AuthenticationEventService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IApartmentService, ApartmentService>();
            
            // Message event handling
            services.AddScoped<IMessageEventHandler, MessageEventHandler>();
            services.AddHostedService<RabbitMqConsumerService>();
            
            // Register RabbitMQ Publisher (remove duplicate)
            // services.AddSingleton<RabbitMqPublisher>(); // Already registered above

            return services;
        }
    }
}
