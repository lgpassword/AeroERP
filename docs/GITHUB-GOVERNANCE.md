# GitHub 仓库治理说明

本文说明 AeroERP 上传到 GitHub 后的分支、审查和主分支保护策略。

## 1. 基本原则

- 不直接向 `main` 或 `master` 上传业务代码。
- 所有改动通过功能分支提交。
- 合并必须通过 Pull Request。
- PR 合并前必须完成构建、测试或明确说明无法执行的原因。
- 涉及权限、审计、数据库结构、智能体行为和关键业务流程的改动必须重点审查。

## 2. 推荐分支

| 分支 | 用途 |
| --- | --- |
| `main` | 稳定主分支，只接收审查后的 PR。 |
| `release/github-readiness` | 本次 GitHub 发布准备分支。 |
| `feature/*` | 新功能分支。 |
| `fix/*` | 缺陷修复分支。 |
| `docs/*` | 文档变更分支。 |

## 3. GitHub 分支保护建议

仓库创建后，建议在 GitHub 页面设置：

1. 打开仓库 Settings。
2. 进入 Branches。
3. 添加 branch protection rule。
4. Branch name pattern 填写 `main`。
5. 勾选：
   - Require a pull request before merging
   - Require approvals
   - Require status checks to pass before merging
   - Require conversation resolution before merging
   - Do not allow bypassing the above settings
6. 保存规则。

如果仓库使用 `master` 作为主分支，也应给 `master` 添加相同规则。

## 4. Pull Request 模板

项目已提供 `.github/pull_request_template.md`。提交 PR 时请填写：

- 改动摘要
- 影响模块
- 验证命令
- 截图
- 权限、审计、数据结构影响
- 风险和回滚说明

## 5. 不应上传的文件

以下文件只属于本地运行或构建过程，不进入 Git：

- `node_modules/`
- `.artifacts/`
- `logs/`
- `.vs/`
- `bin/`
- `obj/`
- `dist/`
- `data/`
- 本地数据库、Token、私钥、临时导出文件

`.gitignore` 已配置这些规则。
