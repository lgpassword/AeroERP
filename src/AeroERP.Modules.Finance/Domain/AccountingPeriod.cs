using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// Accounting Period 业务对象。
/// </summary>
public sealed class AccountingPeriod : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Accounting Period实例。
    /// </summary>
    private AccountingPeriod()
    {
    }

    /// <summary>
    /// 初始化Accounting Period实例。
    /// </summary>
    /// <param name="year">会计年度。</param>
    /// <param name="month">会计月份。</param>
    /// <param name="createdBy">创建人。</param>
    public AccountingPeriod(int year, int month, string createdBy)
    {
        Year = year;
        Month = month;
        Name = $"{year:D4}-{month:D2}";
        StartDate = new DateOnly(year, month, 1);
        EndDate = StartDate.AddMonths(1).AddDays(-1);
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Year。
    /// </summary>
    public int Year { get; private set; }
    /// <summary>
    /// Month。
    /// </summary>
    public int Month { get; private set; }
    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// Start Date。
    /// </summary>
    public DateOnly StartDate { get; private set; }
    /// <summary>
    /// End Date。
    /// </summary>
    public DateOnly EndDate { get; private set; }
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = AccountingPeriodStatus.Open;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Closed By。
    /// </summary>
    public string ClosedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Closed At Utc。
    /// </summary>
    public DateTimeOffset? ClosedAtUtc { get; private set; }

    /// <summary>
    /// Close。
    /// </summary>
    /// <param name="actor">操作人。</param>
    public void Close(string actor)
    {
        if (string.Equals(Status, AccountingPeriodStatus.Closed, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("会计期间已经关闭。");
        }

        Status = AccountingPeriodStatus.Closed;
        ClosedBy = actor;
        ClosedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Reopen。
    /// </summary>
    public void Reopen()
    {
        if (string.Equals(Status, AccountingPeriodStatus.Open, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("会计期间已经打开。");
        }

        Status = AccountingPeriodStatus.Open;
        ClosedBy = string.Empty;
        ClosedAtUtc = null;
        Touch();
    }
}
