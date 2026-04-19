using AutoMapper;
using Backend.Interfaces;
using Backend.Interfaces.Accounts;
using Backend.Interfaces.Customers;
using Backend.Interfaces.Emails;
using Backend.Interfaces.Identity;
using Backend.Interfaces.Setup;
using Backend.Interfaces.Subscription;
using Backend.Mapping;
using Backend.Persistence;
using Backend.Services.Accounts;
using Backend.Services.Customers;
using Backend.Services.Emails;
using Backend.Services.Implementations;
using Backend.Services.Subscriptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Extenstion
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddService(this IServiceCollection services, IConfiguration configuration)
        {
            // Register AutoMapper to scan the Backend assembly containing your profiles
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            //services.AddAutoMapper(typeof(MappingProfiles));

            // Register your DbContext and other services
            services.AddDbContext<AppDbContext>();
            // Register services
            services.AddScoped<IAuthService, AuthService>();  // Register IAuthService and its implementation
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IOtpService, OtpService>();

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ISetupService, SetupService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<ICustomersService, CustomersService>();

            services.AddScoped<IJwtService, JwtService>();

            return services;
        }
    }
}