import { Link } from "react-router-dom";
import {
  BriefcaseBusiness,
  Building2,
  Download,
  Eye,
  Image,
  MessageSquare,
  Network,
  Paperclip,
  RefreshCcw,
  Send,
  UsersRound,
  X,
} from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import { moduleRoutes } from "../modules/moduleNavigation";
import type { CollaborationAttachment, CollaborationConversation, CollaborationMessage, PositionPermissionOverview, UserSummary } from "../types/api";

const loadEmptyUsers = () => Promise.resolve<UserSummary[]>([]);
const loadEmptyConversations = () => Promise.resolve<CollaborationConversation[]>([]);
const loadEmptyMessages = () => Promise.resolve<CollaborationMessage[]>([]);

const emptyPositionOverview: PositionPermissionOverview = {
  departments: [],
  positions: [],
  roles: [],
  permissionPackages: [],
  roleBindings: [],
  dataScopeRules: [],
  permissions: [],
  modules: [],
};

const loadEmptyPositionOverview = () => Promise.resolve(emptyPositionOverview);

function formatMessageTime(value: string) {
  return new Intl.DateTimeFormat("zh-CN", {
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatFileSize(value: number) {
  if (value < 1024) {
    return `${value} B`;
  }

  if (value < 1024 * 1024) {
    return `${(value / 1024).toFixed(1)} KB`;
  }

  return `${(value / 1024 / 1024).toFixed(1)} MB`;
}

function readFileAsBase64(file: File) {
  return new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      const result = String(reader.result ?? "");
      resolve(result.includes(",") ? result.split(",")[1] : result);
    };
    reader.onerror = () => reject(new Error("读取附件失败。"));
    reader.readAsDataURL(file);
  });
}

type AttachmentDraft = {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  contentBase64: string;
  isImage: boolean;
};

