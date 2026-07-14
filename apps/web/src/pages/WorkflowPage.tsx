import { CheckCircle2, RefreshCcw, XCircle } from "lucide-react";
import { useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { ApprovalTask, WorkflowDefinition, WorkflowInstance, WorkflowNotification } from "../types/api";

const loadEmptyDefinitions = () => Promise.resolve<WorkflowDefinition[]>([]);
const loadEmptyInstances = () => Promise.resolve<WorkflowInstance[]>([]);
const loadEmptyTasks = () => Promise.resolve<ApprovalTask[]>([]);
const loadEmptyNotifications = () => Promise.resolve<WorkflowNotification[]>([]);

function formatDate(value: string) {
  return new Intl.DateTimeFormat("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function taskStatusText(status: string) {
  switch (status) {
    case "Pending":
      return "待处理";
    case "Completed":
      return "已处理";
    default:
      return status;
  }
}

function workflowStatusText(status: string) {
  switch (status) {
    case "Pending":
      return "进行中";
    case "Approved":
      return "已通过";
    case "Rejected":
      return "已驳回";
    default:
      return status;
  }
}

function decisionText(decision?: string | null) {
  switch (decision) {
    case "Approved":
      return "通过";
    case "Rejected":
      return "驳回";
    default:
      return "未处理";
  }
}

/** 审批中心页面，展示流程定义、实例、待办审批和通知阅读状态。 */
export function WorkflowPage() {
  const { hasPermission } = useAuth();
  const canReadWorkflow = hasPermission(platformPermissions.workflowRead);
  const canDecideTasks = hasPermission(platformPermissions.workflowTaskDecide);
  const canReadNotifications = hasPermission(platformPermissions.notificationRead);

  const definitionsQuery = useAsyncData(canReadWorkflow ? api.listWorkflowDefinitions : loadEmptyDefinitions);
  const instancesQuery = useAsyncData(canReadWorkflow ? api.listWorkflowInstances : loadEmptyInstances);
  const tasksQuery = useAsyncData(canReadWorkflow ? api.listApprovalTasks : loadEmptyTasks);
  const notificationsQuery = useAsyncData(canReadNotifications ? api.listWorkflowNotifications : loadEmptyNotifications);

  const [comments, setComments] = useState<Record<string, string>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const definitions = definitionsQuery.data ?? [];
  const instances = instancesQuery.data ?? [];
  const tasks = tasksQuery.data ?? [];
  const notifications = notificationsQuery.data ?? [];
  const pendingTasks = tasks.filter((entry) => entry.status === "Pending");
  const unreadNotifications = notifications.filter((entry) => entry.status === "Unread");

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
    const tasksToRun: Promise<unknown>[] = [];
    if (canReadWorkflow) {
      tasksToRun.push(definitionsQuery.reload(), instancesQuery.reload(), tasksQuery.reload());
    }
    if (canReadNotifications) {
      tasksToRun.push(notificationsQuery.reload());
    }
    await Promise.all(tasksToRun);
  }

  async function decideTask(task: ApprovalTask, decision: "Approved" | "Rejected") {
    await runAction(`task-${task.id}`, async () => {
      await api.decideApprovalTask(task.id, decision, comments[task.id] ?? "");
      setComments((current) => {
        const next = { ...current };
        delete next[task.id];
        return next;
      });
      await reloadAll();
    }, `${task.documentNo} 已${decision === "Approved" ? "审批通过" : "审批驳回"}。`);
  }

  if (!canReadWorkflow && !canReadNotifications) {
    return (
      <PageShell title="审批中心">
        <EmptyState title="无审批中心权限" description="当前账号不能查看审批待办或通知。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="审批中心"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "workflow-refresh"}
          onClick={async () => {
            await runAction("workflow-refresh", reloadAll, "审批中心数据已刷新。");
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
        <StatTile label="流程定义" value={definitions.length} tone={definitions.length > 0 ? "success" : "default"} />
        <StatTile label="审批实例" value={instances.length} tone={instances.length > 0 ? "success" : "default"} />
        <StatTile label="待办任务" value={pendingTasks.length} tone={pendingTasks.length > 0 ? "warning" : "success"} />
        <StatTile label="未读通知" value={unreadNotifications.length} tone={unreadNotifications.length > 0 ? "warning" : "success"} />
      </section>

      <div className="split-grid">
        <SectionBlock title="待办审批" hint="当前最小接入场景为采购申请审批，处理后会同步更新采购申请状态。">
          {!canReadWorkflow ? (
            <EmptyState title="无待办查看权限" description="当前账号不能读取审批任务。" />
          ) : tasksQuery.loading ? (
            <div className="section-note">正在加载审批任务...</div>
          ) : tasksQuery.error ? (
            <div className="section-note error">{tasksQuery.error}</div>
          ) : tasks.length > 0 ? (
            <div className="table-shell">
              {tasks.map((task) => (
                <div key={task.id} className="review-card">
                  <div>
                    <strong>{task.documentNo} · {task.title}</strong>
                    <p>{task.definitionName} · 提交人：{task.submittedBy}</p>
                    <small>{taskStatusText(task.status)} · {formatDate(task.createdAtUtc)}</small>
                    {task.status !== "Pending" ? (
                      <small>处理结果：{decisionText(task.decision)} · {task.decidedBy ?? "未知"}{task.decidedAtUtc ? ` · ${formatDate(task.decidedAtUtc)}` : ""}</small>
                    ) : null}
                  </div>
                  <div className="inventory-actions">
                    {task.status === "Pending" && canDecideTasks ? (
                      <>
                        <input
                          placeholder="审批意见"
                          value={comments[task.id] ?? ""}
                          onChange={(event) => setComments({ ...comments, [task.id]: event.target.value })}
                        />
                        <button
                          className="icon-button"
                          disabled={busyKey === `task-${task.id}`}
                          onClick={async () => decideTask(task, "Approved")}
                        >
                          <CheckCircle2 size={16} />
                          <span>审批通过</span>
                        </button>
                        <button
                          className="secondary icon-button"
                          disabled={busyKey === `task-${task.id}`}
                          onClick={async () => decideTask(task, "Rejected")}
                        >
                          <XCircle size={16} />
                          <span>审批驳回</span>
                        </button>
                      </>
                    ) : (
                      <small>{task.status === "Pending" ? "当前账号不能处理该审批任务。" : "该任务已处理。"}</small>
                    )}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无审批任务" description="采购申请提交后，会在这里形成待办。" />
          )}
        </SectionBlock>

        <SectionBlock title="通知消息" hint="审批待办与处理结果会形成通知，可标记已读或未读。">
          {!canReadNotifications ? (
            <EmptyState title="无通知读取权限" description="当前账号不能查看工作流通知。" />
          ) : notificationsQuery.loading ? (
            <div className="section-note">正在加载通知...</div>
          ) : notificationsQuery.error ? (
            <div className="section-note error">{notificationsQuery.error}</div>
          ) : notifications.length > 0 ? (
            <div className="table-shell">
              {notifications.map((entry) => (
                <div key={entry.id} className="review-card">
                  <div>
                    <strong>{entry.title}</strong>
                    <p>{entry.message}</p>
                    <small>{entry.relatedDocumentNo} · {entry.status === "Unread" ? "未读" : "已读"} · {formatDate(entry.createdAtUtc)}</small>
                  </div>
                  <div className="button-row">
                    <button
                      className="secondary"
                      disabled={busyKey === `notification-${entry.id}`}
                      onClick={async () => {
                        await runAction(`notification-${entry.id}`, async () => {
                          await api.markWorkflowNotification(entry.id, entry.status === "Unread");
                          await notificationsQuery.reload();
                        }, entry.status === "Unread" ? "通知已标记为已读。" : "通知已标记为未读。");
                      }}
                    >
                      {entry.status === "Unread" ? "标为已读" : "标为未读"}
                    </button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无通知" description="工作流产生待办或处理结果后会生成通知。" />
          )}
        </SectionBlock>
      </div>

      <SectionBlock title="流程实例" hint="这里展示统一工作流运行历史，便于追踪业务单据处理过程。">
        {!canReadWorkflow ? (
          <EmptyState title="无流程历史权限" description="当前账号不能查看流程实例。" />
        ) : instances.length > 0 ? (
          <div className="inventory-record-list">
            {instances.map((entry) => (
              <div key={entry.id} className="inventory-record-row">
                <div>
                  <strong>{entry.documentNo} · {entry.title}</strong>
                  <p>{entry.definitionName} · {entry.submittedBy}</p>
                </div>
                <div className="inventory-record-meta">
                  <small>{workflowStatusText(entry.status)}</small>
                  <small>{formatDate(entry.submittedAtUtc)}</small>
                  {entry.completedAtUtc ? <small>{formatDate(entry.completedAtUtc)}</small> : null}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState title="暂无流程实例" description="业务单据提交审批后，这里会显示流程历史。" />
        )}
      </SectionBlock>
    </PageShell>
  );
}
