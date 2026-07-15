# @aeroerp/ui-kit 包说明

## 模块作用

`@aeroerp/ui-kit` 提供 AeroERP 前端共享业务组件。业务页面通过这些组件保持统一的页面结构、区块、空状态和统计卡片。

## 目录内容

- `src/index.tsx`：导出 `StyleRegistry`、`PageShell`、`SectionBlock`、`EmptyState` 和 `StatTile`。
- `package.json`：包名、构建入口和依赖声明。

## 整体链路

Web 应用在入口挂载 `StyleRegistry` 注入共享样式。各业务页面使用 `PageShell` 承载页面标题和操作区，使用 `SectionBlock` 组织业务区块，使用 `EmptyState` 表达无数据或无权限状态，使用 `StatTile` 展示关键指标。

## 审查重点

- 组件应保持通用，不写入具体业务模块规则。
- 新增共享组件前应确认多个业务页面复用。
- 视觉样式应来自 `@aeroerp/ui-style`，不要在业务组件里重新定义视觉系统。
