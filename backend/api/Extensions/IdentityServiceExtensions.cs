using System.Text;
using api.Models;
using api.Settings;
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

namespace api.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
    {
        #region Authentication & Authorization
        string tokenValue = configuration["TokenKey"]!;

        if (!string.IsNullOrEmpty(tokenValue))
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenValue)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true
                    };
                });
        }
        #endregion Authentication & Authorization

        #region MongoIdentity & Role
        var mongoDbSettings = configuration.GetSection(nameof(MyMongoDbSettings)).Get<MyMongoDbSettings>();

        if (mongoDbSettings is not null)
        {
            var mongodbIdentityConfig = new MongoDbIdentityConfiguration
            {
                MongoDbSettings = new MongoDbSettings
                {
                    ConnectionString = mongoDbSettings.ConnectionString,
                    DatabaseName = mongoDbSettings.DatabaseName
                },
                IdentityOptionsAction = options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireLowercase = false;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                    options.Lockout.MaxFailedAccessAttempts = 5;

                    options.User.RequireUniqueEmail = true;
                }
            };

            services.AddDataProtection();

            services.ConfigureMongoDbIdentity<AppUser, AppRole, ObjectId>(mongodbIdentityConfig)
            .AddUserManager<UserManager<AppUser>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddDefaultTokenProviders();
        }
        #endregion MongoIdentity & Role
        
          #region Policy
        services.AddAuthorizationBuilder()
            .AddPolicy("RequiredAdminRole", policy => policy.RequireRole("admin"))
            .AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("admin", "moderator"));
        #endregion

        return services;
    }

}