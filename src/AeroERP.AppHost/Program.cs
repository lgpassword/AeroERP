using AeroERP.AppHost;
using AeroERP.Modules.AdvancedManufacturing.Endpoints;
using AeroERP.Modules.Control.Endpoints;
using AeroERP.Modules.DocumentExchange.Endpoints;
using AeroERP.Modules.Finance.Endpoints;
using AeroERP.Modules.Integration.Endpoints;
using AeroERP.Modules.Inventory.Endpoints;
using AeroERP.Modules.Localization.Endpoints;
using AeroERP.Modules.Manufacturing.Endpoints;
using AeroERP.Modules.MasterData.Endpoints;
using AeroERP.Modules.MobileWork.Endpoints;
using AeroERP.Modules.Planning.Endpoints;
using AeroERP.Modules.PositionPermissions.Endpoints;
using AeroERP.Modules.Procurement.Endpoints;
using AeroERP.Modules.Quality.Endpoints;
using AeroERP.Modules.Reporting.Endpoints;
using AeroERP.Modules.Sales.Endpoints;
using AeroERP.Modules.Workflow.Endpoints;
using AeroERP.Modules.Wms.Endpoints;
using AeroERP.BuildingBlocks.Abstractions;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Infrastructure;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAeroErpInfrastructure(builder.Configuration);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Auth:Jwt:Key"] ?? "AeroERP_Local_Dev_Key_Change_Me_Immediately_2026";
        var issuer = builder.Configuration["Auth:Jwt:Issuer"] ?? "AeroERP";
        var audience = builder.Configuration["Auth:Jwt:Audience"] ?? "AeroERP.Web";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var rawUserId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(rawUserId, out var userId))
                {
                    context.Fail("无效身份令牌。");
                    return;
                }

                var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                var currentUser = await authService.GetCurrentUserAsync(userId, context.HttpContext.RequestAborted);
                if (currentUser is null || !currentUser.IsEnabled)
                {
                    context.Fail("账号不可用。");
                    return;
                }

                if (context.Principal?.Identity is not ClaimsIdentity identity)
                {
                    context.Fail("无效身份上下文。");
                    return;
                }

                var removableClaims = identity.Claims
                    .Where(claim =>
                        claim.Type is ClaimTypes.Name
                        or ClaimTypes.Role
                        or PlatformClaimTypes.DisplayName
                        or PlatformClaimTypes.Module
                        or PlatformClaimTypes.Permission)
                    .ToList();

                foreach (var claim in removableClaims)
                {
                    identity.RemoveClaim(claim);
                }

                identity.AddClaim(new Claim(ClaimTypes.Name, currentUser.UserName));
                identity.AddClaim(new Claim(PlatformClaimTypes.DisplayName, currentUser.DisplayName));

                foreach (var role in currentUser.Roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                foreach (var permission in currentUser.Permissions)
                {
                    identity.AddClaim(new Claim(PlatformClaimTypes.Permission, permission));
                }

                foreach (var module in currentUser.VisibleModuleKeys)
                {
                    identity.AddClaim(new Claim(PlatformClaimTypes.Module, module));
                }
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in PlatformPermissions.All)
    {
        options.AddPolicy(permission, policy =>
            policy.RequireAuthenticatedUser()
                .RequireClaim(PlatformClaimTypes.Permission, permission));
    }
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "data"));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AeroErpDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<AppUser>>();
    await db.Database.EnsureCreatedAsync();
    foreach (var initializer in scope.ServiceProvider.GetServices<IPluginSchemaInitializer>())
    {
        await initializer.InitializeAsync(app.Lifetime.ApplicationStopping);
    }

    await Seeder.SeedPlatformAsync(db, ModuleCatalog.Modules, passwordHasher, app.Lifetime.ApplicationStopping);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    name = "AeroERP API",
    modules = ModuleCatalog.Modules.Select(x => x.Key),
    plugins = PluginCatalog.Plugins.Select(plugin => new
    {
        plugin.Key,
        plugin.DisplayName,
        modules = plugin.Modules.Select(module => module.Key)
    })
}));

app.MapGet("/health/live", () => Results.Ok(new
{
    status = "Healthy",
    service = "AeroERP API",
    checkedAtUtc = DateTimeOffset.UtcNow
}));

app.MapGet("/health/ready", async (AeroErpDbContext db, CancellationToken ct) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync(ct);
        if (!canConnect)
        {
            return Results.Json(new
            {
                status = "Unhealthy",
                database = "Unavailable",
                checkedAtUtc = DateTimeOffset.UtcNow
            }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Results.Ok(new
        {
            status = "Healthy",
            database = "Available",
            modules = ModuleCatalog.Modules.Length,
            plugins = PluginCatalog.Plugins.Length,
            checkedAtUtc = DateTimeOffset.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "Unhealthy",
            database = "Unavailable",
            error = ex.GetType().Name,
            checkedAtUtc = DateTimeOffset.UtcNow
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapPlatformEndpoints();
app.MapMasterDataEndpoints();
app.MapProcurementEndpoints();
app.MapSalesEndpoints();
app.MapInventoryEndpoints();
app.MapFinanceEndpoints();
app.MapWorkflowEndpoints();
app.MapControlEndpoints();
app.MapLocalizationEndpoints();
app.MapManufacturingEndpoints();
app.MapAdvancedManufacturingEndpoints();
app.MapQualityEndpoints();
app.MapReportingEndpoints();
app.MapPlanningEndpoints();
app.MapPositionPermissionEndpoints();
app.MapWmsEndpoints();
app.MapMobileWorkEndpoints();
app.MapIntegrationEndpoints();
app.MapDocumentExchangeEndpoints();

app.Run();

/// <summary>
/// 应用程序启动入口。
/// </summary>
public partial class Program;
