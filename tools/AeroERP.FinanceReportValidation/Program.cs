using AeroERP.Modules.Finance.Contracts;
using AeroERP.Modules.Finance.Domain;
using AeroERP.Modules.Finance.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<AeroErpDbContext>()
    .UseInMemoryDatabase($"aeroerp-finance-report-validation-{Guid.NewGuid():N}")
    .Options;

await using var dbContext = new AeroErpDbContext(options);
var finance = new FinanceService(dbContext, new NoOpAuditWriter(), new ValidationUserAccessor());

var assetAccount = await RequireAsync(finance.UpsertAccountingAccountAsync(
    new UpsertAccountingAccountRequest(null, "18G-A100", "18G validation asset", AccountingAccountType.Asset, null, true),
    CancellationToken.None));
var revenueAccount = await RequireAsync(finance.UpsertAccountingAccountAsync(
    new UpsertAccountingAccountRequest(null, "18G-R100", "18G validation revenue", AccountingAccountType.Revenue, null, true),
    CancellationToken.None));
var period = await RequireAsync(finance.CreateAccountingPeriodAsync(
    new CreateAccountingPeriodRequest(2096, 7),
    CancellationToken.None));

var approvedVoucher = await RequireAsync(finance.CreateManualVoucherAsync(
    new CreateManualVoucherRequest(
        period.Id,
        new DateOnly(2096, 7, 15),
        "18G approved report voucher",
        [
            new CreateManualVoucherLineRequest(assetAccount.Id, "asset debit", 100m, 0m),
            new CreateManualVoucherLineRequest(revenueAccount.Id, "revenue credit", 0m, 100m)
        ]),
    CancellationToken.None));
await RequireAsync(finance.SubmitVoucherAsync(approvedVoucher.Id, CancellationToken.None));
await RequireAsync(finance.ApproveVoucherAsync(
    approvedVoucher.Id,
    new ReviewVoucherRequest("18G validation approval"),
    CancellationToken.None));

await RequireAsync(finance.CreateManualVoucherAsync(
    new CreateManualVoucherRequest(
        period.Id,
        new DateOnly(2096, 7, 16),
        "18G draft voucher ignored by reports",
        [
            new CreateManualVoucherLineRequest(assetAccount.Id, "draft asset debit", 40m, 0m),
            new CreateManualVoucherLineRequest(revenueAccount.Id, "draft revenue credit", 0m, 40m)
        ]),
    CancellationToken.None));

var report = await RequireAsync(finance.GetFinanceReportSnapshotAsync(period.Id, CancellationToken.None));

AssertEqual(period.Id, report.AccountingPeriodId, "accountingPeriodId");
AssertEqual(1, report.ApprovedVoucherCount, "approvedVoucherCount");
AssertEqual(100m, report.TotalDebit, "totalDebit");
AssertEqual(100m, report.TotalCredit, "totalCredit");
AssertTrue(report.IsBalanced, "trial balance should be balanced");

var assetLine = report.TrialBalance.SingleOrDefault(x => x.AccountCode == assetAccount.Code);
var revenueLine = report.TrialBalance.SingleOrDefault(x => x.AccountCode == revenueAccount.Code);

AssertTrue(assetLine is not null, "asset account should appear in trial balance");
AssertEqual(100m, assetLine!.DebitAmount, "asset debitAmount");
AssertEqual(0m, assetLine.CreditAmount, "asset creditAmount");
AssertEqual(100m, assetLine.EndingDebit, "asset endingDebit");
AssertEqual(0m, assetLine.EndingCredit, "asset endingCredit");

AssertTrue(revenueLine is not null, "revenue account should appear in trial balance");
AssertEqual(0m, revenueLine!.DebitAmount, "revenue debitAmount");
AssertEqual(100m, revenueLine.CreditAmount, "revenue creditAmount");
AssertEqual(0m, revenueLine.EndingDebit, "revenue endingDebit");
AssertEqual(100m, revenueLine.EndingCredit, "revenue endingCredit");

AssertEqual(100m, report.IncomeStatement.Revenue, "income revenue");
AssertEqual(0m, report.IncomeStatement.Cost, "income cost");
AssertEqual(0m, report.IncomeStatement.Expense, "income expense");
AssertEqual(100m, report.IncomeStatement.Profit, "income profit");

AssertEqual(100m, report.BalanceSheet.Assets, "balance assets");
AssertEqual(0m, report.BalanceSheet.Liabilities, "balance liabilities");
AssertEqual(0m, report.BalanceSheet.Equity, "balance equity");
AssertEqual(100m, report.BalanceSheet.RetainedEarnings, "balance retainedEarnings");
AssertEqual(100m, report.BalanceSheet.TotalLiabilitiesAndEquity, "balance totalLiabilitiesAndEquity");
AssertEqual(0m, report.BalanceSheet.Difference, "balance difference");

Console.WriteLine("Finance report validation passed.");
Console.WriteLine($"Approved vouchers: {report.ApprovedVoucherCount}");
Console.WriteLine($"Trial balance debit/credit: {report.TotalDebit}/{report.TotalCredit}");
Console.WriteLine($"Profit: {report.IncomeStatement.Profit}");
Console.WriteLine($"Balance sheet difference: {report.BalanceSheet.Difference}");

static async Task<T> RequireAsync<T>(Task<AeroERP.BuildingBlocks.Results.OperationResult<T>> task)
{
    var result = await task;
    if (!result.IsSuccess || result.Value is null)
    {
        throw new InvalidOperationException(result.Error ?? "Operation failed.");
    }

    return result.Value;
}

static void AssertEqual<T>(T expected, T actual, string name)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected {name} to be {expected}, got {actual}.");
    }
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

sealed class NoOpAuditWriter : IAuditWriter
{
    /// <summary>
    /// Write Async。
    /// </summary>
    /// <param name="category">业务分类。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="detail">详细说明。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public Task WriteAsync(string category, string action, string actor, string detail, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

sealed class ValidationUserAccessor : ICurrentUserAccessor
{
    public bool IsAuthenticated => true;
    /// <summary>
    /// User Id。
    /// </summary>
    public Guid? UserId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public string UserName => "finance-report-validation";
    public string DisplayName => "Finance Report Validation";
    public IReadOnlyList<string> Roles => ["platform-admin"];
    public IReadOnlyList<string> Permissions => ["finance.read", "finance.accounting.manage", "finance.voucher.manage", "finance.voucher.review"];
    /// <summary>
    /// 判断是否存在Role。
    /// </summary>
    /// <param name="roleKey">role Key 参数。</param>
    public bool HasRole(string roleKey) => Roles.Contains(roleKey);
    /// <summary>
    /// 判断是否存在Permission。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    public bool HasPermission(string permission) => Permissions.Contains(permission);
    /// <summary>
    /// 判断是否允许Access Module。
    /// </summary>
    /// <param name="moduleKey">模块键。</param>
    public bool CanAccessModule(string moduleKey) => string.Equals(moduleKey, "finance", StringComparison.Ordinal);
}
