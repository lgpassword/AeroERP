# 界面截图

截图用于 GitHub 介绍、评审和协作沟通。截图文件保存在 `docs/images/`。

## 当前截图

| 文件 | 内容 |
| --- | --- |
| `docs/images/login.png` | 登录页，展示默认入口和产品标识。 |
| `docs/images/workspace.png` | 登录后的工作台，展示模块分组、权限数量和闭环业务入口。 |
| `docs/images/platform.png` | 平台治理页，展示账号、角色、模块可见性和审计相关入口。 |
| `docs/images/people-management.png` | 人员管理页，展示员工账号、入职建档、角色按钮选择和组织岗位状态。 |
| `docs/images/plugin-center.png` | 插件中心页，展示插件分组、模块显隐和入口状态。 |
| `docs/images/master-data.png` | 主数据页，展示客户、供应商、物料和仓库维护入口。 |
| `docs/images/inventory.png` | 库存执行页，展示入库、出库、调拨、盘点、库存余额和明细账入口。 |
| `docs/images/finance.png` | 财务工作台，展示科目、期间、凭证、应收应付、结算、对账和报表入口。 |
| `docs/images/manufacturing.png` | 制造管理页，展示 BOM、工单、领料、完工入库和工单成本入口。 |
| `docs/images/integration.png` | 通知与集成页，展示消息通道、Webhook、连接器、同步任务和审计入口。 |

## 重新生成截图

1. 启动后端：

   ```powershell
   .\scripts\start-apphost-single.ps1 -StopExisting
   ```

2. 启动前端：

   ```powershell
   npm run dev --workspace @aeroerp/web -- --host 0.0.0.0 --port 5173
   ```

3. 登录：

   ```text
   账号：admin
   密码：Admin@123456
   ```

4. 访问对应页面后截图并覆盖 `docs/images/` 下同名文件。

也可以使用仓库内置脚本自动登录并重新生成主要界面截图：

```powershell
npm run screenshots
```

脚本默认读取：

- Web：`http://localhost:5173`
- API：`http://localhost:5099`
- 账号：`admin`
- 密码：`Admin@123456`

可通过环境变量覆盖：

```powershell
$env:AEROERP_WEB_URL="http://localhost:5173"
$env:AEROERP_API_URL="http://localhost:5099"
$env:AEROERP_SCREENSHOT_USER="admin"
$env:AEROERP_SCREENSHOT_PASSWORD="Admin@123456"
npm run screenshots
```

截图只保存能帮助理解系统的界面，不保存本地日志、数据库、构建产物或含敏感信息的内容。
