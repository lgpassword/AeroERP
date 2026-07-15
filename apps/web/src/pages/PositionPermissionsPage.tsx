import { RefreshCcw } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { PositionPermissionOption, PositionPermissionOverview } from "../types/api";

const emptyOverview: PositionPermissionOverview = {
  departments: [],
  positions: [],
  roles: [],
  permissionPackages: [],
  roleBindings: [],
  dataScopeRules: [],
  permissions: [],
  modules: [],
};

const loadEmptyOverview = () => Promise.resolve(emptyOverview);

const scopeLabels: Record<string, string> = {
  department: "部门范围",
  organization: "组织范围",
  warehouse: "仓库范围",
  project: "项目范围",
};

function toggleSelection(values: string[], value: string) {
  return values.includes(value)
    ? values.filter((item) => item !== value)
    : [...values, value];
}

/** 权限选择器，复用在权限包和自定义岗位角色的权限勾选场景。 */
function PermissionSelector({
  permissions,
  selected,
  onChange,
}: {
  permissions: PositionPermissionOption[];
  selected: string[];
  onChange: (next: string[]) => void;
}) {
  const groups = useMemo(() => {
    const map = new Map<string, PositionPermissionOption[]>();
    permissions.forEach((permission) => {
      const key = permission.moduleDisplayName;
      map.set(key, [...(map.get(key) ?? []), permission]);
    });
    return Array.from(map.entries());
  }, [permissions]);

  return (
    <div className="permission-picker">
      {groups.map(([groupName, items]) => (
        <div key={groupName} className="permission-group">
          <strong>{groupName}</strong>
          <div className="permission-option-grid">
            {items.map((permission) => (
              <label key={permission.key} className="checkbox-row compact">
                <input
                  type="checkbox"
                  checked={selected.includes(permission.key)}
                  onChange={() => onChange(toggleSelection(selected, permission.key))}
                />
                <span>{permission.displayName}</span>
              </label>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}

/** 岗位权限页面，管理部门、岗位、权限包、岗位角色绑定和数据范围规则。 */
export function PositionPermissionsPage() {
  const { hasPermission } = useAuth();
  const canRead = hasPermission(platformPermissions.positionPermissionsRead);
  const canManage = hasPermission(platformPermissions.positionPermissionsManage);
  const overviewQuery = useAsyncData(canRead ? api.getPositionPermissionOverview : loadEmptyOverview);

  const overview = overviewQuery.data ?? emptyOverview;
  const customRoles = overview.roles.filter((role) => !role.isSystemProtected);
  const roleOptions = overview.roles;
  const [departmentForm, setDepartmentForm] = useState({
    id: "",
    code: "",
    name: "",
    parentDepartmentId: "",
    isEnabled: true,
  });
  const [positionForm, setPositionForm] = useState({
    id: "",
    code: "",
    name: "",
    departmentId: "",
    description: "",
    isEnabled: true,
  });
  const [packageForm, setPackageForm] = useState({
    id: "",
    displayName: "",
    description: "",
    moduleKeys: [] as string[],
    permissions: [] as string[],
    isEnabled: true,
  });
  const [roleForm, setRoleForm] = useState({
    id: "",
    displayName: "",
    moduleKeys: [] as string[],
    permissions: [] as string[],
  });
  const [selectedPackageId, setSelectedPackageId] = useState("");
  const [selectedPositionId, setSelectedPositionId] = useState("");
  const [bindingRoleIds, setBindingRoleIds] = useState<string[]>([]);
  const [scopeForm, setScopeForm] = useState({
    scopeType: "department",
    matchValue: "",
    description: "",
    isEnabled: true,
  });
  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!selectedPositionId && overview.positions.length > 0) {
      setSelectedPositionId(overview.positions[0].id);
    }
  }, [overview.positions, selectedPositionId]);

  useEffect(() => {
    const bindings = overview.roleBindings
      .filter((binding) => binding.positionId === selectedPositionId)
      .map((binding) => binding.roleId);
    setBindingRoleIds(bindings);

    const existingScope = overview.dataScopeRules.find((rule) => rule.positionId === selectedPositionId);
    setScopeForm({
      scopeType: existingScope?.scopeType ?? "department",
      matchValue: existingScope?.matchValue ?? "",
      description: existingScope?.description ?? "",
      isEnabled: existingScope?.isEnabled ?? true,
    });
  }, [overview.dataScopeRules, overview.roleBindings, selectedPositionId]);

  async function runAction(actionKey: string, action: () => Promise<void>, successText: string) {
    setBusyKey(actionKey);
    setMessage(null);
    setError(null);
    try {
      await action();
      setMessage(successText);
    } catch (err) {
      setError(err instanceof Error ? err.message : "操作失败");
    } finally {
      setBusyKey(null);
    }
  }

  function permissionNames(keys: string[]) {
    return keys
      .map((key) => overview.permissions.find((permission) => permission.key === key)?.displayName)
      .filter(Boolean)
      .join("、");
  }

  function moduleNames(keys: string[]) {
    return keys
      .map((key) => overview.modules.find((module) => module.key === key)?.displayName)
      .filter(Boolean)
      .join("、");
  }

  function applyPackageToRole(packageId: string) {
    setSelectedPackageId(packageId);
    const selectedPackage = overview.permissionPackages.find((item) => item.id === packageId);
    if (!selectedPackage) {
      return;
    }

    setRoleForm({
      ...roleForm,
      moduleKeys: selectedPackage.moduleKeys,
      permissions: selectedPackage.permissions,
    });
  }

  if (!canRead) {
    return (
      <PageShell title="岗位权限">
        <EmptyState title="无岗位权限查看权限" description="当前账号不能查看部门、岗位、自定义角色或权限包。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="岗位权限"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "refresh"}
          onClick={async () => runAction("refresh", overviewQuery.reload, "岗位权限数据已刷新。")}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      }
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <section className="stats-grid">
        <StatTile label="部门" value={overview.departments.length} tone={overview.departments.length > 0 ? "success" : "default"} />
        <StatTile label="岗位" value={overview.positions.length} tone={overview.positions.length > 0 ? "success" : "default"} />
        <StatTile label="自定义角色" value={customRoles.length} tone={customRoles.length > 0 ? "success" : "default"} />
        <StatTile label="权限包" value={overview.permissionPackages.length} tone={overview.permissionPackages.length > 0 ? "success" : "default"} />
      </section>

      <div className="split-grid">
        <SectionBlock title="部门" hint="部门是岗位归属边界，用于组织职位和数据范围。">
          {overview.departments.length > 0 ? (
            <div className="table-shell">
              {overview.departments.map((department) => (
                <div key={department.id} className="review-card">
                  <div>
                    <strong>{department.name}</strong>
                    <p>{department.isEnabled ? "启用" : "停用"}</p>
                  </div>
                  {canManage ? (
                    <button
                      type="button"
                      className="secondary"
                      onClick={() => setDepartmentForm({
                        id: department.id,
                        code: department.code,
                        name: department.name,
                        parentDepartmentId: department.parentDepartmentId ?? "",
                        isEnabled: department.isEnabled,
                      })}
                    >
                      编辑
                    </button>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无部门" description="创建部门后即可维护岗位和权限范围。" />
          )}

          {canManage ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await runAction("department-save", async () => {
                  await api.upsertPositionDepartment({
                    id: departmentForm.id || null,
                    code: departmentForm.code,
                    name: departmentForm.name,
                    parentDepartmentId: departmentForm.parentDepartmentId || null,
                    isEnabled: departmentForm.isEnabled,
                  });
                  setDepartmentForm({ id: "", code: "", name: "", parentDepartmentId: "", isEnabled: true });
                  await overviewQuery.reload();
                }, "部门已保存。");
              }}
            >
              <input placeholder="部门编码" value={departmentForm.code} onChange={(event) => setDepartmentForm({ ...departmentForm, code: event.target.value })} />
              <input placeholder="部门名称" value={departmentForm.name} onChange={(event) => setDepartmentForm({ ...departmentForm, name: event.target.value })} />
              <label className="checkbox-row">
                <input type="checkbox" checked={departmentForm.isEnabled} onChange={(event) => setDepartmentForm({ ...departmentForm, isEnabled: event.target.checked })} />
                <span>启用部门</span>
              </label>
              <button type="submit" disabled={busyKey === "department-save" || !departmentForm.code.trim() || !departmentForm.name.trim()}>保存部门</button>
            </form>
          ) : null}
        </SectionBlock>

        <SectionBlock title="岗位" hint="岗位绑定部门和角色，用户后续可按岗位获取对应权限。">
          {overview.positions.length > 0 ? (
            <div className="table-shell">
              {overview.positions.map((position) => (
                <div key={position.id} className="review-card">
                  <div>
                    <strong>{position.name}</strong>
                    <p>{position.departmentName} · {position.description || "无备注"}</p>
                    <small>{position.isEnabled ? "启用" : "停用"}</small>
                  </div>
                  {canManage ? (
                    <button
                      type="button"
                      className="secondary"
                      onClick={() => setPositionForm({
                        id: position.id,
                        code: position.code,
                        name: position.name,
                        departmentId: position.departmentId,
                        description: position.description,
                        isEnabled: position.isEnabled,
                      })}
                    >
                      编辑
                    </button>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无岗位" description="先创建部门，再维护岗位。" />
          )}

          {canManage ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await runAction("position-save", async () => {
                  await api.upsertJobPosition({
                    id: positionForm.id || null,
                    code: positionForm.code,
                    name: positionForm.name,
                    departmentId: positionForm.departmentId,
                    description: positionForm.description,
                    isEnabled: positionForm.isEnabled,
                  });
                  setPositionForm({ id: "", code: "", name: "", departmentId: "", description: "", isEnabled: true });
                  await overviewQuery.reload();
                }, "岗位已保存。");
              }}
            >
              <select value={positionForm.departmentId} onChange={(event) => setPositionForm({ ...positionForm, departmentId: event.target.value })}>
                <option value="">选择部门</option>
                {overview.departments.filter((department) => department.isEnabled).map((department) => (
                  <option key={department.id} value={department.id}>{department.name}</option>
                ))}
              </select>
              <input placeholder="岗位编码" value={positionForm.code} onChange={(event) => setPositionForm({ ...positionForm, code: event.target.value })} />
              <input placeholder="岗位名称" value={positionForm.name} onChange={(event) => setPositionForm({ ...positionForm, name: event.target.value })} />
              <input placeholder="岗位说明" value={positionForm.description} onChange={(event) => setPositionForm({ ...positionForm, description: event.target.value })} />
              <label className="checkbox-row">
                <input type="checkbox" checked={positionForm.isEnabled} onChange={(event) => setPositionForm({ ...positionForm, isEnabled: event.target.checked })} />
                <span>启用岗位</span>
              </label>
              <button type="submit" disabled={busyKey === "position-save" || !positionForm.departmentId || !positionForm.code.trim() || !positionForm.name.trim()}>保存岗位</button>
            </form>
          ) : null}
        </SectionBlock>
      </div>

      <div className="split-grid">
        <SectionBlock title="权限包" hint="权限包用于复用一组模块和权限，创建角色时可以套用。">
          {overview.permissionPackages.length > 0 ? (
            <div className="table-shell">
              {overview.permissionPackages.map((item) => (
                <div key={item.id} className="review-card">
                  <div>
                    <strong>{item.displayName}</strong>
                    <p>{item.description || "无备注"}</p>
                    <small>{moduleNames(item.moduleKeys) || "未选择模块"} · {permissionNames(item.permissions) || "未选择权限"}</small>
                  </div>
                  {canManage ? (
                    <button
                      type="button"
                      className="secondary"
                      onClick={() => setPackageForm({
                        id: item.id,
                        displayName: item.displayName,
                        description: item.description,
                        moduleKeys: item.moduleKeys,
                        permissions: item.permissions,
                        isEnabled: item.isEnabled,
                      })}
                    >
                      编辑
                    </button>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无权限包" description="保存权限包后可快速套用到自定义角色。" />
          )}

          {canManage ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await runAction("package-save", async () => {
                  await api.upsertPositionPermissionPackage({
                    id: packageForm.id || null,
                    displayName: packageForm.displayName,
                    description: packageForm.description,
                    moduleKeys: packageForm.moduleKeys,
                    permissions: packageForm.permissions,
                    isEnabled: packageForm.isEnabled,
                  });
                  setPackageForm({ id: "", displayName: "", description: "", moduleKeys: [], permissions: [], isEnabled: true });
                  await overviewQuery.reload();
                }, "权限包已保存。");
              }}
            >
              <input placeholder="权限包名称" value={packageForm.displayName} onChange={(event) => setPackageForm({ ...packageForm, displayName: event.target.value })} />
              <input placeholder="权限包说明" value={packageForm.description} onChange={(event) => setPackageForm({ ...packageForm, description: event.target.value })} />
              <div className="compact-tag-list">
                {overview.modules.map((module) => (
                  <label key={module.key} className="checkbox-row compact">
                    <input type="checkbox" checked={packageForm.moduleKeys.includes(module.key)} onChange={() => setPackageForm({ ...packageForm, moduleKeys: toggleSelection(packageForm.moduleKeys, module.key) })} />
                    <span>{module.displayName}</span>
                  </label>
                ))}
              </div>
              <PermissionSelector permissions={overview.permissions} selected={packageForm.permissions} onChange={(permissions) => setPackageForm({ ...packageForm, permissions })} />
              <label className="checkbox-row">
                <input type="checkbox" checked={packageForm.isEnabled} onChange={(event) => setPackageForm({ ...packageForm, isEnabled: event.target.checked })} />
                <span>启用权限包</span>
              </label>
              <button type="submit" disabled={busyKey === "package-save" || !packageForm.displayName.trim() || packageForm.permissions.length === 0}>保存权限包</button>
            </form>
          ) : null}
        </SectionBlock>

        <SectionBlock title="自定义角色" hint="自定义角色只显示中文名称，内置系统角色不可在这里修改。">
          {roleOptions.length > 0 ? (
            <div className="table-shell">
              {roleOptions.map((role) => (
                <div key={role.id} className="review-card">
                  <div>
                    <strong>{role.displayName}</strong>
                    <p>{role.isSystemProtected ? "系统内置" : "用户自定义"} · {moduleNames(role.moduleKeys) || "未选择模块"}</p>
                    <small>{permissionNames(role.permissions) || "未配置权限"}</small>
                  </div>
                  {canManage && !role.isSystemProtected ? (
                    <button
                      type="button"
                      className="secondary"
                      onClick={() => setRoleForm({
                        id: role.id,
                        displayName: role.displayName,
                        moduleKeys: role.moduleKeys,
                        permissions: role.permissions,
                      })}
                    >
                      编辑
                    </button>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无角色" description="系统会准备内置角色，也可以在这里新增自定义角色。" />
          )}

          {canManage ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await runAction("role-save", async () => {
                  await api.upsertCustomPositionRole({
                    id: roleForm.id || null,
                    displayName: roleForm.displayName,
                    moduleKeys: roleForm.moduleKeys,
                    permissions: roleForm.permissions,
                  });
                  setRoleForm({ id: "", displayName: "", moduleKeys: [], permissions: [] });
                  setSelectedPackageId("");
                  await overviewQuery.reload();
                }, "自定义角色已保存。");
              }}
            >
              <input placeholder="角色名称" value={roleForm.displayName} onChange={(event) => setRoleForm({ ...roleForm, displayName: event.target.value })} />
              <select value={selectedPackageId} onChange={(event) => applyPackageToRole(event.target.value)}>
                <option value="">套用权限包</option>
                {overview.permissionPackages.filter((item) => item.isEnabled).map((item) => (
                  <option key={item.id} value={item.id}>{item.displayName}</option>
                ))}
              </select>
              <div className="compact-tag-list">
                {overview.modules.map((module) => (
                  <label key={module.key} className="checkbox-row compact">
                    <input type="checkbox" checked={roleForm.moduleKeys.includes(module.key)} onChange={() => setRoleForm({ ...roleForm, moduleKeys: toggleSelection(roleForm.moduleKeys, module.key) })} />
                    <span>{module.displayName}</span>
                  </label>
                ))}
              </div>
              <PermissionSelector permissions={overview.permissions} selected={roleForm.permissions} onChange={(permissions) => setRoleForm({ ...roleForm, permissions })} />
              <button type="submit" disabled={busyKey === "role-save" || !roleForm.displayName.trim() || roleForm.permissions.length === 0}>保存角色</button>
            </form>
          ) : null}
        </SectionBlock>
      </div>

      <SectionBlock title="岗位绑定" hint="把岗位绑定到一个或多个角色，并维护岗位级数据范围。">
        {overview.positions.length > 0 ? (
          <div className="position-binding-panel">
            <div className="position-selector-row">
              <select value={selectedPositionId} onChange={(event) => setSelectedPositionId(event.target.value)}>
                {overview.positions.map((position) => (
                  <option key={position.id} value={position.id}>{position.name} · {position.departmentName}</option>
                ))}
              </select>
              {canManage ? (
                <button
                  type="button"
                  disabled={busyKey === "binding-save" || !selectedPositionId}
                  onClick={async () => runAction("binding-save", async () => {
                    await api.updatePositionRoleBindings(selectedPositionId, bindingRoleIds);
                    await overviewQuery.reload();
                  }, "岗位角色绑定已保存。")}
                >
                  保存角色绑定
                </button>
              ) : null}
            </div>

            <div className="compact-tag-list">
              {roleOptions.map((role) => (
                <label key={role.id} className="checkbox-row compact">
                  <input type="checkbox" checked={bindingRoleIds.includes(role.id)} disabled={!canManage} onChange={() => setBindingRoleIds(toggleSelection(bindingRoleIds, role.id))} />
                  <span>{role.displayName}</span>
                </label>
              ))}
            </div>

            <div className="table-shell">
              {overview.roleBindings.filter((binding) => binding.positionId === selectedPositionId).map((binding) => (
                <div key={binding.id} className="review-card">
                  <div>
                    <strong>{binding.positionName}</strong>
                    <p>{binding.roleDisplayName}</p>
                  </div>
                </div>
              ))}
            </div>

            {canManage ? (
              <form
                className="inline-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  await runAction("scope-save", async () => {
                    await api.updatePositionDataScopeRules(selectedPositionId, [scopeForm]);
                    await overviewQuery.reload();
                  }, "岗位数据范围已保存。");
                }}
              >
                <select value={scopeForm.scopeType} onChange={(event) => setScopeForm({ ...scopeForm, scopeType: event.target.value })}>
                  {Object.entries(scopeLabels).map(([value, label]) => (
                    <option key={value} value={value}>{label}</option>
                  ))}
                </select>
                <input placeholder="范围值" value={scopeForm.matchValue} onChange={(event) => setScopeForm({ ...scopeForm, matchValue: event.target.value })} />
                <input placeholder="范围说明" value={scopeForm.description} onChange={(event) => setScopeForm({ ...scopeForm, description: event.target.value })} />
                <label className="checkbox-row">
                  <input type="checkbox" checked={scopeForm.isEnabled} onChange={(event) => setScopeForm({ ...scopeForm, isEnabled: event.target.checked })} />
                  <span>启用范围</span>
                </label>
                <button type="submit" disabled={busyKey === "scope-save" || !selectedPositionId || !scopeForm.matchValue.trim()}>保存数据范围</button>
              </form>
            ) : null}

            {overview.dataScopeRules.filter((rule) => rule.positionId === selectedPositionId).length > 0 ? (
              <div className="compact-tag-list">
                {overview.dataScopeRules.filter((rule) => rule.positionId === selectedPositionId).map((rule) => (
                  <span key={rule.id} className="compact-tag">{scopeLabels[rule.scopeType] ?? "自定义范围"}：{rule.matchValue}</span>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无数据范围" description="保存数据范围后，岗位级范围会被记录并可审计。" />
            )}
          </div>
        ) : (
          <EmptyState title="暂无可绑定岗位" description="先创建部门和岗位，再绑定角色与数据范围。" />
        )}
      </SectionBlock>
    </PageShell>
  );
}
