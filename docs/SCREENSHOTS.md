# 界面截图

截图用于 GitHub 介绍、评审和协作沟通。截图文件保存在 `docs/images/`。

## 当前截图

| 文件 | 内容 |
| --- | --- |
| `docs/images/login.png` | 登录页，展示默认入口和产品标识。 |
| `docs/images/platform.png` | 平台治理页，展示账号、角色、模块可见性和审计相关入口。 |
| `docs/images/inventory.png` | 库存执行页，展示入库、出库、调拨、盘点、库存余额和明细账入口。 |
| `docs/images/finance.png` | 财务工作台，展示科目、期间、凭证、应收应付、结算、对账和报表入口。 |

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

截图只保存能帮助理解系统的界面，不保存本地日志、数据库、构建产物或含敏感信息的内容。
