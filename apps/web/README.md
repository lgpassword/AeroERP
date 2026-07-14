# AeroERP Web 前端说明

`apps/web` 是 AeroERP 主 Web 工作台，负责登录态、模块导航、业务页面、接口调用、权限判断和语言切换。

## 常用命令

```bash
npm run dev --workspace @aeroerp/web
npm run build --workspace @aeroerp/web
npm run lint --workspace @aeroerp/web
```

## 目录内容

- `src/App.tsx`：登录态路由守卫和模块路由注册。
- `src/components/Shell.tsx`：顶部导航、语言切换、账号操作和页面切换动效。
- `src/modules/moduleNavigation.ts`：模块路由路径和默认路由顺序的唯一来源。
- `src/pages/`：各业务模块页面。
- `src/api/client.ts`：类型化 HTTP 客户端。
- `src/hooks/useAsyncData.ts`：共享异步加载 hook，支持同请求复用和最新请求回写保护。
- `src/auth/`：当前用户、登录、退出、权限判断和令牌状态。
- `src/i18n/`：轻量语言上下文和显示文本查找。

## 整体链路

用户进入前端后，`AuthContext` 恢复登录态并读取当前用户。`App.tsx` 根据用户可见模块注册可访问路由。`Shell` 根据模块权限渲染导航。业务页面通过 `api/client.ts` 调用后端接口，并用 `useAsyncData` 管理加载状态。

## 页面规则

- 页面只能展示真实持久化数据，或展示明确空状态。
- 可点击命令必须对应导航、数据变更、校验、弹层状态或后端持久化结果。
- 模块路径必须维护在 `src/modules/moduleNavigation.ts`，不要在页面里重复写路径判断。
- 共享视觉能力应放在 `packages/ui-kit` 和 `packages/ui-style`。
