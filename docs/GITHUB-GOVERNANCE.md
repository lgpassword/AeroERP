# GitHub 仓库治理说明

本文说明 AeroERP 上传到 GitHub 后的分支、审查和主分支保护策略。

## 1. 基本原则

- 不直接向 `main` 或 `master` 上传业务代码。
- 当前仓库所有者为 `lgpassword`。除 `lgpassword` 外，不给任何账号直接写入 `main` 的权限。
- 如果需要邀请共创者，默认只给只读或通过 Pull Request 协作，不直接授予 `main` 写权限。
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

## 4. 私有仓库套餐限制下的替代约束

如果 GitHub 返回 `Upgrade to GitHub Pro or make this repository public to enable this feature`，说明当前私有仓库无法启用官方 branch protection 或 ruleset。

在这种情况下，项目保留以下替代约束：

- 当前协作者列表只保留仓库所有者 `lgpassword`。
- `main` 只保存仓库入口、治理说明和 PR 模板，不直接承载完整业务代码。
- 完整源码先进入 `release/*` 或 `feature/*` 分支。
- 通过 Pull Request 审查完整代码后再决定是否合并。
- `.github/workflows/main-branch-policy.yml` 会检查 `main` 的直接 push。非 `lgpassword` 推送会失败；业务代码直接推送也会失败，用于留下审计信号。

注意：GitHub Actions 失败不能像官方分支保护一样预先阻止推送。要获得强制拦截能力，需要升级 GitHub Pro/Team，或者将仓库改为公开仓库后再启用 branch protection/ruleset。

## 5. Pull Request 模板

项目已提供 `.github/pull_request_template.md`。提交 PR 时请填写：

- 改动摘要
- 影响模块
- 验证命令
- 截图
- 权限、审计、数据结构影响
- 风险和回滚说明

## 6. 不应上传的文件

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
