using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// Accounting Account 业务对象。
/// </summary>
public sealed class AccountingAccount : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Accounting Account实例。
    /// </summary>
    private AccountingAccount()
    {
    }

    /// <summary>
    /// 初始化Accounting Account实例。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="type">业务类型。</param>
    /// <param name="parentAccountId">parent Account Id 参数。</param>
    /// <param name="isActive">is Active 参数。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public AccountingAccount(
        string code,
        string name,
        string type,
        Guid? parentAccountId,
        bool isActive,
        string updatedBy)
    {
        Code = code;
        Name = name;
        Type = type;
        ParentAccountId = parentAccountId;
        IsActive = isActive;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// 业务编码。
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// Type。
    /// </summary>
    public string Type { get; private set; } = AccountingAccountType.Asset;
    /// <summary>
    /// Parent Account Id。
    /// </summary>
    public Guid? ParentAccountId { get; private set; }
    /// <summary>
    /// Is Active。
    /// </summary>
    public bool IsActive { get; private set; } = true;
    /// <summary>
    /// 最后更新人。
    /// </summary>
    public string UpdatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="type">业务类型。</param>
    /// <param name="parentAccountId">parent Account Id 参数。</param>
    /// <param name="isActive">is Active 参数。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string code, string name, string type, Guid? parentAccountId, bool isActive, string updatedBy)
    {
        Code = code;
        Name = name;
        Type = type;
        ParentAccountId = parentAccountId;
        IsActive = isActive;
        UpdatedBy = updatedBy;
        Touch();
    }
}
