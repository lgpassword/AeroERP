# AeroERP.AppHost 模块说明

## 模块作用

`AeroERP.AppHost` 是后端启动入口和组合根，只负责把平台能力、基础设施、业务模块、插件目录、认证授权和 HTTP API 组合成一个可运行的 ASP.NET Core 应用。

## 目录内容

- `Program.cs`：应用启动、服务注册、中间件、数据库初始化和端点挂载入口。
- `PlatformEndpoints.cs`：登录、用户、角色、模块可见性、组织、审计和智能体审查等平台 API。
- `ModuleCatalog.cs`：内置业务模块目录，决定系统有哪些模块可注册。
- `PluginCatalog.cs`：插件描述目录，供平台启动时发现和初始化插件能力。
- `appsettings*.json`：运行配置、连接字符串和 JWT 设置。

## 整体链路

请求进入 AppHost 后，先经过认证授权中间件，再进入平台端点或各业务模块端点。端点调用平台服务或模块服务，服务通过 `AeroERP.Platform.Infrastructure` 访问数据库并写入审计。

## 审查重点

- AppHost 不应承载业务规则，业务逻辑应放到模块服务中。
- 新增模块时应同步更新模块目录、插件目录、依赖注入和前端导航。
- 认证、审计、模块可见性和数据库初始化必须在启动流程中保持稳定顺序。
