using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.PositionPermissions.Contracts;
using AeroERP.Modules.PositionPermissions.Domain;
using AeroERP.Modules.PositionPermissions.Services;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Position Permission Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class PositionPermissionService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IPositionPermissionService
{
    /// <summary>
    /// Package Prefix。
    /// </summary>
    private const string PackagePrefix = "permission-package-";
    /// <summary>
    /// Custom Role Prefix。
    /// </summary>
    private const string CustomRolePrefix = "custom-role-";

    /// <summary>
    /// Permission Names。
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> PermissionNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [PlatformPermissions.PlatformManage] = "平台管理",
            [PlatformPermissions.OrganizationManage] = "组织管理",
            [PlatformPermissions.IdentityUserRead] = "用户查看",
            [PlatformPermissions.IdentityUserManage] = "用户维护",
            [PlatformPermissions.IdentityUserPasswordManage] = "密码重置",
            [PlatformPermissions.IdentityRoleManage] = "角色维护",
            [PlatformPermissions.PluginManage] = "插件管理",
            [PlatformPermissions.PositionPermissionsRead] = "岗位权限查看",
            [PlatformPermissions.PositionPermissionsManage] = "岗位权限维护",
            [PlatformPermissions.AgentReviewSubmit] = "智能体复核提交",
            [PlatformPermissions.AgentReviewDecide] = "智能体复核决策",
            [PlatformPermissions.MasterDataRead] = "主数据查看",
            [PlatformPermissions.MasterDataManage] = "主数据维护",
            [PlatformPermissions.ProcurementRead] = "采购查看",
            [PlatformPermissions.ProcurementRequestCreate] = "采购申请创建",
            [PlatformPermissions.ProcurementRequestReview] = "采购申请复核",
            [PlatformPermissions.ProcurementOrderCreate] = "采购订单创建",
            [PlatformPermissions.ProcurementOrderRelease] = "采购订单下达",
            [PlatformPermissions.InventoryRead] = "库存查看",
            [PlatformPermissions.InventoryReceiptManage] = "入库管理",
            [PlatformPermissions.InventoryIssueManage] = "出库管理",
            [PlatformPermissions.InventoryTransferManage] = "调拨管理",
            [PlatformPermissions.InventoryCountManage] = "盘点管理",
            [PlatformPermissions.InventoryLocationManage] = "库位管理",
            [PlatformPermissions.WmsRead] = "WMS 查看",
            [PlatformPermissions.WmsManage] = "WMS 维护",
            [PlatformPermissions.WmsExecute] = "WMS 执行",
            [PlatformPermissions.SalesRead] = "销售查看",
            [PlatformPermissions.SalesQuotationCreate] = "报价创建",
            [PlatformPermissions.SalesOrderCreate] = "销售订单创建",
            [PlatformPermissions.SalesOrderManage] = "销售订单管理",
            [PlatformPermissions.FinanceRead] = "财务查看",
            [PlatformPermissions.FinanceAccountingManage] = "会计基础维护",
            [PlatformPermissions.FinanceVoucherManage] = "总账凭证维护",
            [PlatformPermissions.FinanceVoucherReview] = "总账凭证审核",
            [PlatformPermissions.FinancePayableManage] = "应付管理",
            [PlatformPermissions.FinanceReceivableManage] = "应收管理",
            [PlatformPermissions.FinanceSettlementManage] = "结算管理",
            [PlatformPermissions.WorkflowRead] = "审批查看",
            [PlatformPermissions.WorkflowTaskDecide] = "审批决策",
            [PlatformPermissions.NotificationRead] = "通知查看",
            [PlatformPermissions.ControlAnalyticsRead] = "经营分析查看",
            [PlatformPermissions.ControlDataScopeManage] = "数据范围维护",
            [PlatformPermissions.ControlNumberingManage] = "编号规则维护",
            [PlatformPermissions.LocalizationRead] = "本地化查看",
            [PlatformPermissions.LocalizationManage] = "本地化维护",
            [PlatformPermissions.ManufacturingRead] = "制造查看",
            [PlatformPermissions.ManufacturingBomManage] = "BOM 维护",
            [PlatformPermissions.ManufacturingWorkOrderManage] = "工单维护",
            [PlatformPermissions.ManufacturingExecutionManage] = "制造执行",
            [PlatformPermissions.AdvancedManufacturingRead] = "高级制造查看",
            [PlatformPermissions.AdvancedManufacturingManage] = "高级制造维护",
            [PlatformPermissions.AdvancedManufacturingSchedule] = "工序排程",
            [PlatformPermissions.AdvancedManufacturingCostManage] = "制造成本维护",
            [PlatformPermissions.AdvancedManufacturingMrpManage] = "MRP 维护",
            [PlatformPermissions.ReportingRead] = "报表查看",
            [PlatformPermissions.ReportingManage] = "报表维护",
            [PlatformPermissions.ReportingExport] = "报表导出",
            [PlatformPermissions.QualityRead] = "质量查看",
            [PlatformPermissions.QualityInspectionManage] = "质检维护",
            [PlatformPermissions.QualityTraceabilityManage] = "追溯维护",
            [PlatformPermissions.PlanningRead] = "计划查看",
            [PlatformPermissions.PlanningManage] = "计划维护",
            [PlatformPermissions.OutsourcingManage] = "委外管理",
            [PlatformPermissions.BarcodeExecute] = "扫码执行"
        };

    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<PositionPermissionOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var departments = await dbContext.PositionDepartments
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
        var positions = await dbContext.JobPositions
            .AsNoTracking()
            .OrderBy(x => x.DepartmentName)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);
        var packages = await dbContext.PermissionPackages
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);
        var bindings = await dbContext.PositionRoleBindings
            .AsNoTracking()
            .OrderBy(x => x.PositionId)
            .ToListAsync(cancellationToken);
        var dataScopes = await dbContext.PositionDataScopeRules
            .AsNoTracking()
            .OrderBy(x => x.PositionId)
            .ThenBy(x => x.ScopeType)
            .ToListAsync(cancellationToken);
        var roles = await dbContext.Roles
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);
        var roleGrants = await dbContext.RolePermissionGrants
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var modules = await dbContext.PluginModules
            .AsNoTracking()
            .OrderBy(x => x.Category)
            .ThenBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        var positionNames = positions.ToDictionary(x => x.Id, x => x.Name);
        var roleNames = roles.ToDictionary(x => x.Id, x => x.DisplayName);
        var moduleNames = modules.ToDictionary(x => x.Key, x => x.DisplayName, StringComparer.OrdinalIgnoreCase);
        var grantsByRole = roleGrants
            .GroupBy(x => x.RoleId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Permission).ToList());

        return new PositionPermissionOverviewDto(
            departments.Select(MapDepartment).ToList(),
            positions.Select(MapPosition).ToList(),
            roles.Select(role => MapRole(role, grantsByRole)).ToList(),
            packages.Select(MapPackage).ToList(),
            bindings.Select(binding => MapBinding(binding, positionNames, roleNames)).ToList(),
            dataScopes.Select(rule => MapDataScope(rule, positionNames)).ToList(),
            PlatformPermissions.All
                .Select(permission => new PermissionOptionDto(
                    permission,
                    PermissionNames.TryGetValue(permission, out var name) ? name : permission,
                    ResolvePermissionModuleKey(permission),
                    moduleNames.GetValueOrDefault(ResolvePermissionModuleKey(permission), "平台")))
                .OrderBy(x => x.ModuleDisplayName)
                .ThenBy(x => x.DisplayName)
                .ToList(),
            modules.Select(x => new ModuleOptionDto(x.Key, x.DisplayName)).ToList());
    }

    /// <summary>
    /// Upsert Department Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<DepartmentDto>> UpsertDepartmentAsync(UpsertDepartmentRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeCode(request.Code);
        var name = NormalizeText(request.Name);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<DepartmentDto>.Failure("部门编码和部门名称不能为空。");
        }

        if (code.Length > 64 || name.Length > 128)
        {
            return OperationResult<DepartmentDto>.Failure("部门编码或名称长度超过限制。");
        }

        if (request.ParentDepartmentId is not null &&
            !await dbContext.PositionDepartments.AnyAsync(x => x.Id == request.ParentDepartmentId, cancellationToken))
        {
            return OperationResult<DepartmentDto>.Failure("上级部门不存在。");
        }

        var entity = request.Id is null
            ? await dbContext.PositionDepartments.FirstOrDefaultAsync(x => x.Code == code, cancellationToken)
            : await dbContext.PositionDepartments.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken);

        var duplicate = await dbContext.PositionDepartments
            .AnyAsync(x => x.Code == code && (entity == null || x.Id != entity.Id), cancellationToken);
        if (duplicate)
        {
            return OperationResult<DepartmentDto>.Failure("部门编码已存在。");
        }

        if (entity is null)
        {
            entity = new PositionDepartment(code, name, request.ParentDepartmentId, request.IsEnabled);
            dbContext.PositionDepartments.Add(entity);
        }
        else
        {
            entity.Update(code, name, request.ParentDepartmentId, request.IsEnabled);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("PositionPermissions", "DepartmentUpserted", currentUser.GetActor(), name, cancellationToken);
        return OperationResult<DepartmentDto>.Success(MapDepartment(entity));
    }

    /// <summary>
    /// Upsert Position Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<JobPositionDto>> UpsertPositionAsync(UpsertJobPositionRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeCode(request.Code);
        var name = NormalizeText(request.Name);
        var description = NormalizeText(request.Description);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<JobPositionDto>.Failure("岗位编码和岗位名称不能为空。");
        }

        var department = await dbContext.PositionDepartments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
        if (department is null)
        {
            return OperationResult<JobPositionDto>.Failure("所属部门不存在。");
        }

        var entity = request.Id is null
            ? await dbContext.JobPositions.FirstOrDefaultAsync(x => x.Code == code, cancellationToken)
            : await dbContext.JobPositions.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken);

        var duplicate = await dbContext.JobPositions
            .AnyAsync(x => x.Code == code && (entity == null || x.Id != entity.Id), cancellationToken);
        if (duplicate)
        {
            return OperationResult<JobPositionDto>.Failure("岗位编码已存在。");
        }

        if (entity is null)
        {
            entity = new JobPosition(code, name, department.Id, department.Name, description, request.IsEnabled);
            dbContext.JobPositions.Add(entity);
        }
        else
        {
            entity.Update(code, name, department.Id, department.Name, description, request.IsEnabled);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("PositionPermissions", "PositionUpserted", currentUser.GetActor(), name, cancellationToken);
        return OperationResult<JobPositionDto>.Success(MapPosition(entity));
    }

    /// <summary>
    /// Upsert Custom Role Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PositionRoleDto>> UpsertCustomRoleAsync(UpsertCustomRoleRequest request, CancellationToken cancellationToken)
    {
        var displayName = NormalizeText(request.DisplayName);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return OperationResult<PositionRoleDto>.Failure("角色名称不能为空。");
        }

        var moduleKeys = await NormalizeModuleKeysAsync(request.ModuleKeys, cancellationToken);
        var permissions = NormalizePermissions(request.Permissions);
        if (permissions.Count == 0)
        {
            return OperationResult<PositionRoleDto>.Failure("自定义角色至少需要一个权限。");
        }

        var role = request.Id is null
            ? null
            : await dbContext.Roles.Include(x => x.ModuleAccesses).FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken);

        if (role is not null && PlatformRoleCatalog.IsSystemRole(role.Key))
        {
            return OperationResult<PositionRoleDto>.Failure("内置系统角色不能在岗位权限插件中修改。");
        }

        if (role is null)
        {
            role = new AppRole($"{CustomRolePrefix}{Guid.NewGuid():N}", displayName);
            dbContext.Roles.Add(role);
        }
        else
        {
            role.UpdateDisplayName(displayName);
        }

        role.SetModuleAccess(moduleKeys);

        var existingGrants = await dbContext.RolePermissionGrants
            .Where(x => x.RoleId == role.Id)
            .ToListAsync(cancellationToken);
        dbContext.RolePermissionGrants.RemoveRange(existingGrants);
        dbContext.RolePermissionGrants.AddRange(permissions.Select(permission => new RolePermissionGrant(role.Id, permission)));

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("PositionPermissions", "CustomRoleUpserted", currentUser.GetActor(), displayName, cancellationToken);
        return OperationResult<PositionRoleDto>.Success(MapRole(role, new Dictionary<Guid, List<string>> { [role.Id] = permissions }));
    }

    /// <summary>
    /// Upsert Permission Package Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PermissionPackageDto>> UpsertPermissionPackageAsync(UpsertPermissionPackageRequest request, CancellationToken cancellationToken)
    {
        var displayName = NormalizeText(request.DisplayName);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return OperationResult<PermissionPackageDto>.Failure("权限包名称不能为空。");
        }

        var moduleKeys = await NormalizeModuleKeysAsync(request.ModuleKeys, cancellationToken);
        var permissions = NormalizePermissions(request.Permissions);
        if (permissions.Count == 0)
        {
            return OperationResult<PermissionPackageDto>.Failure("权限包至少需要一个权限。");
        }

        var entity = request.Id is null
            ? null
            : await dbContext.PermissionPackages.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken);

        if (entity is null)
        {
            entity = new PermissionPackage(
                $"{PackagePrefix}{Guid.NewGuid():N}",
                displayName,
                NormalizeText(request.Description),
                Serialize(moduleKeys),
                Serialize(permissions),
                request.IsEnabled);
            dbContext.PermissionPackages.Add(entity);
        }
        else
        {
            entity.Update(displayName, NormalizeText(request.Description), Serialize(moduleKeys), Serialize(permissions), request.IsEnabled);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("PositionPermissions", "PermissionPackageUpserted", currentUser.GetActor(), displayName, cancellationToken);
        return OperationResult<PermissionPackageDto>.Success(MapPackage(entity));
    }

    /// <summary>
    /// 更新Position Role Bindings。
    /// </summary>
    /// <param name="positionId">position Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<IReadOnlyList<PositionRoleBindingDto>>> UpdatePositionRoleBindingsAsync(Guid positionId, UpdatePositionRoleBindingsRequest request, CancellationToken cancellationToken)
    {
        var position = await dbContext.JobPositions.FirstOrDefaultAsync(x => x.Id == positionId, cancellationToken);
        if (position is null)
        {
            return OperationResult<IReadOnlyList<PositionRoleBindingDto>>.Failure("岗位不存在。");
        }

        var roleIds = request.RoleIds.Distinct().ToList();
        var roles = await dbContext.Roles
            .Where(x => roleIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        if (roles.Count != roleIds.Count)
        {
            return OperationResult<IReadOnlyList<PositionRoleBindingDto>>.Failure("包含不存在的角色。");
        }

        var existing = await dbContext.PositionRoleBindings
            .Where(x => x.PositionId == positionId)
            .ToListAsync(cancellationToken);
        dbContext.PositionRoleBindings.RemoveRange(existing);
        dbContext.PositionRoleBindings.AddRange(roleIds.Select(roleId => new PositionRoleBinding(positionId, roleId)));

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("PositionPermissions", "PositionRoleBindingsUpdated", currentUser.GetActor(), position.Name, cancellationToken);

        var positionNames = new Dictionary<Guid, string> { [position.Id] = position.Name };
        var roleNames = roles.ToDictionary(x => x.Id, x => x.DisplayName);
        var bindings = await dbContext.PositionRoleBindings
            .AsNoTracking()
            .Where(x => x.PositionId == positionId)
            .ToListAsync(cancellationToken);
        return OperationResult<IReadOnlyList<PositionRoleBindingDto>>.Success(bindings.Select(x => MapBinding(x, positionNames, roleNames)).ToList());
    }

    /// <summary>
    /// 更新Position Data Scope Rules。
    /// </summary>
    /// <param name="positionId">position Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<IReadOnlyList<PositionDataScopeRuleDto>>> UpdatePositionDataScopeRulesAsync(Guid positionId, UpdatePositionDataScopeRulesRequest request, CancellationToken cancellationToken)
    {
        var position = await dbContext.JobPositions.FirstOrDefaultAsync(x => x.Id == positionId, cancellationToken);
        if (position is null)
        {
            return OperationResult<IReadOnlyList<PositionDataScopeRuleDto>>.Failure("岗位不存在。");
        }

        var incoming = request.Rules
            .Select(x => new
            {
                ScopeType = NormalizeCode(x.ScopeType),
                MatchValue = NormalizeText(x.MatchValue),
                Description = NormalizeText(x.Description),
                x.IsEnabled
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.ScopeType))
            .GroupBy(x => x.ScopeType, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList();

        var existing = await dbContext.PositionDataScopeRules
            .Where(x => x.PositionId == positionId)
            .ToListAsync(cancellationToken);
        dbContext.PositionDataScopeRules.RemoveRange(existing);
        dbContext.PositionDataScopeRules.AddRange(incoming.Select(rule =>
            new PositionDataScopeRule(positionId, rule.ScopeType, rule.MatchValue, rule.Description, rule.IsEnabled)));

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("PositionPermissions", "PositionDataScopesUpdated", currentUser.GetActor(), position.Name, cancellationToken);

        var positionNames = new Dictionary<Guid, string> { [position.Id] = position.Name };
        var rules = await dbContext.PositionDataScopeRules
            .AsNoTracking()
            .Where(x => x.PositionId == positionId)
            .ToListAsync(cancellationToken);
        return OperationResult<IReadOnlyList<PositionDataScopeRuleDto>>.Success(rules.Select(x => MapDataScope(x, positionNames)).ToList());
    }

    /// <summary>
    /// 注册Department 路由。
    /// </summary>
    /// <param name="department">部门对象。</param>
    private static DepartmentDto MapDepartment(PositionDepartment department) =>
        new(department.Id, department.Code, department.Name, department.ParentDepartmentId, department.IsEnabled, department.UpdatedAtUtc);

    /// <summary>
    /// 注册Position 路由。
    /// </summary>
    /// <param name="position">岗位对象。</param>
    private static JobPositionDto MapPosition(JobPosition position) =>
        new(position.Id, position.Code, position.Name, position.DepartmentId, position.DepartmentName, position.Description, position.IsEnabled, position.UpdatedAtUtc);

    /// <summary>
    /// 注册Package 路由。
    /// </summary>
    /// <param name="package">权限包。</param>
    private static PermissionPackageDto MapPackage(PermissionPackage package) =>
        new(package.Id, package.DisplayName, package.Description, Deserialize(package.ModuleKeys), Deserialize(package.Permissions), package.IsEnabled, package.UpdatedAtUtc);

    /// <summary>
    /// 注册Role 路由。
    /// </summary>
    /// <param name="role">角色实体。</param>
    /// <param name="grantsByRole">grants By Role 参数。</param>
    private static PositionRoleDto MapRole(AppRole role, IReadOnlyDictionary<Guid, List<string>> grantsByRole)
    {
        var permissions = PlatformRoleCatalog.ResolvePermissions([role.Key])
            .Concat(grantsByRole.GetValueOrDefault(role.Id) ?? [])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        return new PositionRoleDto(
            role.Id,
            role.DisplayName,
            PlatformRoleCatalog.IsSystemRole(role.Key),
            role.ModuleAccesses.Select(x => x.ModuleKey).OrderBy(x => x).ToList(),
            permissions);
    }

    /// <summary>
    /// 注册Binding 路由。
    /// </summary>
    /// <param name="binding">绑定关系。</param>
    /// <param name="positionNames">position Names 参数。</param>
    /// <param name="roleNames">role Names 参数。</param>
    private static PositionRoleBindingDto MapBinding(PositionRoleBinding binding, IReadOnlyDictionary<Guid, string> positionNames, IReadOnlyDictionary<Guid, string> roleNames) =>
        new(
            binding.Id,
            binding.PositionId,
            binding.RoleId,
            positionNames.GetValueOrDefault(binding.PositionId, "未知岗位"),
            roleNames.GetValueOrDefault(binding.RoleId, "未知角色"));

    /// <summary>
    /// 注册Data Scope 路由。
    /// </summary>
    /// <param name="rule">规则对象。</param>
    /// <param name="positionNames">position Names 参数。</param>
    private static PositionDataScopeRuleDto MapDataScope(PositionDataScopeRule rule, IReadOnlyDictionary<Guid, string> positionNames) =>
        new(
            rule.Id,
            rule.PositionId,
            positionNames.GetValueOrDefault(rule.PositionId, "未知岗位"),
            rule.ScopeType,
            rule.MatchValue,
            rule.Description,
            rule.IsEnabled);

    /// <summary>
    /// Normalize Code。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeCode(string value) => NormalizeText(value).ToLowerInvariant();

    /// <summary>
    /// Normalize Text。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeText(string value) => value?.Trim() ?? string.Empty;

    /// <summary>
    /// Serialize。
    /// </summary>
    /// <param name="values">数值或配置值集合。</param>
    private static string Serialize(IEnumerable<string> values) => string.Join('\n', values);

    /// <summary>
    /// Deserialize。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static IReadOnlyList<string> Deserialize(string value) =>
        value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// Normalize Module Keys Async。
    /// </summary>
    /// <param name="moduleKeys">module Keys 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<List<string>> NormalizeModuleKeysAsync(IEnumerable<string> moduleKeys, CancellationToken cancellationToken)
    {
        var requested = moduleKeys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (requested.Count == 0)
        {
            return [];
        }

        var existing = await dbContext.PluginModules
            .Where(x => requested.Contains(x.Key))
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);
        return existing.OrderBy(x => x).ToList();
    }

    /// <summary>
    /// Normalize Permissions。
    /// </summary>
    /// <param name="permissions">权限编码集合。</param>
    private static List<string> NormalizePermissions(IEnumerable<string> permissions)
    {
        var allowed = PlatformPermissions.All.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return permissions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeText)
            .Where(allowed.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }

    /// <summary>
    /// Resolve Permission Module Key。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    private static string ResolvePermissionModuleKey(string permission)
    {
        if (permission.StartsWith("master-data.", StringComparison.OrdinalIgnoreCase))
        {
            return "master-data";
        }

        if (permission.StartsWith("procurement.", StringComparison.OrdinalIgnoreCase))
        {
            return "procurement";
        }

        if (permission.StartsWith("sales.", StringComparison.OrdinalIgnoreCase))
        {
            return "sales";
        }

        if (permission.StartsWith("inventory.", StringComparison.OrdinalIgnoreCase) || permission.StartsWith("barcode.", StringComparison.OrdinalIgnoreCase))
        {
            return "inventory";
        }

        if (permission.StartsWith("wms.", StringComparison.OrdinalIgnoreCase))
        {
            return "wms";
        }

        if (permission.StartsWith("finance.", StringComparison.OrdinalIgnoreCase))
        {
            return "finance";
        }

        if (permission.StartsWith("workflow.", StringComparison.OrdinalIgnoreCase) || permission.StartsWith("notification.", StringComparison.OrdinalIgnoreCase))
        {
            return "workflow";
        }

        if (permission.StartsWith("control.", StringComparison.OrdinalIgnoreCase))
        {
            return "control";
        }

        if (permission.StartsWith("localization.", StringComparison.OrdinalIgnoreCase))
        {
            return "localization";
        }

        if (permission.StartsWith("manufacturing.", StringComparison.OrdinalIgnoreCase) || permission.StartsWith("outsourcing.", StringComparison.OrdinalIgnoreCase))
        {
            return "manufacturing";
        }

        if (permission.StartsWith("quality.", StringComparison.OrdinalIgnoreCase))
        {
            return "quality";
        }

        if (permission.StartsWith("planning.", StringComparison.OrdinalIgnoreCase))
        {
            return "planning";
        }

        if (permission.StartsWith("position-permissions.", StringComparison.OrdinalIgnoreCase) || permission.StartsWith("identity.", StringComparison.OrdinalIgnoreCase))
        {
            return "position-permissions";
        }

        return "platform";
    }
}
