# AeroERP

中文 | [Release Branch](../../tree/release/github-readiness)

AeroERP 是一个模块化 ERP 项目。本仓库主分支只作为受保护入口，不直接承载未经审查的业务代码。

完整项目源码、中文/英文介绍、构建方式、运行方式、使用流程和界面截图位于：

```text
release/github-readiness
```

## 主分支约束

- 禁止直接向 `main` 推送业务代码。
- 所有改动必须通过功能分支和 Pull Request。
- 涉及权限、审计、数据库结构、智能体行为和关键业务流程的改动必须重点审查。
- 不提交 `node_modules/`、`logs/`、`.artifacts/`、`data/`、`bin/`、`obj/`、`dist/`、本地数据库或密钥。

## 共创入口

请阅读：

- [CONTRIBUTING.md](CONTRIBUTING.md)
- [docs/GITHUB-GOVERNANCE.md](docs/GITHUB-GOVERNANCE.md)

---

English

AeroERP is a modular ERP project. The `main` branch is a protected entry branch only. Full source code, bilingual introduction, build/run instructions, user guide, and screenshots are available on:

```text
release/github-readiness
```

Do not push business changes directly to `main`. Use feature branches and Pull Requests.
