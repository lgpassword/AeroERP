import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { DocumentExchangeOverview } from "../types/api";

const emptyOverview: DocumentExchangeOverview = {
  importTemplates: [],
  fieldMappings: [],
  importBatches: [],
  exportTasks: [],
  printTemplates: [],
  printJobs: [],
  auditRecords: [],
  metrics: [],
};

const moduleOptions = [
  { key: "master-data", label: "主数据" },
  { key: "procurement", label: "采购管理" },
  { key: "sales", label: "销售管理" },
  { key: "inventory", label: "库存管理" },
  { key: "wms", label: "WMS 执行" },
  { key: "mobile-work", label: "移动作业" },
  { key: "integration", label: "通知与集成" },
  { key: "finance", label: "财务结算" },
  { key: "workflow", label: "审批中心" },
  { key: "control", label: "经营管控" },
  { key: "localization", label: "语言与本地化" },
  { key: "position-permissions", label: "岗位权限" },
  { key: "manufacturing", label: "制造管理" },
  { key: "advanced-manufacturing", label: "高级制造" },
  { key: "reporting", label: "报表中心" },
  { key: "quality", label: "质量追溯" },
  { key: "planning", label: "计划执行" },
];

const loadEmptyOverview = () => Promise.resolve(emptyOverview);

