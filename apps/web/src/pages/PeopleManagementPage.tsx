import { BriefcaseBusiness, Building2, Check, Circle, RefreshCcw, ShieldCheck, UserPlus, UsersRound } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { PositionPermissionOverview, RoleSummary, UserSummary } from "../types/api";

const loadEmptyUsers = () => Promise.resolve<UserSummary[]>([]);
const loadEmptyRoles = () => Promise.resolve<RoleSummary[]>([]);

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

type EmployeeForm = {
  userName: string;
  displayName: string;
  password: string;
  isEnabled: boolean;
  roleIds: string[];
};

/** 人员管理页面，基于真实平台用户、角色、组织和岗位数据承接员工入职管理。 */
export function PeopleManagementPage() {
  const { hasPermission } = useAuth();
  const canReadUsers = hasPermission(platformPermissions.identityUserRead);
  const canManageUsers = hasPermission(platformPermissions.identityUserManage);
  const canReadPositions = hasPermission(platformPermissions.positionPermissionsRead);

  const usersQuery = useAsyncData(canReadUsers ? api.listUsers : loadEmptyUsers, canReadUsers ? "users" : "no-users");
  const rolesQuery = useAsyncData(canManageUsers ? api.listRoleOptions : loadEmptyRoles, canManageUsers ? "role-options" : "no-role-options");
  const organizationsQuery = useAsyncData(api.listOrganizations);
  const positionQuery = useAsyncData(
    canReadPositions ? api.getPositionPermissionOverview : loadEmptyPositionOverview,
    canReadPositions ? "position-overview" : "no-position-overview",
  );

  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [employeeForm, setEmployeeForm] = useState<EmployeeForm>({
    userName: "",
    displayName: "",
    password: "",
    isEnabled: true,
    roleIds: [],
  });

  const users = usersQuery.data ?? [];
  const roles = rolesQuery.data ?? [];
  const organizations = organizationsQuery.data ?? [];
  const positionOverview = positionQuery.data ?? emptyPositionOverview;

  const enabledUserCount = users.filter((user) => user.isEnabled).length;
  const positionsByDepartment = useMemo(() => {
    const groups = new Map<string, typeof positionOverview.positions>();
    for (const position of positionOverview.positions) {
      const current = groups.get(position.departmentId) ?? [];
      groups.set(position.departmentId, [...current, position]);
    }

    return groups;
  }, [positionOverview.positions]);

  async function reloadAll() {
    await Promise.all([
      usersQuery.reload(),
      rolesQuery.reload(),
      organizationsQuery.reload(),
      positionQuery.reload(),
    ]);
  }

  function toggleRole(roleId: string) {
    setEmployeeForm((current) => ({
      ...current,
      roleIds: current.roleIds.includes(roleId)
        ? current.roleIds.filter((id) => id !== roleId)
        : [...current.roleIds, roleId],
    }));
  }

  async function createEmployee() {
    const payload = {
      userName: employeeForm.userName.trim(),
      displayName: employeeForm.displayName.trim(),
      password: employeeForm.password,
      isEnabled: employeeForm.isEnabled,
      roleIds: employeeForm.roleIds,
    };

    if (!payload.userName || !payload.displayName || !payload.password) {
      setError("请完整填写登录账号、员工姓名和初始密码。");
      return;
    }

    if (payload.roleIds.length === 0) {
      setError("请至少为新员工分配一个角色。");
      return;
    }

    setBusy(true);
    setMessage(null);
    setError(null);
    try {
      await api.createUser(payload);
      setEmployeeForm({ userName: "", displayName: "", password: "", isEnabled: true, roleIds: [] });
      await usersQuery.reload();
      setMessage("新员工账号已创建，并完成角色分配。");
    } catch (err) {
      setError(err instanceof Error ? err.message : "创建员工账号失败。");
    } finally {
      setBusy(false);
    }
  }

  return (
    <PageShell
      title="人员管理"
      actions={(
        <button type="button" className="secondary icon-button" onClick={reloadAll}>
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      )}
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <section className="stats-grid people-summary-grid">
        <StatTile label="员工账号" value={users.length} tone={users.length > 0 ? "success" : "warning"} />
        <StatTile label="启用员工" value={enabledUserCount} tone={enabledUserCount > 0 ? "success" : "warning"} />
        <StatTile label="组织数量" value={organizations.length} tone={organizations.length > 0 ? "success" : "warning"} />
        <StatTile label="岗位数量" value={positionOverview.positions.length} tone={positionOverview.positions.length > 0 ? "success" : "warning"} />
      </section>

      <section className="people-workspace-grid">
        <SectionBlock title="员工账号" hint="员工列表来自平台真实账号，角色决定可见模块和业务权限。">
          {!canReadUsers ? (
            <EmptyState title="缺少员工读取权限" description="当前账号没有读取用户列表的权限，无法查看员工账号。" />
          ) : usersQuery.loading ? (
            <div className="section-note">正在加载员工账号...</div>
          ) : usersQuery.error ? (
            <div className="section-note error">{usersQuery.error}</div>
          ) : users.length > 0 ? (
            <div className="people-card-list">
              {users.map((user) => (
                <div key={user.id} className="people-card-row">
                  <span className="people-avatar"><UsersRound size={17} /></span>
                  <div>
                    <strong>{user.displayName}</strong>
                    <p>{user.userName}</p>
                    <small>{user.roles.map((role) => role.displayName).join(" / ") || "未分配角色"}</small>
                  </div>
                  <span className={user.isEnabled ? "status-pill success" : "status-pill warning"}>
                    {user.isEnabled ? "启用" : "停用"}
                  </span>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无员工账号" description="请先通过入职建档创建真实员工账号。" />
          )}
        </SectionBlock>

        <SectionBlock title="入职建档" hint="创建真实登录账号，并在同一步分配角色权限。">
          {!canManageUsers ? (
            <EmptyState title="缺少员工维护权限" description="当前账号只能查看人员结构，不能创建员工账号。" />
          ) : rolesQuery.loading ? (
            <div className="section-note">正在加载角色选项...</div>
          ) : rolesQuery.error ? (
            <div className="section-note error">{rolesQuery.error}</div>
          ) : roles.length === 0 ? (
            <EmptyState title="暂无可分配角色" description="请先在平台治理中建立角色，再创建员工账号。" />
          ) : (
            <form
              className="stack-form people-onboarding-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await createEmployee();
              }}
            >
              <label>
                登录账号
                <input
                  value={employeeForm.userName}
                  onChange={(event) => setEmployeeForm({ ...employeeForm, userName: event.target.value })}
                  placeholder="例如 zhangsan"
                  autoComplete="username"
                />
              </label>
              <label>
                员工姓名
                <input
                  value={employeeForm.displayName}
                  onChange={(event) => setEmployeeForm({ ...employeeForm, displayName: event.target.value })}
                  placeholder="例如 张三"
                />
              </label>
              <label>
                初始密码
                <input
                  value={employeeForm.password}
                  onChange={(event) => setEmployeeForm({ ...employeeForm, password: event.target.value })}
                  type="password"
                  autoComplete="new-password"
                />
              </label>
              <fieldset className="people-form-section">
                <legend>角色权限</legend>
                <div className="people-role-picker" aria-label="选择员工角色">
                  {roles.map((role) => {
                    const selected = employeeForm.roleIds.includes(role.id);
                    return (
                      <button
                        key={role.id}
                        type="button"
                        className={selected ? "people-choice-button selected" : "people-choice-button"}
                        aria-pressed={selected}
                        onClick={() => toggleRole(role.id)}
                      >
                        {selected ? <Check size={16} /> : <Circle size={16} />}
                        <span>{role.displayName}</span>
                      </button>
                    );
                  })}
                </div>
              </fieldset>
              <fieldset className="people-form-section compact">
                <legend>登录状态</legend>
                <div className="people-state-switch" aria-label="选择账号登录状态">
                  <button
                    type="button"
                    className={employeeForm.isEnabled ? "people-choice-button selected" : "people-choice-button"}
                    aria-pressed={employeeForm.isEnabled}
                    onClick={() => setEmployeeForm({ ...employeeForm, isEnabled: true })}
                  >
                    <ShieldCheck size={16} />
                    <span>允许登录</span>
                  </button>
                  <button
                    type="button"
                    className={!employeeForm.isEnabled ? "people-choice-button selected warning" : "people-choice-button"}
                    aria-pressed={!employeeForm.isEnabled}
                    onClick={() => setEmployeeForm({ ...employeeForm, isEnabled: false })}
                  >
                    <Circle size={16} />
                    <span>暂不启用</span>
                  </button>
                </div>
              </fieldset>
              <button type="submit" disabled={busy}>
                <UserPlus size={16} />
                <span>创建员工账号</span>
              </button>
            </form>
          )}
        </SectionBlock>
      </section>

      <section className="people-workspace-grid">
        <SectionBlock title="组织归属" hint="组织清单来自平台组织档案，员工账号后续可继续扩展为组织归属字段。">
          {organizationsQuery.loading ? (
            <div className="section-note">正在加载组织...</div>
          ) : organizationsQuery.error ? (
            <div className="section-note error">{organizationsQuery.error}</div>
          ) : organizations.length > 0 ? (
            <div className="people-card-list compact-list">
              {organizations.map((organization) => (
                <div key={organization.id} className="people-card-row">
                  <span className="people-avatar"><Building2 size={17} /></span>
                  <div>
                    <strong>{organization.name}</strong>
                    <p>{organization.regionCode}</p>
                    <small>默认角色：{organization.defaultRole}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无组织" description="请先在平台治理中建立企业组织。" />
          )}
        </SectionBlock>

        <SectionBlock title="部门岗位架构" hint="部门和岗位来自岗位权限模块，用于承接员工编制和权限结构。">
          {!canReadPositions ? (
            <EmptyState title="缺少岗位读取权限" description="当前账号不能读取部门和岗位架构。" />
          ) : positionQuery.loading ? (
            <div className="section-note">正在加载部门岗位...</div>
          ) : positionQuery.error ? (
            <div className="section-note error">{positionQuery.error}</div>
          ) : positionOverview.departments.length > 0 ? (
            <div className="people-org-tree">
              {positionOverview.departments.map((department) => {
                const positions = positionsByDepartment.get(department.id) ?? [];
                return (
                  <div key={department.id} className="people-org-node">
                    <div className="people-org-node-head">
                      <BriefcaseBusiness size={17} />
                      <div>
                        <strong>{department.name}</strong>
                        <small>{department.code} · {department.isEnabled ? "启用" : "停用"}</small>
                      </div>
                    </div>
                    <div className="compact-tag-list">
                      {positions.length > 0 ? positions.map((position) => (
                        <span key={position.id} className="compact-tag">
                          {position.name}
                        </span>
                      )) : (
                        <span className="compact-tag">未配置岗位</span>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无部门岗位" description="请先在岗位权限模块建立部门和岗位。" />
          )}
        </SectionBlock>
      </section>

      <SectionBlock title="落地边界" hint="人员主档已接入真实账号；员工与部门、岗位、组织的强绑定需要后端新增员工档案表后闭环。">
        <div className="people-readiness-grid">
          <div className="people-readiness-card ready">
            <ShieldCheck size={18} />
            <strong>账号入职</strong>
            <span>已调用真实用户创建接口</span>
          </div>
          <div className="people-readiness-card">
            <BriefcaseBusiness size={18} />
            <strong>岗位归属</strong>
            <span>等待员工档案与岗位关系表</span>
          </div>
          <div className="people-readiness-card">
            <Building2 size={18} />
            <strong>组织归属</strong>
            <span>等待员工档案与组织关系表</span>
          </div>
        </div>
      </SectionBlock>
    </PageShell>
  );
}
