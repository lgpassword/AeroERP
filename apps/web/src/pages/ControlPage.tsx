import { RefreshCcw } from "lucide-react";
import { useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { AnalyticsSnapshot, DataScopeRule, NumberingRule, RoleSummary } from "../types/api";

const emptyAnalytics = (): Promise<AnalyticsSnapshot> => Promise.resolve({
  procurement: [],
  sales: [],
  inventory: [],
  finance: [],
  generatedAtUtc: new Date().toISOString(),
});
const loadEmptyDataScopeRules = () => Promise.resolve<DataScopeRule[]>([]);
const loadEmptyNumberingRules = () => Promise.resolve<NumberingRule[]>([]);
const loadEmptyRoles = () => Promise.resolve<RoleSummary[]>([]);

function documentTypeText(value: string) {
  switch (value) {
    case "ProcurementRequest":
      return "采购申请";
    case "SalesQuotation":
      return "销售报价";
    default:
      return value;
  }
}

function scopeTypeText(value: string) {
  switch (value) {
    case "SalesCustomerName":
      return "销售客户名称";
    default:
      return value;
  }
}

/** 经营管控页面，展示运营指标，并维护数据范围规则和单据编号规则。 */
export function ControlPage() {
  const { hasPermission } = useAuth();
  const canReadAnalytics = hasPermission(platformPermissions.controlAnalyticsRead);
  const canManageDataScope = hasPermission(platformPermissions.controlDataScopeManage);
  const canManageNumbering = hasPermission(platformPermissions.controlNumberingManage);

  const analyticsQuery = useAsyncData(canReadAnalytics ? api.getAnalytics : emptyAnalytics);
  const dataScopeQuery = useAsyncData(canManageDataScope ? api.listDataScopeRules : loadEmptyDataScopeRules);
  const rolesQuery = useAsyncData(canManageDataScope ? api.listRoleOptions : loadEmptyRoles);
  const numberingQuery = useAsyncData(canManageNumbering ? api.listNumberingRules : loadEmptyNumberingRules);

  const [scopeForm, setScopeForm] = useState({
    roleKey: "",
    scopeType: "SalesCustomerName",
    matchValue: "",
    description: "",
    isEnabled: true,
  });
  const [numberingForm, setNumberingForm] = useState({
    documentType: "ProcurementRequest",
    prefix: "PR-",
    useDateSegment: true,
    padding: 4,
    isEnabled: true,
  });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const analytics = analyticsQuery.data;
  const roleLabelMap = new Map((rolesQuery.data ?? []).map((role) => [role.key, role.displayName]));
  const allMetrics = [
    ...(analytics?.procurement ?? []),
    ...(analytics?.sales ?? []),
    ...(analytics?.inventory ?? []),
    ...(analytics?.finance ?? []),
  ];

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

  async function reloadAll() {
    const tasks: Promise<unknown>[] = [];
    if (canReadAnalytics) {
      tasks.push(analyticsQuery.reload());
    }
    if (canManageDataScope) {
      tasks.push(dataScopeQuery.reload(), rolesQuery.reload());
    }
    if (canManageNumbering) {
      tasks.push(numberingQuery.reload());
    }
    await Promise.all(tasks);
  }

  if (!canReadAnalytics && !canManageDataScope && !canManageNumbering) {
    return (
      <PageShell title="经营管控">
        <EmptyState title="无经营管控权限" description="当前账号不能查看统计、数据范围或编号规则。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="经营管控"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "control-refresh"}
          onClick={async () => {
            await runAction("control-refresh", reloadAll, "经营管控数据已刷新。");
          }}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      }
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      {canReadAnalytics ? (
        analyticsQuery.loading ? (
          <div className="section-note">正在加载经营统计...</div>
        ) : analyticsQuery.error ? (
          <div className="section-note error">{analyticsQuery.error}</div>
        ) : allMetrics.length > 0 ? (
          <section className="stats-grid">
            {allMetrics.slice(0, 8).map((metric) => (
              <StatTile
                key={metric.key}
                label={metric.label}
                value={`${metric.value}${metric.unit}`}
                tone={metric.value > 0 ? "success" : "default"}
              />
            ))}
          </section>
        ) : (
          <EmptyState title="暂无统计指标" description="业务发生后，这里会显示真实经营指标。" />
        )
      ) : (
        <EmptyState title="无报表查看权限" description="当前账号不能查看经营统计。" />
      )}

      <div className="split-grid">
        <SectionBlock title="数据范围规则" hint="当前最小范围控制为销售订单按客户名称关键字过滤，规则对非平台管理员角色生效。">
          {canManageDataScope ? (
            <>
              {(dataScopeQuery.data ?? []).length > 0 ? (
                <div className="table-shell">
                  {dataScopeQuery.data?.map((rule) => (
                    <div key={rule.id} className="review-card">
                      <div>
                        <strong>{roleLabelMap.get(rule.roleKey) ?? rule.roleKey} · {scopeTypeText(rule.scopeType)}</strong>
                        <p>{rule.matchValue || "空关键字"} · {rule.description || "无备注"}</p>
                        <small>{rule.isEnabled ? "启用" : "停用"}</small>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState title="暂无数据范围规则" description="新增规则后，匹配角色的销售订单列表会按客户名称过滤。" />
              )}

              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  await runAction("data-scope-upsert", async () => {
                    await api.upsertDataScopeRule(scopeForm);
                    setScopeForm({ roleKey: "", scopeType: "SalesCustomerName", matchValue: "", description: "", isEnabled: true });
                    await dataScopeQuery.reload();
                  }, "数据范围规则已保存。");
                }}
              >
                <select value={scopeForm.roleKey} onChange={(event) => setScopeForm({ ...scopeForm, roleKey: event.target.value })}>
                  <option value="">选择职位/角色</option>
                  {(rolesQuery.data ?? []).map((role) => (
                    <option key={role.id} value={role.key}>{role.displayName}</option>
                  ))}
                </select>
                <select value={scopeForm.scopeType} onChange={(event) => setScopeForm({ ...scopeForm, scopeType: event.target.value })}>
                  <option value="SalesCustomerName">销售客户名称</option>
                </select>
                <input placeholder="客户名称关键字" value={scopeForm.matchValue} onChange={(event) => setScopeForm({ ...scopeForm, matchValue: event.target.value })} />
                <input placeholder="规则说明" value={scopeForm.description} onChange={(event) => setScopeForm({ ...scopeForm, description: event.target.value })} />
                <label className="checkbox-row">
                  <input type="checkbox" checked={scopeForm.isEnabled} onChange={(event) => setScopeForm({ ...scopeForm, isEnabled: event.target.checked })} />
                  <span>启用规则</span>
                </label>
                <button type="submit" disabled={busyKey === "data-scope-upsert" || !scopeForm.roleKey.trim()}>保存数据范围</button>
              </form>
            </>
          ) : (
            <EmptyState title="无数据范围治理权限" description="当前账号不能配置数据范围规则。" />
          )}
        </SectionBlock>

        <SectionBlock title="单据编号规则" hint="保存后会影响后续新建采购申请或销售报价编号。">
          {canManageNumbering ? (
            <>
              {(numberingQuery.data ?? []).length > 0 ? (
                <div className="table-shell">
                  {numberingQuery.data?.map((rule) => (
                    <div key={rule.id} className="review-card">
                      <div>
                        <strong>{documentTypeText(rule.documentType)} · {rule.prefix}</strong>
                        <p>下一流水：{rule.nextSequence} · 位数：{rule.padding} · {rule.useDateSegment ? "含日期段" : "不含日期段"}</p>
                        <small>{rule.isEnabled ? "启用" : "停用"}</small>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState title="暂无编号规则" description="系统初始化会准备基础编号规则；如为空请检查后端启动日志。" />
              )}

              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  await runAction("numbering-upsert", async () => {
                    await api.upsertNumberingRule(numberingForm);
                    await numberingQuery.reload();
                  }, "编号规则已保存。");
                }}
              >
                <select
                  value={numberingForm.documentType}
                  onChange={(event) => {
                    const documentType = event.target.value;
                    setNumberingForm({
                      ...numberingForm,
                      documentType,
                      prefix: documentType === "SalesQuotation" ? "SQ-" : "PR-",
                    });
                  }}
                >
                  <option value="ProcurementRequest">采购申请</option>
                  <option value="SalesQuotation">销售报价</option>
                </select>
                <input placeholder="编号前缀" value={numberingForm.prefix} onChange={(event) => setNumberingForm({ ...numberingForm, prefix: event.target.value })} />
                <input
                  type="number"
                  min={2}
                  max={8}
                  value={numberingForm.padding}
                  onChange={(event) => setNumberingForm({ ...numberingForm, padding: Number(event.target.value) })}
                />
                <label className="checkbox-row">
                  <input type="checkbox" checked={numberingForm.useDateSegment} onChange={(event) => setNumberingForm({ ...numberingForm, useDateSegment: event.target.checked })} />
                  <span>包含日期段</span>
                </label>
                <label className="checkbox-row">
                  <input type="checkbox" checked={numberingForm.isEnabled} onChange={(event) => setNumberingForm({ ...numberingForm, isEnabled: event.target.checked })} />
                  <span>启用规则</span>
                </label>
                <button type="submit" disabled={busyKey === "numbering-upsert" || !numberingForm.prefix.trim()}>保存编号规则</button>
              </form>
            </>
          ) : (
            <EmptyState title="无编号规则治理权限" description="当前账号不能配置单据编号规则。" />
          )}
        </SectionBlock>
      </div>
    </PageShell>
  );
}
