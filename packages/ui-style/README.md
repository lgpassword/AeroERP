# @aeroerp/ui-style 包说明

## 模块作用

`@aeroerp/ui-style` 提供 AeroERP 前端共享设计令牌和基础样式，是 UI 颜色、间距、圆角、阴影和动效节奏的统一来源。

## 目录内容

- `src/index.ts`：导出 `tokens`、`motion` 和 `shellStyles`。
- `package.json`：包名、构建入口和依赖声明。

## 整体链路

`@aeroerp/ui-kit` 读取 `shellStyles` 并通过 `StyleRegistry` 注入页面。业务页面通过共享组件使用这些样式，避免每个业务模块单独定义视觉规则。

## 审查重点

- 颜色、间距、圆角和动效应集中维护。
- 业务页面不应绕过样式包直接创造新的视觉系统。
- 修改令牌前要检查所有共享组件和业务页面的显示效果。