function formatDate(value?: string | null) {
  if (!value) {
    return "未完成";
  }

  return new Intl.DateTimeFormat("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function statusText(status: string) {
  switch (status) {
    case "Pending":
      return "待处理";
    case "Running":
      return "运行中";
    case "Completed":
      return "已完成";
    case "Failed":
      return "失败";
    default:
      return status || "未设置";
  }
}

function moduleText(key: string) {
  return moduleOptions.find((module) => module.key === key)?.label ?? key;
}

function categoryText(category: string) {
  switch (category) {
    case "ImportTemplate":
      return "导入模板";
    case "FieldMapping":
      return "字段映射";
    case "ImportBatch":
      return "导入批次";
    case "ExportFile":
      return "导出文件";
    case "PrintTemplate":
      return "打印模板";
    case "PrintJob":
      return "打印任务";
    default:
      return category || "未分类";
  }
}

function actionText(action: string) {
  switch (action) {
    case "ImportTemplateUpserted":
      return "导入模板已保存";
    case "FieldMappingUpserted":
      return "字段映射已保存";
    case "ImportBatchCreated":
      return "导入批次已创建";
    case "ImportBatchCompleted":
      return "导入批次已完成";
    case "ImportBatchFailed":
      return "导入批次已失败";
    case "ExportTaskCreated":
      return "导出任务已创建";
    case "ExportTaskCompleted":
      return "导出任务已完成";
    case "ExportTaskFailed":
      return "导出任务已失败";
    case "PrintTemplateUpserted":
      return "打印模板已保存";
    case "PrintJobCreated":
      return "打印任务已创建";
    case "PrintJobCompleted":
      return "打印任务已完成";
    case "PrintJobFailed":
      return "打印任务已失败";
    default:
      return action || "未记录动作";
  }
}

function resultText(result: string) {
  return result === "Success" ? "成功" : result === "Failed" ? "失败" : result || "未设置";
}

/** 文档交换页面，串联导入模板、字段映射、导入批次、导出任务和打印任务。 */
export function DocumentExchangePage() {
  const { hasPermission, user } = useAuth();
  const canRead = hasPermission(platformPermissions.documentExchangeRead);
  const canManage = hasPermission(platformPermissions.documentExchangeManage);
  const canExecute = hasPermission(platformPermissions.documentExchangeExecute);
  const overviewQuery = useAsyncData(canRead ? api.getDocumentExchangeOverview : loadEmptyOverview);
  const overview = overviewQuery.data ?? emptyOverview;

  const [importTemplateForm, setImportTemplateForm] = useState({
    templateKey: "",
    displayName: "",
    targetModule: "",
    fileType: "CSV",
    isEnabled: true,
  });
  const [mappingForm, setMappingForm] = useState({
    templateKey: "",
    sourceField: "",
    targetField: "",
    isRequired: true,
    transformRule: "",
  });
  const [batchForm, setBatchForm] = useState({
    templateKey: "",
    fileName: "",
  });
  const [exportForm, setExportForm] = useState({
    sourceModule: "",
    fileName: "",
    format: "CSV",
  });
  const [printTemplateForm, setPrintTemplateForm] = useState({
    templateKey: "",
    displayName: "",
    targetModule: "",
    contentType: "HTML",
    templateBody: "",
    isEnabled: true,
  });
  const [printJobForm, setPrintJobForm] = useState({
    templateKey: "",
    documentNo: "",
  });
  const [batchResults, setBatchResults] = useState<Record<string, { rowCount: number; errorCount: number }>>({});
  const [failReasons, setFailReasons] = useState<Record<string, string>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const visibleModuleKey = user?.visibleModuleKeys.join("|") ?? "";
  const availableModules = useMemo(
    () => moduleOptions.filter((module) => user?.visibleModuleKeys.includes(module.key)),
    [visibleModuleKey, user],
  );
  const enabledImportTemplates = useMemo(
    () => overview.importTemplates.filter((template) => template.isEnabled),
    [overview.importTemplates],
  );
  const enabledPrintTemplates = useMemo(
    () => overview.printTemplates.filter((template) => template.isEnabled),
    [overview.printTemplates],
  );
  const openImportBatches = useMemo(
    () => overview.importBatches.filter((batch) => batch.status !== "Completed"),
    [overview.importBatches],
  );
  const openFileTasks = useMemo(
    () => overview.exportTasks.filter((task) => task.status !== "Completed").length + overview.printJobs.filter((job) => job.status !== "Completed").length,
    [overview.exportTasks, overview.printJobs],
  );

  async function runAction(actionKey: string, action: () => Promise<void>, successText?: string) {
    setBusyKey(actionKey);
    setMessage(null);
    setError(null);
    try {
      await action();
      if (successText) {
        setMessage(successText);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "操作失败");
    } finally {
      setBusyKey(null);
    }
  }

  async function reloadOverview() {
    if (canRead) {
      await overviewQuery.reload();
    }
  }

  if (!canRead) {
    return (
      <PageShell title="文档交换">
        <EmptyState title="无文档交换查看权限" description="当前账号不能读取导入模板、文件任务、打印任务和文件审计。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="文档交换"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "document-exchange-refresh"}
          onClick={async () => {
            await runAction("document-exchange-refresh", reloadOverview, "文档交换数据已刷新。");
          }}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      }
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <section className="stats-grid inventory-kpi-grid">
        {(overview.metrics.length > 0 ? overview.metrics : [
          { key: "import-templates", label: "启用导入模板", value: enabledImportTemplates.length, unit: "个" },
          { key: "print-templates", label: "启用打印模板", value: enabledPrintTemplates.length, unit: "个" },
          { key: "open-imports", label: "未完成导入", value: openImportBatches.length, unit: "批" },
          { key: "open-files", label: "未完成文件任务", value: openFileTasks, unit: "个" },
        ]).map((metric) => (
          <StatTile key={metric.key} label={`${metric.label}（${metric.unit}）`} value={metric.value} tone={metric.value > 0 ? "success" : "default"} />
        ))}
      </section>

      {overviewQuery.loading ? <div className="section-note">正在加载文档交换...</div> : null}
      {overviewQuery.error ? <div className="section-note error">{overviewQuery.error}</div> : null}

      <SectionBlock title="导入模板" hint="模板定义文件类型和目标模块，字段映射绑定到启用模板。">
        <div className="inventory-surface-grid">
          <div className="inventory-surface">
            {overview.importTemplates.length > 0 ? (
              <div className="inventory-record-list">
                {overview.importTemplates.map((template) => (
                  <div key={template.id} className="inventory-record-row">
                    <div>
                      <strong>{template.templateKey} · {template.displayName}</strong>
                      <p>{moduleText(template.targetModule)} · {template.fileType} · {template.isEnabled ? "已启用" : "已停用"}</p>
                    </div>
                    <div className="inventory-record-meta">
                      <small>{template.updatedBy || "系统"}</small>
                      <small>{formatDate(template.updatedAtUtc)}</small>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无导入模板" description="保存导入模板后，可以继续维护字段映射和创建导入批次。" />
            )}
          </div>
          <div className="inventory-surface">
            {canManage ? (
              availableModules.length > 0 ? (
                <form
                  className="stack-form inventory-form-panel"
                  onSubmit={async (event) => {
                    event.preventDefault();
                    if (!importTemplateForm.templateKey.trim() || !importTemplateForm.displayName.trim() || !importTemplateForm.targetModule || !importTemplateForm.fileType.trim()) {
                      setError("请填写模板编码、名称、目标模块和文件类型。");
                      return;
                    }

                    await runAction("import-template-upsert", async () => {
                      await api.upsertImportTemplate({
                        templateKey: importTemplateForm.templateKey.trim(),
                        displayName: importTemplateForm.displayName.trim(),
                        targetModule: importTemplateForm.targetModule,
                        fileType: importTemplateForm.fileType.trim(),
                        isEnabled: importTemplateForm.isEnabled,
                      });
                      setImportTemplateForm({ templateKey: "", displayName: "", targetModule: "", fileType: "CSV", isEnabled: true });
                      await reloadOverview();
                    }, "导入模板已保存。");
                  }}
                >
                  <input placeholder="模板编码" value={importTemplateForm.templateKey} onChange={(event) => setImportTemplateForm({ ...importTemplateForm, templateKey: event.target.value })} />
                  <input placeholder="模板名称" value={importTemplateForm.displayName} onChange={(event) => setImportTemplateForm({ ...importTemplateForm, displayName: event.target.value })} />
                  <select value={importTemplateForm.targetModule} onChange={(event) => setImportTemplateForm({ ...importTemplateForm, targetModule: event.target.value })}>
                    <option value="">选择目标模块</option>
                    {availableModules.map((module) => (
                      <option key={module.key} value={module.key}>{module.label}</option>
                    ))}
                  </select>
                  <select value={importTemplateForm.fileType} onChange={(event) => setImportTemplateForm({ ...importTemplateForm, fileType: event.target.value })}>
                    <option value="CSV">CSV 文件</option>
                    <option value="XLSX">Excel 文件</option>
                    <option value="JSON">JSON 文件</option>
                  </select>
                  <label className="checkbox-row">
                    <input type="checkbox" checked={importTemplateForm.isEnabled} onChange={(event) => setImportTemplateForm({ ...importTemplateForm, isEnabled: event.target.checked })} />
                    启用导入模板
                  </label>
                  <button type="submit" disabled={busyKey === "import-template-upsert" || !importTemplateForm.templateKey.trim() || !importTemplateForm.displayName.trim() || !importTemplateForm.targetModule}>
                    保存导入模板
                  </button>
                </form>
              ) : (
                <EmptyState title="暂无可选目标模块" description="当前账号没有可用于文档交换的业务模块访问权。" />
              )
            ) : (
              <EmptyState title="无模板维护权限" description="当前账号只能查看导入模板和字段映射。" />
            )}
          </div>
        </div>
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="字段映射" hint="字段映射说明来源字段如何写入目标字段。">
          {overview.fieldMappings.length > 0 ? (
            <div className="inventory-record-list">
              {overview.fieldMappings.map((mapping) => (
                <div key={mapping.id} className="inventory-record-row">
                  <div>
                    <strong>{mapping.templateKey} · {mapping.sourceField} → {mapping.targetField}</strong>
                    <p>{mapping.isRequired ? "必填字段" : "可选字段"}</p>
                    <small>{mapping.transformRule || "无转换规则"}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{mapping.updatedBy || "系统"}</small>
                    <small>{formatDate(mapping.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无字段映射" description="启用导入模板后，可以为模板维护来源字段和目标字段。" />
          )}

          {canManage ? (
            enabledImportTemplates.length > 0 ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!mappingForm.templateKey || !mappingForm.sourceField.trim() || !mappingForm.targetField.trim()) {
                    setError("请选择模板，并填写来源字段和目标字段。");
                    return;
                  }

                  await runAction("field-mapping-upsert", async () => {
                    await api.upsertImportFieldMapping({
                      templateKey: mappingForm.templateKey,
                      sourceField: mappingForm.sourceField.trim(),
                      targetField: mappingForm.targetField.trim(),
                      isRequired: mappingForm.isRequired,
                      transformRule: mappingForm.transformRule.trim(),
                    });
                    setMappingForm({ templateKey: "", sourceField: "", targetField: "", isRequired: true, transformRule: "" });
                    await reloadOverview();
                  }, "字段映射已保存。");
                }}
              >
                <select value={mappingForm.templateKey} onChange={(event) => setMappingForm({ ...mappingForm, templateKey: event.target.value })}>
                  <option value="">选择导入模板</option>
                  {enabledImportTemplates.map((template) => (
                    <option key={template.id} value={template.templateKey}>{template.templateKey} · {template.displayName}</option>
                  ))}
                </select>
                <input placeholder="来源字段" value={mappingForm.sourceField} onChange={(event) => setMappingForm({ ...mappingForm, sourceField: event.target.value })} />
                <input placeholder="目标字段" value={mappingForm.targetField} onChange={(event) => setMappingForm({ ...mappingForm, targetField: event.target.value })} />
                <input placeholder="转换规则" value={mappingForm.transformRule} onChange={(event) => setMappingForm({ ...mappingForm, transformRule: event.target.value })} />
                <label className="checkbox-row">
                  <input type="checkbox" checked={mappingForm.isRequired} onChange={(event) => setMappingForm({ ...mappingForm, isRequired: event.target.checked })} />
                  必填字段
                </label>
                <button type="submit" disabled={busyKey === "field-mapping-upsert" || !mappingForm.templateKey || !mappingForm.sourceField.trim() || !mappingForm.targetField.trim()}>
                  保存字段映射
                </button>
              </form>
            ) : (
              <EmptyState title="没有启用导入模板" description="先保存并启用导入模板，再维护字段映射。" />
            )
          ) : null}
        </SectionBlock>

        <SectionBlock title="导入批次" hint="批次记录文件名、模板、处理行数、错误数和执行状态。">
          {overview.importBatches.length > 0 ? (
            <div className="inventory-record-list">
              {overview.importBatches.map((batch) => {
                const result = batchResults[batch.id] ?? { rowCount: batch.rowCount, errorCount: batch.errorCount };
                const failReason = failReasons[batch.id] ?? batch.errorMessage ?? "";
                return (
                  <div key={batch.id} className="inventory-record-row">
                    <div>
                      <strong>{batch.batchNo} · {batch.fileName}</strong>
                      <p>{batch.templateKey} · {statusText(batch.status)}</p>
                      <small>行数：{batch.rowCount} · 错误：{batch.errorCount} · 创建：{batch.createdBy || "系统"}</small>
                      {batch.errorMessage ? <small>失败原因：{batch.errorMessage}</small> : null}
                    </div>
                    <div className="inventory-record-meta">
                      {canExecute && batch.status !== "Completed" ? (
                        <>
                          <input aria-label="处理行数" type="number" min={0} value={result.rowCount} onChange={(event) => setBatchResults({ ...batchResults, [batch.id]: { ...result, rowCount: Number(event.target.value) } })} />
                          <input aria-label="错误行数" type="number" min={0} value={result.errorCount} onChange={(event) => setBatchResults({ ...batchResults, [batch.id]: { ...result, errorCount: Number(event.target.value) } })} />
                          <button
                            disabled={busyKey === `batch-complete-${batch.id}`}
                            onClick={async () => {
                              await runAction(`batch-complete-${batch.id}`, async () => {
                                await api.completeImportBatch(batch.id, result);
                                await reloadOverview();
                              }, `${batch.batchNo} 已完成。`);
                            }}
                          >
                            完成导入
                          </button>
                          <input placeholder="失败原因" value={failReason} onChange={(event) => setFailReasons({ ...failReasons, [batch.id]: event.target.value })} />
                          <button
                            className="secondary"
                            disabled={busyKey === `batch-fail-${batch.id}` || !failReason.trim()}
                            onClick={async () => {
                              await runAction(`batch-fail-${batch.id}`, async () => {
                                await api.failImportBatch(batch.id, failReason.trim());
                                await reloadOverview();
                              }, `${batch.batchNo} 已标记失败。`);
                            }}
                          >
                            标记失败
                          </button>
                        </>
                      ) : (
                        <small>完成人：{batch.completedBy || "未完成"}</small>
                      )}
                      <small>{formatDate(batch.completedAtUtc ?? batch.updatedAtUtc)}</small>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无导入批次" description="基于启用模板创建批次后，这里会显示处理状态。" />
          )}

          {canManage ? (
            enabledImportTemplates.length > 0 ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!batchForm.templateKey || !batchForm.fileName.trim()) {
                    setError("请选择导入模板并填写文件名。");
                    return;
                  }

                  await runAction("batch-create", async () => {
                    await api.createImportBatch({
                      templateKey: batchForm.templateKey,
                      fileName: batchForm.fileName.trim(),
                    });
                    setBatchForm({ templateKey: "", fileName: "" });
                    await reloadOverview();
                  }, "导入批次已创建。");
                }}
              >
                <select value={batchForm.templateKey} onChange={(event) => setBatchForm({ ...batchForm, templateKey: event.target.value })}>
                  <option value="">选择导入模板</option>
                  {enabledImportTemplates.map((template) => (
                    <option key={template.id} value={template.templateKey}>{template.templateKey} · {template.displayName}</option>
                  ))}
                </select>
                <input placeholder="文件名" value={batchForm.fileName} onChange={(event) => setBatchForm({ ...batchForm, fileName: event.target.value })} />
                <button type="submit" disabled={busyKey === "batch-create" || !batchForm.templateKey || !batchForm.fileName.trim()}>
                  创建导入批次
                </button>
              </form>
            ) : null
          ) : null}
        </SectionBlock>
      </div>

      <div className="split-grid">
        <SectionBlock title="导出文件任务" hint="导出任务按来源模块、文件名和格式独立记录。">
          {overview.exportTasks.length > 0 ? (
            <div className="inventory-record-list">
              {overview.exportTasks.map((task) => {
                const failReason = failReasons[task.id] ?? "";
                return (
                  <div key={task.id} className="inventory-record-row">
                    <div>
                      <strong>{task.exportNo} · {task.fileName}</strong>
                      <p>{moduleText(task.sourceModule)} · {task.format} · {statusText(task.status)}</p>
                      <small>申请人：{task.requestedBy || "系统"}</small>
                    </div>
                    <div className="inventory-record-meta">
                      {canExecute && task.status !== "Completed" ? (
                        <>
                          <button
                            disabled={busyKey === `export-complete-${task.id}`}
                            onClick={async () => {
                              await runAction(`export-complete-${task.id}`, async () => {
                                await api.completeExportFileTask(task.id);
                                await reloadOverview();
                              }, `${task.exportNo} 已完成。`);
                            }}
                          >
                            完成导出
                          </button>
                          <input placeholder="失败原因" value={failReason} onChange={(event) => setFailReasons({ ...failReasons, [task.id]: event.target.value })} />
                          <button
                            className="secondary"
                            disabled={busyKey === `export-fail-${task.id}` || !failReason.trim()}
                            onClick={async () => {
                              await runAction(`export-fail-${task.id}`, async () => {
                                await api.failExportFileTask(task.id, failReason.trim());
                                await reloadOverview();
                              }, `${task.exportNo} 已标记失败。`);
                            }}
                          >
                            标记失败
                          </button>
                        </>
                      ) : (
                        <small>完成人：{task.completedBy || "未完成"}</small>
                      )}
                      <small>{formatDate(task.completedAtUtc ?? task.updatedAtUtc)}</small>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无导出任务" description="创建导出文件任务后，可以在这里推进完成或失败状态。" />
          )}

          {canManage ? (
            availableModules.length > 0 ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!exportForm.sourceModule || !exportForm.fileName.trim() || !exportForm.format.trim()) {
                    setError("请选择来源模块，并填写文件名和格式。");
                    return;
                  }

                  await runAction("export-create", async () => {
                    await api.createExportFileTask({
                      sourceModule: exportForm.sourceModule,
                      fileName: exportForm.fileName.trim(),
                      format: exportForm.format.trim(),
                    });
                    setExportForm({ sourceModule: "", fileName: "", format: "CSV" });
                    await reloadOverview();
                  }, "导出文件任务已创建。");
                }}
              >
                <select value={exportForm.sourceModule} onChange={(event) => setExportForm({ ...exportForm, sourceModule: event.target.value })}>
                  <option value="">选择来源模块</option>
                  {availableModules.map((module) => (
                    <option key={module.key} value={module.key}>{module.label}</option>
                  ))}
                </select>
                <input placeholder="文件名" value={exportForm.fileName} onChange={(event) => setExportForm({ ...exportForm, fileName: event.target.value })} />
                <select value={exportForm.format} onChange={(event) => setExportForm({ ...exportForm, format: event.target.value })}>
                  <option value="CSV">CSV 文件</option>
                  <option value="XLSX">Excel 文件</option>
                  <option value="PDF">PDF 文件</option>
                  <option value="JSON">JSON 文件</option>
                </select>
                <button type="submit" disabled={busyKey === "export-create" || !exportForm.sourceModule || !exportForm.fileName.trim()}>
                  创建导出任务
                </button>
              </form>
            ) : null
          ) : null}
        </SectionBlock>

        <SectionBlock title="打印模板" hint="打印模板保存目标模块、内容类型和模板正文。">
          {overview.printTemplates.length > 0 ? (
            <div className="inventory-record-list">
              {overview.printTemplates.map((template) => (
                <div key={template.id} className="inventory-record-row">
                  <div>
                    <strong>{template.templateKey} · {template.displayName}</strong>
                    <p>{moduleText(template.targetModule)} · {template.contentType} · {template.isEnabled ? "已启用" : "已停用"}</p>
                    <small>{template.templateBody || "未填写模板正文"}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{template.updatedBy || "系统"}</small>
                    <small>{formatDate(template.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无打印模板" description="保存打印模板后，可以按模板创建打印任务。" />
          )}

          {canManage ? (
            availableModules.length > 0 ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!printTemplateForm.templateKey.trim() || !printTemplateForm.displayName.trim() || !printTemplateForm.targetModule || !printTemplateForm.contentType.trim()) {
                    setError("请填写模板编码、名称、目标模块和内容类型。");
                    return;
                  }

                  await runAction("print-template-upsert", async () => {
                    await api.upsertPrintTemplate({
                      templateKey: printTemplateForm.templateKey.trim(),
                      displayName: printTemplateForm.displayName.trim(),
                      targetModule: printTemplateForm.targetModule,
                      contentType: printTemplateForm.contentType.trim(),
                      templateBody: printTemplateForm.templateBody.trim(),
                      isEnabled: printTemplateForm.isEnabled,
                    });
                    setPrintTemplateForm({ templateKey: "", displayName: "", targetModule: "", contentType: "HTML", templateBody: "", isEnabled: true });
                    await reloadOverview();
                  }, "打印模板已保存。");
                }}
              >
                <input placeholder="模板编码" value={printTemplateForm.templateKey} onChange={(event) => setPrintTemplateForm({ ...printTemplateForm, templateKey: event.target.value })} />
                <input placeholder="模板名称" value={printTemplateForm.displayName} onChange={(event) => setPrintTemplateForm({ ...printTemplateForm, displayName: event.target.value })} />
                <select value={printTemplateForm.targetModule} onChange={(event) => setPrintTemplateForm({ ...printTemplateForm, targetModule: event.target.value })}>
                  <option value="">选择目标模块</option>
                  {availableModules.map((module) => (
                    <option key={module.key} value={module.key}>{module.label}</option>
                  ))}
                </select>
                <select value={printTemplateForm.contentType} onChange={(event) => setPrintTemplateForm({ ...printTemplateForm, contentType: event.target.value })}>
                  <option value="HTML">网页模板</option>
                  <option value="PDF">PDF 模板</option>
                  <option value="TEXT">纯文本模板</option>
                </select>
                <textarea rows={4} placeholder="模板正文" value={printTemplateForm.templateBody} onChange={(event) => setPrintTemplateForm({ ...printTemplateForm, templateBody: event.target.value })} />
                <label className="checkbox-row">
                  <input type="checkbox" checked={printTemplateForm.isEnabled} onChange={(event) => setPrintTemplateForm({ ...printTemplateForm, isEnabled: event.target.checked })} />
                  启用打印模板
                </label>
                <button type="submit" disabled={busyKey === "print-template-upsert" || !printTemplateForm.templateKey.trim() || !printTemplateForm.displayName.trim() || !printTemplateForm.targetModule}>
                  保存打印模板
                </button>
              </form>
            ) : null
          ) : null}
        </SectionBlock>
      </div>

      <div className="split-grid">
        <SectionBlock title="打印任务" hint="打印任务绑定启用模板和业务单据号。">
          {overview.printJobs.length > 0 ? (
            <div className="inventory-record-list">
              {overview.printJobs.map((job) => {
                const failReason = failReasons[job.id] ?? "";
                return (
                  <div key={job.id} className="inventory-record-row">
                    <div>
                      <strong>{job.jobNo} · {job.documentNo}</strong>
                      <p>{job.templateKey} · {statusText(job.status)}</p>
                      <small>申请人：{job.requestedBy || "系统"}</small>
                    </div>
                    <div className="inventory-record-meta">
                      {canExecute && job.status !== "Completed" ? (
                        <>
                          <button
                            disabled={busyKey === `print-complete-${job.id}`}
                            onClick={async () => {
                              await runAction(`print-complete-${job.id}`, async () => {
                                await api.completePrintJob(job.id);
                                await reloadOverview();
                              }, `${job.jobNo} 已完成。`);
                            }}
                          >
                            完成打印
                          </button>
                          <input placeholder="失败原因" value={failReason} onChange={(event) => setFailReasons({ ...failReasons, [job.id]: event.target.value })} />
                          <button
                            className="secondary"
                            disabled={busyKey === `print-fail-${job.id}` || !failReason.trim()}
                            onClick={async () => {
                              await runAction(`print-fail-${job.id}`, async () => {
                                await api.failPrintJob(job.id, failReason.trim());
                                await reloadOverview();
                              }, `${job.jobNo} 已标记失败。`);
                            }}
                          >
                            标记失败
                          </button>
                        </>
                      ) : (
                        <small>完成人：{job.completedBy || "未完成"}</small>
                      )}
                      <small>{formatDate(job.completedAtUtc ?? job.updatedAtUtc)}</small>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无打印任务" description="基于启用打印模板创建任务后，这里会显示执行状态。" />
          )}

          {canManage ? (
            enabledPrintTemplates.length > 0 ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!printJobForm.templateKey || !printJobForm.documentNo.trim()) {
                    setError("请选择打印模板并填写单据号。");
                    return;
                  }

                  await runAction("print-job-create", async () => {
                    await api.createPrintJob({
                      templateKey: printJobForm.templateKey,
                      documentNo: printJobForm.documentNo.trim(),
                    });
                    setPrintJobForm({ templateKey: "", documentNo: "" });
                    await reloadOverview();
                  }, "打印任务已创建。");
                }}
              >
                <select value={printJobForm.templateKey} onChange={(event) => setPrintJobForm({ ...printJobForm, templateKey: event.target.value })}>
                  <option value="">选择打印模板</option>
                  {enabledPrintTemplates.map((template) => (
                    <option key={template.id} value={template.templateKey}>{template.templateKey} · {template.displayName}</option>
                  ))}
                </select>
                <input placeholder="单据号" value={printJobForm.documentNo} onChange={(event) => setPrintJobForm({ ...printJobForm, documentNo: event.target.value })} />
                <button type="submit" disabled={busyKey === "print-job-create" || !printJobForm.templateKey || !printJobForm.documentNo.trim()}>
                  创建打印任务
                </button>
              </form>
            ) : (
              <EmptyState title="没有启用打印模板" description="先保存并启用打印模板，再创建打印任务。" />
            )
          ) : null}
        </SectionBlock>

        <SectionBlock title="文件审计" hint="服务端会记录维护、创建和执行动作。">
          {overview.auditRecords.length > 0 ? (
            <div className="inventory-record-list">
              {overview.auditRecords.map((audit) => (
                <div key={audit.id} className="inventory-record-row">
                  <div>
                    <strong>{audit.auditNo} · {actionText(audit.action)}</strong>
                    <p>{categoryText(audit.category)} · {audit.targetNo} · {resultText(audit.result)}</p>
                    <small>{audit.message}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{audit.actor || "系统"}</small>
                    <small>{formatDate(audit.createdAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无文件审计" description="保存模板、创建任务或推进执行状态后，审计记录会显示在这里。" />
          )}
        </SectionBlock>
      </div>
    </PageShell>
  );
}
