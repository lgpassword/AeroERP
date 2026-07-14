# AeroERP 共创规范 / Contribution Guide

欢迎共创 AeroERP。项目接受功能、文档、测试、界面体验、模块治理和业务闭环方面的改进，但所有变更必须通过分支和 Pull Request 审查，不能随意直接上传到主分支。

## 中文规范

### 分支规则

- 禁止直接向 `main` 或 `master` 推送业务代码。
- 每个改动使用独立分支，例如：
  - `feature/procurement-approval-note`
  - `fix/inventory-ledger-filter`
  - `docs/github-readiness`
- 分支只包含一个明确目标，避免把无关整理混在一起。

### Pull Request 要求

- PR 标题要说明模块和目的。
- PR 描述必须包含：
  - 改动内容
  - 影响模块
  - 验证命令
  - 是否涉及权限、审计、数据结构或 UI 流程
- 涉及页面的改动建议附截图。
- 涉及数据库、权限、审计或智能体动作的改动必须说明兼容影响和回滚方式。

### 禁止提交的内容

- `node_modules/`
- `logs/`
- `.artifacts/`
- `.vs/`
- `bin/`
- `obj/`
- `dist/`
- 本地数据库、私钥、Token、临时导出文件

这些内容已由 `.gitignore` 忽略。如曾被 Git 跟踪，需要先从索引移除。

### 代码要求

- 保持模块边界清晰，不要跨模块直接耦合。
- 不添加无效按钮、假流程或只展示不落库的业务动作。
- 新业务模块必须同时提供后端能力和可操作 UI。
- 权限、模块可见性、审计和智能体审查逻辑不能绕过。
- 注释要解释业务意图和关键约束，不写空泛说明。
- 修改模块行为时，同步更新对应 `README.md` 或 `docs/` 文档。

### 本地验证

推荐在提交前执行：

```powershell
dotnet build AeroERP.slnx --no-restore --disable-build-servers
npm run build
npm run lint --workspace @aeroerp/web
```

完整验证：

```powershell
.\scripts\verify.ps1
```

## English Guide

### Branch Policy

- Do not push business changes directly to `main` or `master`.
- Use a dedicated branch for each change.
- Keep each branch focused on one clear objective.

### Pull Request Expectations

Each PR should describe:

- What changed.
- Which modules are affected.
- Which verification commands were run.
- Whether the change touches permissions, audit events, schema, or UI workflows.
- Screenshots for visible UI changes.

### Do Not Commit

Do not commit dependencies, logs, build outputs, local databases, IDE caches, secrets, or exported runtime files.

### Engineering Expectations

- Preserve module boundaries.
- Do not ship dead UI or fake business data.
- Keep permissions, visibility, and audit behavior intact.
- Update module documentation when behavior changes.
- Make AI/agent behavior reviewable, auditable, and permission-controlled.

### Verification

Run at least the relevant backend and front-end checks before opening a PR:

```powershell
dotnet build AeroERP.slnx --no-restore --disable-build-servers
npm run build
npm run lint --workspace @aeroerp/web
```
