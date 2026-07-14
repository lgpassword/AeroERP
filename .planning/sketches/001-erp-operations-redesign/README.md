---
sketch: 001
name: erp-operations-redesign
question: "主数据、库存管理、采购管理、平台治理四个页面应该如何重组为更符合 ERP 的分析型工作台？"
winner: null
tags: [layout, dashboard, modal, charts, erp]
---

# Sketch 001: ERP Operations Redesign

## Design Question
围绕用户提出的四项改造要求，比较三种不同密度与分析比重的 ERP 界面结构，并验证哪些操作应保留在当前页，哪些应收敛到弹窗。

## How to View
open .planning/sketches/001-erp-operations-redesign/index.html

## Variants
- **A: 分析工作台** — 主图表区 + 明细列表区，更接近 SAP Fiori 分析页
- **B: 运营总控台** — 顶部经营指标 + 中部业务模块，更接近金蝶/用友后台
- **C: 流程驱动台** — 以待办和动作入口为主，更接近 Odoo 的操作流

## What to Look For
- 主数据页拆成三个模块后，哪种布局更适合持续维护供应商、物料、仓库
- 库存页的图表、点击入库记录、右侧详情跳转逻辑是否顺手
- 采购页把“新增/查看采购申请”收敛到右上角弹窗是否比当前双栏更合理
- 平台治理页是否应该将组织、审查、账号、角色模块权限统一改成按钮打开弹窗
