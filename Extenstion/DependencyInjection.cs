using Amazon.S3;
using Backend.Interfaces;
using Backend.Mapping;
using Backend.Models.Identity;
using Backend.Persistence;
using Backend.Services.Implementations;
using Backend.Utiliy;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Runtime;
using System.Text;
using System.Text.Json;


namespace Backend.Extenstion
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWebApiServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ============================
            // DATABASE (ENV VARIABLES)
            // ============================
            string GetEnv(string name) =>
                Environment.GetEnvironmentVariable(name)
                ?? throw new InvalidOperationException($"{name} is not set in environment variables.");

            var dbHost = GetEnv("DB_HOST");
            var dbPort = GetEnv("DB_PORT");
            var dbName = GetEnv("DB_NAME");
            var dbUser = GetEnv("DB_USER");
            var dbPassword = GetEnv("DB_PASSWORD");

            var connectionString =
                $"Host={dbHost};" +
                $"Port={dbPort};" +
                $"Database={dbName};" +
                $"Username={dbUser};" +
                $"Password={dbPassword};" +
                $"Ssl Mode=Prefer;Trust Server Certificate=true;";

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // ============================
            // EMAIL SERVICE CONFIGURATION
            // ============================
            services.AddScoped<IEmailService>(sp =>
            {
                // 1. Pull the new 'From Address' from environment variables
                var host = Environment.GetEnvironmentVariable("EMAIL_HOST");
                var portStr = Environment.GetEnvironmentVariable("EMAIL_PORT");
                var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
                var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                var fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME");
                var fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS"); // <--- ADD THIS

                // 2. Parse safety defaults
                int port = int.TryParse(portStr, out var p) ? p : 587;

                // 3. Return the updated Service with the new 'fromEmail' parameter
                return new EmailService(
                    host ?? throw new Exception("EMAIL_HOST missing"),
                    port,
                    username ?? throw new Exception("EMAIL_USERNAME missing"),
                    password ?? throw new Exception("EMAIL_PASSWORD missing"),
                    fromName ?? "Kahiye App",
                    fromEmail ?? "info@adnankahiye.com" // <--- PASS THIS
                );
            });
            // ============================

            // ============================
            // IDENTITY
            // ============================
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // ============================
            // JWT SETTINGS (ENV)
            // ============================
            var jwtKey = GetEnv("JWT_KEY");
            var jwtIssuer = GetEnv("JWT_ISSUER");
            var jwtAudience = GetEnv("JWT_AUDIENCE");
            var jwtDuration =
                int.TryParse(Environment.GetEnvironmentVariable("JWT_DURATION"), out var d)
                    ? d
                    : 60;

            services.Configure<JwtSettings>(options =>
            {
                options.Key = jwtKey;
                options.Issuer = jwtIssuer;
                options.Audience = jwtAudience;
                options.DurationInMinutes = jwtDuration;
            });

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));




            // ============================
            // R2 CONFIGURATION (ENV)
            // ============================

            var r2ServiceUrl = GetEnv("R2__ServiceUrl");
            var r2Bucket = GetEnv("R2__BucketName");
            var r2AccessKey = GetEnv("R2__AccessKey");
            var r2SecretKey = GetEnv("R2__SecretKey");

            services.AddSingleton<IAmazonS3>(sp =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = r2ServiceUrl,
                    ForcePathStyle = true,
                    UseHttp = false,
                    AuthenticationRegion = "auto", 

                };

                return new AmazonS3Client(
                    r2AccessKey,
                    r2SecretKey,
                    config
                );
            });
            services.AddSingleton(new R2Settings
            {
                BucketName = r2Bucket,
                PublicUrl = "https://pub-771cef1ae3e640bd8f325bba8bf1a880.r2.dev"
            });

            // ============================
            // HANGFIRE CONFIGURATION (ENV)
            // ============================

            var hfHost = GetEnv("DB_HOST");
            var hfPort = GetEnv("DB_PORT");
            var hfName = GetEnv("DB_NAME");
            var hfUser = GetEnv("DB_USER");
            var hfPassword = GetEnv("DB_PASSWORD");

            var hangfireConnection =
                $"Host={hfHost};" +
                $"Port={hfPort};" +
                $"Database={hfName};" +
                $"Username={hfUser};" +
                $"Password={hfPassword};";

            // Add Hangfire
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(hangfireConnection)
            );

            // Add Hangfire Server
            services.AddHangfireServer();









            // ============================
            // AUTHENTICATION + JWT
            // ============================
            // ============================
            // AUTHENTICATION + JWT
            // ============================
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        // 🔍 TOKEN RECEIVED
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                context.Token = authHeader.Substring("Bearer ".Length).Trim();
                return Task.CompletedTask;
            }

            var token = context.Request.Cookies["token"];

            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
                Console.WriteLine("✅ TOKEN FROM COOKIE");
            }

            return Task.CompletedTask;
        },

        // 🔴 AUTH FAILED (MOST IMPORTANT)
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("❌ AUTH FAILED:");
            Console.WriteLine(context.Exception.GetType().Name);
            Console.WriteLine(context.Exception.Message);

            if (context.Exception.InnerException != null)
            {
                Console.WriteLine("INNER: " + context.Exception.InnerException.Message);
            }

            return Task.CompletedTask;
        },

        // 🟡 TOKEN VALIDATED
        OnTokenValidated = context =>
        {

            var claims = context.Principal.Claims
                .Select(c => $"{c.Type}: {c.Value}");

            Console.WriteLine("🔹 CLAIMS:");
            foreach (var claim in claims)
            {
                Console.WriteLine(claim);
            }

            return Task.CompletedTask;
        },

        // 🔴 CHALLENGE (401)
        OnChallenge = async context =>
        {

            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Authentication required."
                })
            );
        },

        // 🔴 FORBIDDEN (403)
        OnForbidden = async context =>
        {

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Access denied."
                })
            );
        }
    };
});
            // ============================
            // SERVICES
            // ============================
            services.AddScoped<IJwtService, JwtService>();



            //polcy

            services.AddAuthorization();

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            // services.AddAutoMapper(typeof(MappingProfiles));
            services.AddService(configuration);
            // Add memory cache
            services.AddMemoryCache();
            services.AddScoped<R2Service>();
            services
             .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters
                    .Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            // ============================
            // CORS
            // ============================
            services.AddCors(options =>
            {
                options.AddPolicy("KahiyeApp", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost",
                            "http://localhost:3000",
                            "http://localhost:5173",
                            "http://127.0.0.1",
                            "https://www.adnankahiye.com",
                            "https://adnankahiye.com"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // ============================
            // CONTROLLERS & SWAGGER
            // ============================
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        // ============================
        // IDENTITY SEEDER
        // ============================
        public static async Task UseIdentitySeederAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                var dbContext = services.GetRequiredService<AppDbContext>();

                logger.LogInformation("🔹 Starting Identity seeding...");
                await IdentitySeed.SeedRolesAndUsersAsync(userManager, roleManager, dbContext);
                logger.LogInformation(" Identity seeding completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Identity seeding failed.");
            }
        }
    }
}