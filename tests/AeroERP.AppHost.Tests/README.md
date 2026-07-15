# AeroERP.AppHost.Tests 测试说明

## 模块作用

该测试项目验证 AppHost 启动、核心端点、认证链路和基础业务 API 的可用性，是后端集成层的冒烟测试入口。

## 目录内容

- `AppHostFactory.cs`：测试用 WebApplicationFactory，创建隔离的 AppHost 测试环境。
- `AppHostSmokeTests.cs`：围绕启动、登录、模块和业务端点的冒烟测试。

## 整体链路

测试工厂启动 AppHost，测试用例通过 HTTP 客户端调用真实端点，验证平台初始化、认证和业务服务能协同工作。

## 审查重点

- 测试应保持端到端路径，避免只验证内部实现。
- 新增关键业务闭环时，应补充对应冒烟测试或验证工具。