/** 组织协同页面，集中展示企业组织、部门联系和人员联系的真实可用数据。 */
export function OrganizationCollaborationPage() {
  const { user, hasPermission } = useAuth();
  const canReadUsers = hasPermission(platformPermissions.identityUserRead);
  const canReadPositions = hasPermission(platformPermissions.positionPermissionsRead);
  const canReadCollaboration = hasPermission(platformPermissions.organizationCollaborationRead);
  const canSendCollaboration = hasPermission(platformPermissions.organizationCollaborationMessage);
  const canOpenPeopleModule = user?.visibleModuleKeys.includes("people-management") ?? false;
  const canOpenPositionModule = user?.visibleModuleKeys.includes("position-permissions") ?? false;

  const organizationsQuery = useAsyncData(api.listOrganizations);
  const usersQuery = useAsyncData(canReadUsers ? api.listUsers : loadEmptyUsers, canReadUsers ? "users" : "no-users");
  const conversationsQuery = useAsyncData(
    canReadCollaboration ? api.listCollaborationConversations : loadEmptyConversations,
    canReadCollaboration ? "collaboration-conversations" : "no-collaboration-conversations",
  );
  const [activeConversation, setActiveConversation] = useState<CollaborationConversation | null>(null);
  const activeConversationId = activeConversation?.id ?? null;
  const lastMarkedReadRef = useRef<string | null>(null);
  const messagesQuery = useAsyncData(
    activeConversationId && canReadCollaboration
      ? () => api.listCollaborationMessages(activeConversationId)
      : loadEmptyMessages,
    activeConversationId ?? "no-active-conversation",
  );
  const positionQuery = useAsyncData(
    canReadPositions ? api.getPositionPermissionOverview : loadEmptyPositionOverview,
    canReadPositions ? "position-overview" : "no-position-overview",
  );
  const [messageDraft, setMessageDraft] = useState("");
  const [attachmentDrafts, setAttachmentDrafts] = useState<AttachmentDraft[]>([]);
  const [messageNotice, setMessageNotice] = useState<string | null>(null);
  const [messageError, setMessageError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const organizations = organizationsQuery.data ?? [];
  const users = usersQuery.data ?? [];
  const conversations = conversationsQuery.data ?? [];
  const messages = messagesQuery.data ?? [];
  const lastMessageId = messages.at(-1)?.id ?? null;
  const positionOverview = positionQuery.data ?? emptyPositionOverview;

  const positionsByDepartment = useMemo(() => {
    const groups = new Map<string, typeof positionOverview.positions>();
    for (const position of positionOverview.positions) {
      const current = groups.get(position.departmentId) ?? [];
      groups.set(position.departmentId, [...current, position]);
    }

    return groups;
  }, [positionOverview.positions]);

  useEffect(() => {
    if (!canReadCollaboration) {
      return undefined;
    }

    return api.subscribeCollaborationEvents((event) => {
      if (event.eventKey !== "changed") {
        return;
      }

      void conversationsQuery.reload();
      if (activeConversationId) {
        void messagesQuery.reload();
      }
    }, (error) => {
      setMessageError(error.message);
    });
  }, [activeConversationId, canReadCollaboration, conversationsQuery.reload, messagesQuery.reload]);

  useEffect(() => {
    if (!activeConversationId || !canReadCollaboration || !lastMessageId) {
      return;
    }

    const markKey = `${activeConversationId}:${lastMessageId}`;
    if (lastMarkedReadRef.current === markKey) {
      return;
    }

    lastMarkedReadRef.current = markKey;
    void api.markCollaborationConversationRead(activeConversationId, lastMessageId)
      .then((conversation) => {
        setActiveConversation(conversation);
        void conversationsQuery.reload();
      })
      .catch((error) => {
        setMessageError(error instanceof Error ? error.message : "标记已读失败。");
      });
  }, [activeConversationId, canReadCollaboration, conversationsQuery.reload, lastMessageId]);

  async function reloadAll() {
    await Promise.all([
      organizationsQuery.reload(),
      usersQuery.reload(),
      positionQuery.reload(),
      conversationsQuery.reload(),
      messagesQuery.reload(),
    ]);
  }

  async function openDirectConversation(contact: UserSummary) {
    setBusyKey(`conversation-${contact.id}`);
    setMessageNotice(null);
    setMessageError(null);
    try {
      const conversation = await api.ensureDirectCollaborationConversation(contact.id);
      setActiveConversation(conversation);
      lastMarkedReadRef.current = null;
      await conversationsQuery.reload();
      setMessageNotice(`已打开与 ${contact.displayName} 的会话。`);
    } catch (err) {
      setMessageError(err instanceof Error ? err.message : "打开会话失败。");
    } finally {
      setBusyKey(null);
    }
  }

  async function addAttachments(files: FileList | null) {
    if (!files || files.length === 0) {
      return;
    }

    const nextFiles = Array.from(files);
    const nextCount = attachmentDrafts.length + nextFiles.length;
    if (nextCount > 5) {
      setMessageError("单条消息最多选择 5 个附件。");
      return;
    }

    const currentTotal = attachmentDrafts.reduce((sum, item) => sum + item.sizeBytes, 0);
    const nextTotal = nextFiles.reduce((sum, item) => sum + item.size, currentTotal);
    if (nextFiles.some((file) => file.size > 2 * 1024 * 1024)) {
      setMessageError("单个附件不能超过 2 MB。");
      return;
    }

    if (nextTotal > 8 * 1024 * 1024) {
      setMessageError("单条消息附件总大小不能超过 8 MB。");
      return;
    }

    setBusyKey("attach-files");
    setMessageError(null);
    try {
      const drafts = await Promise.all(nextFiles.map(async (file) => ({
        id: `${file.name}-${file.size}-${file.lastModified}-${crypto.randomUUID()}`,
        fileName: file.name,
        contentType: file.type || "application/octet-stream",
        sizeBytes: file.size,
        contentBase64: await readFileAsBase64(file),
        isImage: file.type.startsWith("image/"),
      })));
      setAttachmentDrafts((current) => [...current, ...drafts]);
    } catch (err) {
      setMessageError(err instanceof Error ? err.message : "读取附件失败。");
    } finally {
      setBusyKey(null);
    }
  }

  function removeAttachment(id: string) {
    setAttachmentDrafts((current) => current.filter((item) => item.id !== id));
  }

  async function openAttachment(attachment: CollaborationAttachment, mode: "preview" | "download") {
    setBusyKey(`attachment-${attachment.id}-${mode}`);
    setMessageError(null);
    try {
      const blob = await api.downloadCollaborationAttachment(attachment.downloadUrl);
      const url = URL.createObjectURL(blob);
      if (mode === "preview") {
        window.open(url, "_blank", "noopener,noreferrer");
        window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
        return;
      }

      const link = document.createElement("a");
      link.href = url;
      link.download = attachment.fileName;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    } catch (err) {
      setMessageError(err instanceof Error ? err.message : "附件处理失败。");
    } finally {
      setBusyKey(null);
    }
  }

  async function sendMessage() {
    if (!activeConversationId) {
      setMessageError("请先选择会话。");
      return;
    }

    const content = messageDraft.trim();
    if (!content && attachmentDrafts.length === 0) {
      setMessageError("消息内容或附件不能同时为空。");
      return;
    }

    setBusyKey("send-message");
    setMessageNotice(null);
    setMessageError(null);
    try {
      await api.sendCollaborationMessage(activeConversationId, content, attachmentDrafts.map((attachment) => ({
        fileName: attachment.fileName,
        contentType: attachment.contentType,
        contentBase64: attachment.contentBase64,
      })));
      setMessageDraft("");
      setAttachmentDrafts([]);
      await Promise.all([messagesQuery.reload(), conversationsQuery.reload()]);
      setMessageNotice("消息已发送。");
    } catch (err) {
      setMessageError(err instanceof Error ? err.message : "发送消息失败。");
    } finally {
      setBusyKey(null);
    }
  }

  return (
    <PageShell
      title="组织协同"
      actions={(
        <div className="button-row wrap">
          {canOpenPeopleModule ? (
            <Link className="button-link" to={moduleRoutes["people-management"]}>
              <UsersRound size={16} />
              <span>人员管理</span>
            </Link>
          ) : null}
          {canOpenPositionModule ? (
            <Link className="button-link" to={moduleRoutes["position-permissions"]}>
              <BriefcaseBusiness size={16} />
              <span>岗位权限</span>
            </Link>
          ) : null}
          <button type="button" className="secondary icon-button" onClick={reloadAll}>
            <RefreshCcw size={16} />
            <span>刷新数据</span>
          </button>
        </div>
      )}
    >
      {messageNotice ? <div className="form-message success">{messageNotice}</div> : null}
      {messageError ? <div className="form-message error">{messageError}</div> : null}

      <section className="stats-grid org-summary-grid">
        <StatTile label="企业组织" value={organizations.length} tone={organizations.length > 0 ? "success" : "warning"} />
        <StatTile label="部门" value={positionOverview.departments.length} tone={positionOverview.departments.length > 0 ? "success" : "warning"} />
        <StatTile label="岗位" value={positionOverview.positions.length} tone={positionOverview.positions.length > 0 ? "success" : "warning"} />
        <StatTile label="会话数量" value={conversations.length} tone={conversations.length > 0 ? "success" : "warning"} />
      </section>

      <section className="org-workspace-grid">
        <SectionBlock title="组织架构" hint="从企业组织到部门岗位，先保证联系人和问题归属可追溯。">
          {organizationsQuery.loading || positionQuery.loading ? (
            <div className="section-note">正在加载组织架构...</div>
          ) : organizationsQuery.error || positionQuery.error ? (
            <div className="section-note error">{organizationsQuery.error ?? positionQuery.error}</div>
          ) : organizations.length > 0 || positionOverview.departments.length > 0 ? (
            <div className="org-structure-map">
              {organizations.map((organization) => (
                <div key={organization.id} className="org-structure-card">
                  <div className="org-structure-card-head">
                    <Building2 size={18} />
                    <div>
                      <strong>{organization.name}</strong>
                      <small>{organization.regionCode} · 默认角色 {organization.defaultRole}</small>
                    </div>
                  </div>
                </div>
              ))}

              {positionOverview.departments.map((department) => {
                const positions = positionsByDepartment.get(department.id) ?? [];
                return (
                  <div key={department.id} className="org-structure-card department">
                    <div className="org-structure-card-head">
                      <Network size={18} />
                      <div>
                        <strong>{department.name}</strong>
                        <small>{department.code} · {department.isEnabled ? "启用" : "停用"}</small>
                      </div>
                    </div>
                    <div className="compact-tag-list">
                      {positions.length > 0 ? positions.map((position) => (
                        <span key={position.id} className="compact-tag">{position.name}</span>
                      )) : (
                        <span className="compact-tag">未配置岗位</span>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无组织架构" description="请先建立企业组织、部门和岗位。" />
          )}
        </SectionBlock>

        <SectionBlock title="会话列表" hint="会话和消息均由后端持久化，服务端事件流会触发当前页面刷新。">
          {!canReadCollaboration ? (
            <EmptyState title="缺少协同读取权限" description="当前账号无法读取协同会话。" />
          ) : conversationsQuery.loading ? (
            <div className="section-note">正在加载会话...</div>
          ) : conversationsQuery.error ? (
            <div className="section-note error">{conversationsQuery.error}</div>
          ) : conversations.length > 0 ? (
            <div className="conversation-list">
              {conversations.map((conversation) => {
                const otherParticipants = conversation.participants.filter((participant) => participant.userId !== user?.id);
                return (
                  <button
                    type="button"
                    key={conversation.id}
                    className={`conversation-row${conversation.id === activeConversationId ? " active" : ""}`}
                    onClick={() => {
                      setActiveConversation(conversation);
                      lastMarkedReadRef.current = null;
                    }}
                  >
                    <MessageSquare size={17} />
                    <span>
                      <strong>{otherParticipants.map((participant) => participant.displayName).join(" / ") || conversation.title}</strong>
                      <small>{conversation.lastMessagePreview || "暂无消息"}</small>
                    </span>
                    {conversation.unreadCount > 0 ? <em className="conversation-unread">{conversation.unreadCount}</em> : null}
                  </button>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无会话" description="在个人联系人中打开会话后，可以发送文本消息。" />
          )}
        </SectionBlock>
      </section>

      <section className="org-contact-grid">
        <SectionBlock title="部门联系" hint="部门联系优先按岗位承接，后续由员工档案绑定部门负责人。">
          {!canReadPositions ? (
            <EmptyState title="缺少部门读取权限" description="当前账号无法读取部门和岗位信息。" />
          ) : positionOverview.departments.length > 0 ? (
            <div className="people-card-list compact-list">
              {positionOverview.departments.map((department) => {
                const positions = positionsByDepartment.get(department.id) ?? [];
                return (
                  <div key={department.id} className="people-card-row">
                    <span className="people-avatar"><BriefcaseBusiness size={17} /></span>
                    <div>
                      <strong>{department.name}</strong>
                      <p>{department.code}</p>
                      <small>{positions.length} 个岗位 · {department.isEnabled ? "启用" : "停用"}</small>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无部门" description="请先在岗位权限模块建立部门。" />
          )}
        </SectionBlock>

        <SectionBlock title="个人联系" hint="个人联系人来自真实员工账号，角色反映可协同的业务范围。">
          {!canReadUsers ? (
            <EmptyState title="缺少人员读取权限" description="当前账号无法读取人员联系人。" />
          ) : usersQuery.loading ? (
            <div className="section-note">正在加载人员联系人...</div>
          ) : usersQuery.error ? (
            <div className="section-note error">{usersQuery.error}</div>
          ) : users.length > 0 ? (
            <div className="people-card-list compact-list">
              {users.map((contact) => (
                <div key={contact.id} className="people-card-row">
                  <span className="people-avatar"><UsersRound size={17} /></span>
                  <div>
                    <strong>{contact.displayName}</strong>
                    <p>{contact.userName}</p>
                    <small>{contact.roles.map((role) => role.displayName).join(" / ") || "未分配角色"}</small>
                  </div>
                  <span className={contact.isEnabled ? "status-pill success" : "status-pill warning"}>
                    {contact.isEnabled ? "可联系" : "停用"}
                  </span>
                  {canSendCollaboration && contact.id !== user?.id && contact.isEnabled ? (
                    <button
                      type="button"
                      className="secondary icon-button"
                      disabled={busyKey === `conversation-${contact.id}`}
                      onClick={() => void openDirectConversation(contact)}
                    >
                      <MessageSquare size={16} />
                      <span>打开会话</span>
                    </button>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无人员联系人" description="请先在人员管理中创建员工账号。" />
          )}
        </SectionBlock>
      </section>

      <SectionBlock title="消息线程" hint="个人直接会话支持文本、文件、照片、已读状态和服务端事件流刷新。">
        {!activeConversation ? (
          <EmptyState title="请选择会话" description="从个人联系人或会话列表打开一个会话。" />
        ) : (
          <div className="collaboration-chat-panel">
            <div className="message-thread">
              {messagesQuery.loading ? <div className="section-note">正在加载消息...</div> : null}
              {messagesQuery.error ? <div className="section-note error">{messagesQuery.error}</div> : null}
              {messages.length > 0 ? messages.map((item) => (
                <div key={item.id} className={`message-bubble${item.senderUserId === user?.id ? " mine" : ""}`}>
                  <strong>{item.senderDisplayName}</strong>
                  {item.content ? <p>{item.content}</p> : null}
                  {item.attachments.length > 0 ? (
                    <div className="message-attachment-list">
                      {item.attachments.map((attachment) => (
                        <div key={attachment.id} className="message-attachment">
                          <span className="message-attachment-icon">
                            {attachment.isImage ? <Image size={16} /> : <Paperclip size={16} />}
                          </span>
                          <span>
                            <strong>{attachment.fileName}</strong>
                            <small>{attachment.contentType} · {formatFileSize(attachment.sizeBytes)}</small>
                          </span>
                          {attachment.isImage ? (
                            <button
                              type="button"
                              className="secondary icon-only-button"
                              title="预览图片"
                              disabled={busyKey === `attachment-${attachment.id}-preview`}
                              onClick={() => void openAttachment(attachment, "preview")}
                            >
                              <Eye size={15} />
                            </button>
                          ) : null}
                          <button
                            type="button"
                            className="secondary icon-only-button"
                            title="下载附件"
                            disabled={busyKey === `attachment-${attachment.id}-download`}
                            onClick={() => void openAttachment(attachment, "download")}
                          >
                            <Download size={15} />
                          </button>
                        </div>
                      ))}
                    </div>
                  ) : null}
                  <small>{formatMessageTime(item.createdAtUtc)}</small>
                </div>
              )) : (
                <EmptyState title="暂无消息" description="发送第一条消息后，会在这里显示历史记录。" />
              )}
            </div>
            {canSendCollaboration ? (
              <form
                className="message-composer"
                onSubmit={(event) => {
                  event.preventDefault();
                  void sendMessage();
                }}
              >
                <textarea
                  rows={3}
                  value={messageDraft}
                  onChange={(event) => setMessageDraft(event.target.value)}
                  placeholder="输入消息内容"
                  maxLength={2000}
                />
                {attachmentDrafts.length > 0 ? (
                  <div className="message-composer-attachments">
                    {attachmentDrafts.map((attachment) => (
                      <span key={attachment.id} className="message-composer-attachment">
                        {attachment.isImage ? <Image size={15} /> : <Paperclip size={15} />}
                        <span>{attachment.fileName} · {formatFileSize(attachment.sizeBytes)}</span>
                        <button type="button" className="icon-only-button secondary" title="移除附件" onClick={() => removeAttachment(attachment.id)}>
                          <X size={14} />
                        </button>
                      </span>
                    ))}
                  </div>
                ) : null}
                <div className="message-composer-actions">
                  <label className={`button-link secondary${busyKey === "attach-files" ? " disabled" : ""}`}>
                    <Paperclip size={16} />
                    <span>添加附件</span>
                    <input
                      type="file"
                      multiple
                      hidden
                      disabled={busyKey === "attach-files"}
                      onChange={(event) => {
                        void addAttachments(event.target.files);
                        event.target.value = "";
                      }}
                    />
                  </label>
                  <button type="submit" disabled={busyKey === "send-message" || (!messageDraft.trim() && attachmentDrafts.length === 0)}>
                    <Send size={16} />
                    <span>发送消息</span>
                  </button>
                </div>
              </form>
            ) : (
              <div className="section-note">当前账号没有发送协同消息的权限。</div>
            )}
          </div>
        )}
      </SectionBlock>
    </PageShell>
  );
}
