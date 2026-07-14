import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { LotTrace, LotTraceEvent, QualityInspection, QualitySourceCandidate } from "../types/api";

const loadEmptyCandidates = () => Promise.resolve<QualitySourceCandidate[]>([]);
const loadEmptyInspections = () => Promise.resolve<QualityInspection[]>([]);
const loadEmptyLotTraceEvents = () => Promise.resolve<LotTraceEvent[]>([]);

function formatDate(value: string) {
  return new Intl.DateTimeFormat("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function sourceDocumentText(type: string) {
  switch (type) {
    case "InventoryReceipt":
      return "采购入库";
    case "ProductionReceipt":
      return "完工入库";
    case "InventoryIssue":
      return "销售出库";
    default:
      return type;
  }
}

function inspectionResultText(result: string) {
  switch (result) {
    case "Accepted":
      return "合格";
    case "Rejected":
      return "不合格";
    case "PartiallyAccepted":
      return "部分合格";
    default:
      return result;
  }
}

function traceEventText(type: string) {
  switch (type) {
    case "Incoming":
      return "来料";
    case "ProductionCompletion":
      return "生产完工";
    case "Shipment":
      return "发货";
    case "Inspection":
      return "质检";
    default:
      return type;
  }
}

/** 质量页面，处理来源单据检验、批次事件登记和批次追溯查询。 */
export function QualityPage() {
  const { hasPermission } = useAuth();
  const canReadQuality = hasPermission(platformPermissions.qualityRead);
  const canManageInspection = hasPermission(platformPermissions.qualityInspectionManage);
  const canManageTraceability = hasPermission(platformPermissions.qualityTraceabilityManage);

  const candidatesQuery = useAsyncData(canReadQuality ? api.listQualitySourceCandidates : loadEmptyCandidates);
  const inspectionsQuery = useAsyncData(canReadQuality ? api.listQualityInspections : loadEmptyInspections);
  const traceEventsQuery = useAsyncData(canReadQuality ? api.listLotTraceEvents : loadEmptyLotTraceEvents);

  const [inspectionForm, setInspectionForm] = useState({
    candidateKey: "",
    inspectedQuantity: 1,
    acceptedQuantity: 1,
    rejectedQuantity: 0,
    disposition: "放行",
    note: "",
  });
  const [traceForm, setTraceForm] = useState({
    lotNo: "",
    eventType: "",
    candidateKey: "",
    quantity: 1,
    targetDocumentType: "",
    targetDocumentNo: "",
    note: "",
  });
  const [traceQuery, setTraceQuery] = useState("");
  const [traceResult, setTraceResult] = useState<LotTrace | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const candidates = candidatesQuery.data ?? [];
  const inspections = inspectionsQuery.data ?? [];
  const traceEvents = traceEventsQuery.data ?? [];
  const rejectedCount = inspections.filter((entry) => entry.rejectedQuantity > 0).length;
  const lotCount = useMemo(() => new Set(traceEvents.map((entry) => entry.lotNo)).size, [traceEvents]);

  function candidateKey(candidate: QualitySourceCandidate) {
    return `${candidate.sourceDocumentType}|${candidate.sourceDocumentId}|${candidate.itemId}`;
  }

  function selectedInspectionCandidate() {
    return candidates.find((entry) => candidateKey(entry) === inspectionForm.candidateKey);
  }

  function selectedTraceCandidate() {
    return candidates.find((entry) => candidateKey(entry) === traceForm.candidateKey);
  }

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
    if (canReadQuality) {
      tasks.push(candidatesQuery.reload(), inspectionsQuery.reload(), traceEventsQuery.reload());
    }
    await Promise.all(tasks);
  }

  if (!canReadQuality && !canManageInspection && !canManageTraceability) {
    return (
      <PageShell title="质量追溯">
        <EmptyState title="无质量模块权限" description="当前账号不能查看或执行质量追溯业务。" />
      </PageShell>
    );
  }

  const inspectionCandidate = selectedInspectionCandidate();
  const traceCandidate = selectedTraceCandidate();

  return (
    <PageShell
      title="质量追溯"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "quality-refresh"}
          onClick={async () => {
            await runAction("quality-refresh", reloadAll, "质量追溯数据已刷新。");
          }}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      }
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <section className="stats-grid">
        <StatTile label="可检来源" value={candidates.length} tone={candidates.length > 0 ? "success" : "default"} />
        <StatTile label="质检记录" value={inspections.length} tone={inspections.length > 0 ? "success" : "default"} />
        <StatTile label="异常记录" value={rejectedCount} tone={rejectedCount > 0 ? "warning" : "success"} />
        <StatTile label="追溯批次" value={lotCount} tone={lotCount > 0 ? "success" : "default"} />
      </section>

      <div className="split-grid">
        <SectionBlock title="质检记录" hint="从真实采购入库、生产完工或销售出库来源创建质检记录。">
          {!canReadQuality ? (
            <EmptyState title="无质检查看权限" description="当前账号不能读取质检记录。" />
          ) : inspectionsQuery.loading ? (
            <div className="section-note">正在加载质检记录...</div>
          ) : inspectionsQuery.error ? (
            <div className="section-note error">{inspectionsQuery.error}</div>
          ) : inspections.length > 0 ? (
            <div className="table-shell">
              {inspections.map((inspection) => (
                <div key={inspection.id} className="review-card">
                  <div>
                    <strong>{inspection.inspectionNo} · {inspectionResultText(inspection.result)}</strong>
                    <p>{sourceDocumentText(inspection.sourceDocumentType)} {inspection.sourceDocumentNo} · {inspection.itemCode} · {inspection.itemName}</p>
                    <small>检验 {inspection.inspectedQuantity}，合格 {inspection.acceptedQuantity}，不合格 {inspection.rejectedQuantity}</small>
                    <small>处理：{inspection.disposition} · 检验人：{inspection.inspector} · {formatDate(inspection.inspectedAtUtc)}</small>
                    {inspection.note ? <small>{inspection.note}</small> : null}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无质检记录" description="从真实业务来源提交质检后，这里会形成历史。" />
          )}

          {canManageInspection ? (
            candidates.length === 0 ? (
              <EmptyState title="暂无可检来源" description="采购入库、生产完工或销售出库发生后，质量模块会自动显示可检来源。" />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  const selected = selectedInspectionCandidate();
                  if (!selected || inspectionForm.inspectedQuantity <= 0 || inspectionForm.acceptedQuantity < 0 || inspectionForm.rejectedQuantity < 0) {
                    setError("请选择来源并填写有效质检数量。");
                    return;
                  }

                  await runAction("inspection-create", async () => {
                    await api.createQualityInspection({
                      sourceDocumentType: selected.sourceDocumentType,
                      sourceDocumentId: selected.sourceDocumentId,
                      itemId: selected.itemId,
                      inspectedQuantity: inspectionForm.inspectedQuantity,
                      acceptedQuantity: inspectionForm.acceptedQuantity,
                      rejectedQuantity: inspectionForm.rejectedQuantity,
                      disposition: inspectionForm.disposition,
                      note: inspectionForm.note,
                    });
                    setInspectionForm({ candidateKey: "", inspectedQuantity: 1, acceptedQuantity: 1, rejectedQuantity: 0, disposition: "放行", note: "" });
                    await inspectionsQuery.reload();
                  }, "质检记录已创建。");
                }}
              >
                <select
                  value={inspectionForm.candidateKey}
                  onChange={(event) => {
                    const selected = candidates.find((entry) => candidateKey(entry) === event.target.value);
                    setInspectionForm({
                      ...inspectionForm,
                      candidateKey: event.target.value,
                      inspectedQuantity: selected?.quantity ?? 1,
                      acceptedQuantity: selected?.quantity ?? 1,
                      rejectedQuantity: 0,
                    });
                  }}
                >
                  <option value="">选择质检来源</option>
                  {candidates.map((candidate) => (
                    <option key={candidateKey(candidate)} value={candidateKey(candidate)}>
                      {sourceDocumentText(candidate.sourceDocumentType)} · {candidate.sourceDocumentNo} · {candidate.itemCode} · {candidate.itemName}
                    </option>
                  ))}
                </select>
                <div className="inline-form">
                  <input type="number" min={0.0001} max={inspectionCandidate?.quantity} step="0.0001" value={inspectionForm.inspectedQuantity} onChange={(event) => setInspectionForm({ ...inspectionForm, inspectedQuantity: Number(event.target.value) })} />
                  <input type="number" min={0} step="0.0001" value={inspectionForm.acceptedQuantity} onChange={(event) => setInspectionForm({ ...inspectionForm, acceptedQuantity: Number(event.target.value) })} />
                  <input type="number" min={0} step="0.0001" value={inspectionForm.rejectedQuantity} onChange={(event) => setInspectionForm({ ...inspectionForm, rejectedQuantity: Number(event.target.value) })} />
                </div>
                <input placeholder="处理结论" value={inspectionForm.disposition} onChange={(event) => setInspectionForm({ ...inspectionForm, disposition: event.target.value })} />
                <input placeholder="备注" value={inspectionForm.note} onChange={(event) => setInspectionForm({ ...inspectionForm, note: event.target.value })} />
                <button
                  type="submit"
                  disabled={
                    busyKey === "inspection-create" ||
                    !inspectionForm.candidateKey ||
                    inspectionForm.inspectedQuantity <= 0 ||
                    inspectionForm.acceptedQuantity + inspectionForm.rejectedQuantity !== inspectionForm.inspectedQuantity
                  }
                >
                  创建质检记录
                </button>
              </form>
            )
          ) : canReadQuality ? (
            <div className="section-note">当前账号只能查看质检记录，不能创建。</div>
          ) : null}
        </SectionBlock>

        <SectionBlock title="批次追溯" hint="从真实业务来源创建批次事件，并按批次号查询来源和去向。">
          {!canReadQuality ? (
            <EmptyState title="无追溯查看权限" description="当前账号不能读取批次追溯。" />
          ) : traceEventsQuery.loading ? (
            <div className="section-note">正在加载批次事件...</div>
          ) : traceEventsQuery.error ? (
            <div className="section-note error">{traceEventsQuery.error}</div>
          ) : traceEvents.length > 0 ? (
            <div className="inventory-record-list">
              {traceEvents.map((entry) => (
                <div key={entry.id} className="inventory-record-row">
                  <div>
                    <strong>{entry.lotNo} · {traceEventText(entry.eventType)}</strong>
                    <p>{sourceDocumentText(entry.sourceDocumentType)} {entry.sourceDocumentNo} · {entry.itemCode} · {entry.itemName}</p>
                    <small>数量 {entry.quantity} {entry.unit} · {entry.targetDocumentNo || "未记录去向"}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{entry.actor}</small>
                    <small>{formatDate(entry.occurredAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无批次事件" description="创建批次事件后，这里会形成真实追溯历史。" />
          )}

          {canManageTraceability ? (
            candidates.length === 0 ? (
              <EmptyState title="暂无可追溯来源" description="采购入库、生产完工或销售出库发生后，可以在这里建立批次事件。" />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  const selected = selectedTraceCandidate();
                  if (!selected || !traceForm.lotNo.trim() || traceForm.quantity <= 0) {
                    setError("请填写批次号、来源和有效数量。");
                    return;
                  }

                  await runAction("trace-create", async () => {
                    await api.createLotTraceEvent({
                      lotNo: traceForm.lotNo.trim(),
                      eventType: traceForm.eventType.trim(),
                      sourceDocumentType: selected.sourceDocumentType,
                      sourceDocumentId: selected.sourceDocumentId,
                      itemId: selected.itemId,
                      quantity: traceForm.quantity,
                      targetDocumentType: traceForm.targetDocumentType.trim(),
                      targetDocumentNo: traceForm.targetDocumentNo.trim(),
                      note: traceForm.note.trim(),
                    });
                    setTraceForm({ lotNo: "", eventType: "", candidateKey: "", quantity: 1, targetDocumentType: "", targetDocumentNo: "", note: "" });
                    await traceEventsQuery.reload();
                  }, "批次事件已创建。");
                }}
              >
                <input placeholder="批次号" value={traceForm.lotNo} onChange={(event) => setTraceForm({ ...traceForm, lotNo: event.target.value.toUpperCase() })} />
                <select
                  value={traceForm.candidateKey}
                  onChange={(event) => {
                    const selected = candidates.find((entry) => candidateKey(entry) === event.target.value);
                    setTraceForm({
                      ...traceForm,
                      candidateKey: event.target.value,
                      quantity: selected?.quantity ?? 1,
                    });
                  }}
                >
                  <option value="">选择追溯来源</option>
                  {candidates.map((candidate) => (
                    <option key={candidateKey(candidate)} value={candidateKey(candidate)}>
                      {sourceDocumentText(candidate.sourceDocumentType)} · {candidate.sourceDocumentNo} · {candidate.itemCode} · {candidate.itemName}
                    </option>
                  ))}
                </select>
                <div className="inline-form">
                  <select value={traceForm.eventType} onChange={(event) => setTraceForm({ ...traceForm, eventType: event.target.value })}>
                    <option value="">自动识别事件</option>
                    <option value="Incoming">来料</option>
                    <option value="ProductionCompletion">生产完工</option>
                    <option value="Shipment">发货</option>
                    <option value="Inspection">质检</option>
                  </select>
                  <input type="number" min={0.0001} max={traceCandidate?.quantity} step="0.0001" value={traceForm.quantity} onChange={(event) => setTraceForm({ ...traceForm, quantity: Number(event.target.value) })} />
                </div>
                <div className="inline-form">
                  <input placeholder="去向类型" value={traceForm.targetDocumentType} onChange={(event) => setTraceForm({ ...traceForm, targetDocumentType: event.target.value })} />
                  <input placeholder="去向单号" value={traceForm.targetDocumentNo} onChange={(event) => setTraceForm({ ...traceForm, targetDocumentNo: event.target.value })} />
                </div>
                <input placeholder="备注" value={traceForm.note} onChange={(event) => setTraceForm({ ...traceForm, note: event.target.value })} />
                <button type="submit" disabled={busyKey === "trace-create" || !traceForm.lotNo.trim() || !traceForm.candidateKey || traceForm.quantity <= 0}>
                  创建批次事件
                </button>
              </form>
            )
          ) : canReadQuality ? (
            <div className="section-note">当前账号只能查看批次追溯，不能创建事件。</div>
          ) : null}
        </SectionBlock>
      </div>

      <SectionBlock title="追溯查询" hint="输入批次号后，按时间顺序查看来源、质检、生产或发货事件。">
        {canReadQuality ? (
          <>
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                if (!traceQuery.trim()) {
                  setError("请输入批次号。");
                  return;
                }

                await runAction("trace-query", async () => {
                  const result = await api.getLotTrace(traceQuery.trim());
                  setTraceResult(result);
                });
              }}
            >
              <div className="inline-form">
                <input placeholder="批次号" value={traceQuery} onChange={(event) => setTraceQuery(event.target.value.toUpperCase())} />
                <button type="submit" disabled={busyKey === "trace-query" || !traceQuery.trim()}>查询追溯链</button>
              </div>
            </form>

            {traceResult ? (
              traceResult.events.length > 0 ? (
                <div className="inventory-record-list">
                  {traceResult.events.map((entry) => (
                    <div key={entry.id} className="inventory-record-row">
                      <div>
                        <strong>{traceEventText(entry.eventType)} · {entry.sourceDocumentNo}</strong>
                        <p>{sourceDocumentText(entry.sourceDocumentType)} · {entry.itemCode} · {entry.itemName}</p>
                        <small>去向：{entry.targetDocumentNo || "未记录"} · 数量 {entry.quantity} {entry.unit}</small>
                      </div>
                      <div className="inventory-record-meta">
                        <small>{entry.actor}</small>
                        <small>{formatDate(entry.occurredAtUtc)}</small>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState title="未找到批次事件" description={`${traceResult.lotNo} 暂无追溯记录。`} />
              )
            ) : (
              <EmptyState title="等待查询" description="输入批次号后展示追溯链。" />
            )}
          </>
        ) : (
          <EmptyState title="无追溯查询权限" description="当前账号不能查询批次链路。" />
        )}
      </SectionBlock>
    </PageShell>
  );
}
