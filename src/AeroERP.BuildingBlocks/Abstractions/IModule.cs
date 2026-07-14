using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AeroERP.BuildingBlocks.Abstractions;

/// <summary>
/// Module 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IModule
{
    string Key { get; }
    string DisplayName { get; }
    void AddServices(IServiceCollection services);
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
