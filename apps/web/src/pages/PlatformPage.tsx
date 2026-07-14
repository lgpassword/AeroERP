import { AnimatePresence, motion } from "framer-motion";
import { Eye, EyeOff, KeyRound, LockKeyhole, RefreshCcw, ShieldAlert, UserCog } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { ModuleVisibility, RoleSummary, UserSummary } from "../types/api";

const loadEmptyModules = () => Promise.resolve<ModuleVisibility[]>([]);
const loadEmptyRoles = () => Promise.resolve<RoleSummary[]>([]);
const loadEmptyUsers = () => Promise.resolve<UserSummary[]>([]);

function reviewStatusText(status: string) {
  switch (status) {
    case "Pending":
      return "待审查";
    case "Approved":
      return "已通过";
    case "Rejected":
      return "已驳回";
    default:
      return status;
  }
}

function roleDisplayText(value: string) {
  switch (value) {
    case "platform-admin":
      return "平台管理员";
    case "operations-manager":
      return "运营经理";
    case "purchaser":
      return "采购专员";
    default:
      return value;
  }
}

/** 平台治理页面，集中处理模块可见性、组织、智能代理审查、用户和角色授权。 */
export function PlatformPage() {
  const { user: currentUser, hasPermission, refresh } = useAuth();
  const canManageOrganizations = hasPermission(platformPermissions.organizationManage);
  const canReadUsers = hasPermission(platformPermissions.identityUserRead);
  const canManageUsers = hasPermission(platformPermissions.identityUserManage);
  const canManagePasswords = hasPermission(platformPermissions.identityUserPasswordManage);
  const canManageRoles = hasPermission(platformPermissions.identityRoleManage);
  const canManagePlugins = hasPermission(platformPermissions.pluginManage);
  const canSubmitReview = hasPermission(platformPermissions.agentReviewSubmit);
  const canDecideReview = hasPermission(platformPermissions.agentReviewDecide);
  const canInspectModules = canManagePlugins || canManageRoles;

  const modulesQuery = useAsyncData(canInspectModules ? api.listModules : loadEmptyModules);
  const organizationsQuery = useAsyncData(api.listOrganizations);
  const reviewsQuery = useAsyncData(api.listAgentReviews);
  const rolesQuery = useAsyncData(canManageRoles || canManageUsers ? api.listRoles : loadEmptyRoles);
  const usersQuery = useAsyncData(canReadUsers ? api.listUsers : loadEmptyUsers);

  const [orgForm, setOrgForm] = useState({ name: "", defaultRole: "平台管理员", regionCode: "CN-SH" });
  const [reviewForm, setReviewForm] = useState({ agentName: "", actionName: "", payload: "" });
  const [userForm, setUserForm] = useState({ userName: "", displayName: "", password: "", roleIds: [] as string[] });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [changePasswordOpen, setChangePasswordOpen] = useState(false);
  const [changePasswordForm, setChangePasswordForm] = useState({ currentPassword: "", newPassword: "" });
  const [resetPasswordUserId, setResetPasswordUserId] = useState<string | null>(null);
  const [resetPasswordForm, setResetPasswordForm] = useState({ newPassword: "" });

  const stats = useMemo(() => {
    const modules = modulesQuery.data ?? [];
    return {
      visible: modules.filter((x) => x.isVisible).length,
      hidden: modules.filter((x) => !x.isVisible).length,
      pending: (reviewsQuery.data ?? []).filter((x) => x.status === "Pending").length,
      orgs: organizationsQuery.data?.length ?? 0,
    };
  }, [modulesQuery.data, organizationsQuery.data, reviewsQuery.data]);

  async function runAction(actionKey: string, action: () => Promise<void>, successText?: string) {
    setBusyKey(actionKey);
    setError(null);
    setMessage(null);
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

  return (
    <PageShell
      title="平台治理"
      actions={(
        <button className="secondary icon-button" onClick={() => setChangePasswordOpen((value) => !value)}>
          <LockKeyhole size={16} />
          <span>{changePasswordOpen ? "收起改密" : "修改我的密码"}</span>
        </button>
      )}
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <AnimatePresence initial={false}>
        {changePasswordOpen ? (
          <motion.section
            key="change-password"
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            transition={{ duration: 0.2 }}
          >
            <SectionBlock title="当前账号密码" hint="密码修改后立即生效，旧密码会失效。">
              <form
                className="inline-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  await runAction("change-password", async () => {
                    await api.changePassword(changePasswordForm);
                    setChangePasswordForm({ currentPassword: "", newPassword: "" });
                    setChangePasswordOpen(false);
                    await refresh();
                  }, "当前账号密码已更新。");
                }}
              >
                <input
                  type="password"
                  placeholder="当前密码"
                  value={changePasswordForm.currentPassword}
                  onChange={(e) => setChangePasswordForm({ ...changePasswordForm, currentPassword: e.target.value })}
                />
                <input
                  type="password"
                  placeholder="新密码（至少 8 位）"
                  value={changePasswordForm.newPassword}
                  onChange={(e) => setChangePasswordForm({ ...changePasswordForm, newPassword: e.target.value })}
                />
                <button type="submit" disabled={busyKey === "change-password"}>提交修改</button>
              </form>
            </SectionBlock>
          </motion.section>
        ) : null}
      </AnimatePresence>

      <section className="stats-grid">
        <StatTile label="显示模块" value={stats.visible} />
        <StatTile label="隐藏模块" value={stats.hidden} tone="warning" />
        <StatTile label="待审请求" value={stats.pending} tone="warning" />
        <StatTile label="组织数量" value={stats.orgs} tone="success" />
      </section>

      <SectionBlock title="插件中心" hint="模块显隐会即时影响导航与入口权限。">
        {canManagePlugins ? (
          <div className="table-shell">
            {(modulesQuery.data ?? []).map((module) => (
              <div key={module.id} className="row-card">
                <div>
                  <strong>{module.displayName}</strong>
                  <p>{module.category}</p>
                </div>
                <button
                  className={`toggle-btn${module.isVisible ? " on" : ""}`}
                  disabled={busyKey === `module-${module.id}`}
                  onClick={async () => {
                    await runAction(`module-${module.id}`, async () => {
                      await api.toggleModule(module.id, !module.isVisible);
                      await modulesQuery.reload();
                      await refresh();
                    }, `${module.displayName} 已${module.isVisible ? "隐藏" : "显示"}。`);
                  }}
                >
                  {module.isVisible ? <Eye size={16} /> : <EyeOff size={16} />}
                  <span>{module.isVisible ? "已显示" : "已隐藏"}</span>
                </button>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState title="无插件治理权限" description="当前账号不能调整插件显隐。" />
        )}
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="组织管理" hint="数据库为空时可直接创建首个组织，不注入演示数据。">
          {organizationsQuery.data && organizationsQuery.data.length > 0 ? (
            <div className="table-shell">
              {organizationsQuery.data.map((org) => (
                <div key={org.id} className="row-card">
                  <div>
                    <strong>{org.name}</strong>
                  <p>{roleDisplayText(org.defaultRole)} · {org.regionCode}</p>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无组织" description="请先创建第一个组织，建立权限与归属范围。" />
          )}

          {canManageOrganizations ? (
            <form
              className="inline-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await runAction("organization-create", async () => {
                  await api.createOrganization(orgForm);
                  setOrgForm({ name: "", defaultRole: "平台管理员", regionCode: "CN-SH" });
                  await organizationsQuery.reload();
                }, "组织已创建。");
              }}
            >
              <input placeholder="组织名称" value={orgForm.name} onChange={(e) => setOrgForm({ ...orgForm, name: e.target.value })} />
              <input placeholder="默认职位/角色" value={orgForm.defaultRole} onChange={(e) => setOrgForm({ ...orgForm, defaultRole: e.target.value })} />
              <input placeholder="区域编码" value={orgForm.regionCode} onChange={(e) => setOrgForm({ ...orgForm, regionCode: e.target.value })} />
              <button type="submit" disabled={busyKey === "organization-create"}>创建组织</button>
            </form>
          ) : null}
        </SectionBlock>

        <SectionBlock title="智能体审查队列" hint="所有智能体动作都必须可提交、可审查、可追溯。">
          {(reviewsQuery.data ?? []).length > 0 ? (
            <div className="table-shell">
              {reviewsQuery.data?.map((review) => (
                <div key={review.id} className="review-card">
                  <div>
                    <strong>{review.agentName} · {review.actionName}</strong>
                    <p>{review.payload}</p>
                    <small>{reviewStatusText(review.status)}</small>
                  </div>
                  {review.status === "Pending" && canDecideReview ? (
                    <div className="button-row">
                      <button
                        disabled={busyKey === `review-${review.id}`}
                        onClick={async () => {
                          await runAction(`review-${review.id}`, async () => {
                            await api.decideAgentReview(review.id, "Approved", "审查通过");
                            await reviewsQuery.reload();
                          }, "审查已通过。");
                        }}
                      >
                        通过
                      </button>
                      <button
                        className="secondary"
                        disabled={busyKey === `review-${review.id}`}
                        onClick={async () => {
                          await runAction(`review-${review.id}`, async () => {
                            await api.decideAgentReview(review.id, "Rejected", "需要修订");
                            await reviewsQuery.reload();
                          }, "审查已驳回。");
                        }}
                      >
                        驳回
                      </button>
                    </div>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无审查请求" description="新的智能体任务提交后会出现在这里。" />
          )}

          {canSubmitReview ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await runAction("submit-review", async () => {
                  await api.submitAgentReview(reviewForm);
                  setReviewForm({ agentName: "", actionName: "", payload: "" });
                  await reviewsQuery.reload();
                }, "审查请求已提交。");
              }}
            >
              <input placeholder="智能体名称" value={reviewForm.agentName} onChange={(e) => setReviewForm({ ...reviewForm, agentName: e.target.value })} />
              <input placeholder="动作名称" value={reviewForm.actionName} onChange={(e) => setReviewForm({ ...reviewForm, actionName: e.target.value })} />
              <textarea placeholder="请求载荷" value={reviewForm.payload} onChange={(e) => setReviewForm({ ...reviewForm, payload: e.target.value })} rows={3} />
              <button type="submit" disabled={busyKey === "submit-review"}>提交审查</button>
            </form>
          ) : null}
        </SectionBlock>
      </div>

      <div className="split-grid">
        <SectionBlock title="账号管理" hint="账号、角色、密码和角色分配都走真实权限控制。">
          {canReadUsers ? (
            (usersQuery.data ?? []).length > 0 ? (
              <div className="table-shell">
                {usersQuery.data?.map((user) => (
                  <div key={user.id} className="review-card account-card">
                    <div className="account-header">
                      <div>
                        <strong>{user.displayName} · {user.userName}</strong>
                        <p>{user.roles.map((role) => role.displayName).join(" / ") || "未分配角色"}</p>
                        <small>{user.isEnabled ? "启用" : "停用"}{user.id === currentUser?.id ? " · 当前账号" : ""}</small>
                      </div>
                      <div className="account-actions">
                        {rolesQuery.data && canManageUsers ? (
                          <select
                            value={user.roles[0]?.id ?? ""}
                            disabled={busyKey === `user-role-${user.id}`}
                            onChange={async (e) => {
                              await runAction(`user-role-${user.id}`, async () => {
                                await api.updateUserRoles(user.id, e.target.value ? [e.target.value] : []);
                                await usersQuery.reload();
                                if (user.id === currentUser?.id) {
                                  await refresh();
                                }
                              }, `${user.displayName} 的角色已更新。`);
                            }}
                          >
                            <option value="">未分配</option>
                            {rolesQuery.data.map((role) => (
                              <option key={role.id} value={role.id}>{role.displayName}</option>
                            ))}
                          </select>
                        ) : null}
                        <div className="button-row wrap">
                          {canManageUsers ? (
                            <button
                              className={user.isEnabled ? "secondary" : ""}
                              disabled={busyKey === `user-status-${user.id}` || user.id === currentUser?.id}
                              onClick={async () => {
                                await runAction(`user-status-${user.id}`, async () => {
                                  await api.updateUserStatus(user.id, !user.isEnabled);
                                  await usersQuery.reload();
                                }, `${user.displayName} 已${user.isEnabled ? "停用" : "启用"}。`);
                              }}
                            >
                              {user.isEnabled ? <ShieldAlert size={16} /> : <RefreshCcw size={16} />}
                              <span>{user.isEnabled ? "停用" : "启用"}</span>
                            </button>
                          ) : null}
                          {canManagePasswords ? (
                            <button
                              className="secondary icon-button"
                              disabled={busyKey === `user-reset-${user.id}`}
                              onClick={() => {
                                setResetPasswordUserId((current) => current === user.id ? null : user.id);
                                setResetPasswordForm({ newPassword: "" });
                              }}
                            >
                              <KeyRound size={16} />
                              <span>{resetPasswordUserId === user.id ? "收起重置" : "重置密码"}</span>
                            </button>
                          ) : null}
                        </div>
                      </div>
                    </div>

                    <AnimatePresence initial={false}>
                      {resetPasswordUserId === user.id ? (
                        <motion.form
                          key={`reset-password-${user.id}`}
                          className="inline-form account-password-form"
                          initial={{ opacity: 0, y: -8 }}
                          animate={{ opacity: 1, y: 0 }}
                          exit={{ opacity: 0, y: -8 }}
                          transition={{ duration: 0.18 }}
                          onSubmit={async (event) => {
                            event.preventDefault();
                            await runAction(`user-reset-${user.id}`, async () => {
                              await api.resetUserPassword(user.id, resetPasswordForm.newPassword);
                              setResetPasswordUserId(null);
                              setResetPasswordForm({ newPassword: "" });
                            }, `${user.displayName} 的密码已重置。`);
                          }}
                        >
                          <input
                            type="password"
                            placeholder="新密码（至少 8 位）"
                            value={resetPasswordForm.newPassword}
                            onChange={(e) => setResetPasswordForm({ newPassword: e.target.value })}
                          />
                          <button type="submit" disabled={busyKey === `user-reset-${user.id}`}>确认重置</button>
                        </motion.form>
                      ) : null}
                    </AnimatePresence>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无账号" description="请先创建平台账号，再分配角色与模块权限。" />
            )
          ) : (
            <EmptyState title="无账号查看权限" description="当前账号不能查看或管理平台账号。" />
          )}

          {canManageUsers ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await runAction("user-create", async () => {
                  await api.createUser({
                    userName: userForm.userName,
                    displayName: userForm.displayName,
                    password: userForm.password,
                    isEnabled: true,
                    roleIds: userForm.roleIds,
                  });
                  setUserForm({ userName: "", displayName: "", password: "", roleIds: [] });
                  await usersQuery.reload();
                }, "账号已创建。");
              }}
            >
              <input placeholder="登录账号" value={userForm.userName} onChange={(e) => setUserForm({ ...userForm, userName: e.target.value })} />
              <input placeholder="显示名称" value={userForm.displayName} onChange={(e) => setUserForm({ ...userForm, displayName: e.target.value })} />
              <input type="password" placeholder="初始密码" value={userForm.password} onChange={(e) => setUserForm({ ...userForm, password: e.target.value })} />
              <select
                value={userForm.roleIds[0] ?? ""}
                onChange={(e) => setUserForm({ ...userForm, roleIds: e.target.value ? [e.target.value] : [] })}
              >
                <option value="">选择角色</option>
                {rolesQuery.data?.map((role) => (
                  <option key={role.id} value={role.id}>{role.displayName}</option>
                ))}
              </select>
              <button type="submit" disabled={busyKey === "user-create"}>
                <UserCog size={16} />
                <span>创建账号</span>
              </button>
            </form>
          ) : null}
        </SectionBlock>

        <SectionBlock title="职位/角色模块权限" hint="职位/角色只可见并可进入被授权且未隐藏的模块。">
          {canManageRoles ? (
            (rolesQuery.data ?? []).length > 0 ? (
              <div className="table-shell">
                {rolesQuery.data?.map((role) => (
                  <div key={role.id} className="role-card">
                    <div>
                      <strong>{role.displayName}</strong>
                      <p>职位/角色权限包</p>
                    </div>
                    <div className="module-picker">
                      {(modulesQuery.data ?? []).map((module) => {
                        const checked = role.moduleKeys.includes(module.key);
                        return (
                          <label key={module.id} className="checkbox-row">
                            <input
                              type="checkbox"
                              checked={checked}
                              disabled={busyKey === `role-module-${role.id}`}
                              onChange={async (event) => {
                                const next = event.target.checked
                                  ? [...role.moduleKeys, module.key]
                                  : role.moduleKeys.filter((item) => item !== module.key);
                                await runAction(`role-module-${role.id}`, async () => {
                                  await api.updateRoleModules(role.id, next);
                                  await rolesQuery.reload();
                                  if (currentUser?.roles.includes(role.key)) {
                                    await refresh();
                                  }
                                }, `${role.displayName} 的模块权限已更新。`);
                              }}
                            />
                            <span>{module.displayName}</span>
                          </label>
                        );
                      })}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无角色" description="系统角色初始化失败时请检查后端启动日志。" />
            )
          ) : (
            <EmptyState title="无角色治理权限" description="当前账号不能调整角色与模块授权。" />
          )}
        </SectionBlock>
      </div>
    </PageShell>
  );
}
