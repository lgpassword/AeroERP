import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { BusinessMetric, ReportingOverview } from "../types/api";

const emptyOverview: ReportingOverview = {
  definitions: [],
  runs: [],
  exportTasks: [],
  liveMetrics: [],
};

const loadEmptyOverview = () => Promise.resolve(emptyOverview);

const queryModels = [
  { key: "operations-summary", label: "经营总览" },
  { key: "procurement-summary", label: "采购汇总" },
  { key: "sales-summary", label: "销售汇总" },
  { key: "inventory-summary", label: "库存汇总" },
  { key: "finance-summary", label: "财务汇总" },
  { key: "manufacturing-summary", label: "制造汇总" },
];

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
      return "处理中";
    case "Completed":
      return "已完成";
    case "Failed":
      return "失败";
    default:
      return status || "未设置";
  }
}

function parseMetrics(value: string): BusinessMetric[] {
  try {
    const parsed = JSON.parse(value) as BusinessMetric[];
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

/** 报表页面，维护报表定义，执行报表并创建导出任务。 */
export function ReportingPage() {
  const { hasPermission } = useAuth();
  const canRead = hasPermission(platformPermissions.reportingRead);
  const canManage = hasPermission(platformPermissions.reportingManage);
  const canExport = hasPermission(platformPermissions.reportingExport);

  const overviewQuery = useAsyncData(canRead ? api.getReportingOverview : loadEmptyOverview);
  const overview = overviewQuery.data ?? emptyOverview;

  const [definitionForm, setDefinitionForm] = useState({
    key: "",
    displayName: "",
    category: "经营报表",
    queryModel: "operations-summary",
    parametersJson: "{}",
    isEnabled: true,
  });
  const [runForm, setRunForm] = useState({
    reportDefinitionId: "",
    parametersJson: "{}",
  });
  const [exportFormats, setExportFormats] = useState<Record<string, string>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const enabledDefinitions = useMemo(() => overview.definitions.filter((definition) => definition.isEnabled), [overview.definitions]);

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
      <PageShell title="报表中心">
        <EmptyState title="无报表查看权限" description="当前账号不能读取报表定义、运行记录和导出任务。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="报表中心"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "reporting-refresh"}
          onClick={async () => {
            await runAction("reporting-refresh", reloadOverview, "报表中心数据已刷新。");
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
        {(overview.liveMetrics.length > 0 ? overview.liveMetrics.slice(0, 4) : [
          { key: "definitions", label: "报表定义", value: overview.definitions.length, unit: "个" },
          { key: "runs", label: "运行记录", value: overview.runs.length, unit: "次" },
          { key: "exports", label: "导出任务", value: overview.exportTasks.length, unit: "个" },
          { key: "enabled", label: "启用定义", value: enabledDefinitions.length, unit: "个" },
        ]).map((metric) => (
          <StatTile key={metric.key} label={`${metric.label}（${metric.unit}）`} value={metric.value} tone={metric.value > 0 ? "success" : "default"} />
        ))}
      </section>

      {overviewQuery.loading ? <div className="section-note">正在加载报表中心...</div> : null}
      {overviewQuery.error ? <div className="section-note error">{overviewQuery.error}</div> : null}

      <div className="split-grid">
        <SectionBlock title="报表定义" hint="定义只保存查询模型和参数，不复制业务明细数据。">
          {overview.definitions.length > 0 ? (
            <div className="inventory-record-list">
              {overview.definitions.map((definition) => (
                <div key={definition.id} className="inventory-record-row">
                  <div>
                    <strong>{definition.key} · {definition.displayName}</strong>
                    <p>{definition.category} · {queryModels.find((item) => item.key === definition.queryModel)?.label ?? definition.queryModel}</p>
                    <small>{definition.isEnabled ? "已启用" : "已停用"} · {definition.parametersJson}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{definition.updatedBy || "系统"}</small>
                    <small>{formatDate(definition.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无报表定义" description="创建报表定义后，可以运行并产生真实报表记录。" />
          )}

          {canManage ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                if (!definitionForm.key.trim() || !definitionForm.displayName.trim() || !definitionForm.queryModel) {
                  setError("请填写报表编码、名称和查询模型。");
                  return;
                }

                await runAction("definition-upsert", async () => {
                  await api.upsertReportDefinition(definitionForm);
                  setDefinitionForm({ key: "", displayName: "", category: "经营报表", queryModel: "operations-summary", parametersJson: "{}", isEnabled: true });
                  await reloadOverview();
                }, "报表定义已保存。");
              }}
            >
              <input placeholder="报表编码" value={definitionForm.key} onChange={(event) => setDefinitionForm({ ...definitionForm, key: event.target.value })} />
              <input placeholder="报表名称" value={definitionForm.displayName} onChange={(event) => setDefinitionForm({ ...definitionForm, displayName: event.target.value })} />
              <input placeholder="分类" value={definitionForm.category} onChange={(event) => setDefinitionForm({ ...definitionForm, category: event.target.value })} />
              <select value={definitionForm.queryModel} onChange={(event) => setDefinitionForm({ ...definitionForm, queryModel: event.target.value })}>
                {queryModels.map((model) => (
                  <option key={model.key} value={model.key}>{model.label}</option>
                ))}
              </select>
              <input placeholder="参数 JSON" value={definitionForm.parametersJson} onChange={(event) => setDefinitionForm({ ...definitionForm, parametersJson: event.target.value })} />
              <label className="checkbox-row">
                <input type="checkbox" checked={definitionForm.isEnabled} onChange={(event) => setDefinitionForm({ ...definitionForm, isEnabled: event.target.checked })} />
                启用报表
              </label>
              <button type="submit" disabled={busyKey === "definition-upsert"}>保存定义</button>
            </form>
          ) : null}
        </SectionBlock>

        <SectionBlock title="运行报表" hint="运行记录保存当时的参数和结果摘要，结果来自现有业务表聚合。">
          {overview.runs.length > 0 ? (
            <div className="inventory-record-list">
              {overview.runs.map((run) => {
                const metrics = parseMetrics(run.resultSummaryJson);
                const format = exportFormats[run.id] ?? "CSV";
                return (
                  <div key={run.id} className="inventory-record-row">
                    <div>
                      <strong>{run.runNo} · {run.reportName}</strong>
                      <p>{statusText(run.status)} · 行数 {run.rowCount}</p>
                      <div className="inventory-lines">
                        {metrics.slice(0, 4).map((metric) => (
                          <span key={metric.key}>{metric.label}: {metric.value} {metric.unit}</span>
                        ))}
                      </div>
                    </div>
                    <div className="inventory-record-meta">
                      {canExport && run.status === "Completed" ? (
                        <>
                          <select value={format} onChange={(event) => setExportFormats({ ...exportFormats, [run.id]: event.target.value })}>
                            <option value="CSV">CSV</option>
                            <option value="XLSX">XLSX</option>
                          </select>
                          <button
                            disabled={busyKey === `export-${run.id}`}
                            onClick={async () => {
                              await runAction(`export-${run.id}`, async () => {
                                await api.createReportExportTask({ reportRunRecordId: run.id, format });
                                await reloadOverview();
                              }, `${run.runNo} 已创建导出任务。`);
                            }}
                          >
                            创建导出
                          </button>
                        </>
                      ) : null}
                      <small>{run.runBy}</small>
                      <small>{formatDate(run.completedAtUtc ?? run.updatedAtUtc)}</small>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无运行记录" description="运行报表后，这里会保存结果摘要和执行人。" />
          )}

          {enabledDefinitions.length > 0 ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                if (!runForm.reportDefinitionId) {
                  setError("请选择要运行的报表定义。");
                  return;
                }

                await runAction("report-run", async () => {
                  await api.runReport(runForm);
                  setRunForm({ reportDefinitionId: "", parametersJson: "{}" });
                  await reloadOverview();
                }, "报表已运行。");
              }}
            >
              <select value={runForm.reportDefinitionId} onChange={(event) => setRunForm({ ...runForm, reportDefinitionId: event.target.value })}>
                <option value="">选择报表定义</option>
                {enabledDefinitions.map((definition) => (
                  <option key={definition.id} value={definition.id}>{definition.key} · {definition.displayName}</option>
                ))}
              </select>
              <input placeholder="运行参数 JSON" value={runForm.parametersJson} onChange={(event) => setRunForm({ ...runForm, parametersJson: event.target.value })} />
              <button type="submit" disabled={busyKey === "report-run"}>运行报表</button>
            </form>
          ) : (
            <EmptyState title="没有启用的报表定义" description="先创建并启用报表定义，再运行报表。" />
          )}
        </SectionBlock>
      </div>

      <SectionBlock title="导出任务" hint="导出任务是持久化任务记录，文件名由服务端生成。">
        {overview.exportTasks.length > 0 ? (
          <div className="inventory-record-list">
            {overview.exportTasks.map((task) => (
              <div key={task.id} className="inventory-record-row">
                <div>
                  <strong>{task.exportNo} · {task.reportName}</strong>
                  <p>{task.format} · {task.fileName}</p>
                </div>
                <div className="inventory-record-meta">
                  <small>{statusText(task.status)}</small>
                  <small>{task.requestedBy}</small>
                  <small>{formatDate(task.completedAtUtc ?? task.updatedAtUtc)}</small>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState title="暂无导出任务" description="对已完成运行记录创建导出后，这里会显示任务和文件名。" />
        )}
      </SectionBlock>
    </PageShell>
  );
}
