# AeroERP.Platform.Infrastructure 模块说明

## 模块作用

`AeroERP.Platform.Infrastructure` 是平台和业务模块的运行时实现层，负责 EF Core 数据访问、服务实现、审计写入、认证、编号、模块可见性和插件表初始化。

## 目录内容

- `Persistence/`：`AeroErpDbContext`、表结构启动器、插件 schema 初始化器和种子数据。
- `Services/`：平台服务和所有业务模块服务的实现。
- `ServiceCollectionExtensions.cs`：依赖注入注册入口。

## 整体链路

端点调用模块接口，接口由本项目中的服务类实现。服务通过 `AeroErpDbContext` 读取和写入领域对象，并在关键业务动作后调用审计写入器。启动时 schema 初始化器创建核心表和插件表。

## 审查重点

- 服务实现应保持单一业务边界，不要跨模块直接复制规则。
- 数据变更要有权限校验、状态校验和必要审计。
- `SchemaBootstrapper` 和各插件初始化器要兼容 SQLite 本地开发环境。
