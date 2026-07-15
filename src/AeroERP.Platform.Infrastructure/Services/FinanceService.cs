using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Finance.Contracts;
using AeroERP.Modules.Finance.Domain;
using AeroERP.Modules.Finance.Services;
using AeroERP.Modules.Inventory.Domain;
using AeroERP.Modules.Procurement.Domain;
using AeroERP.Modules.Sales.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Modules.Finance.Services;

/// <summary>
/// Finance Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class FinanceService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IFinanceService
{
    /// <summary>
    /// Default Payment Term Days。
    /// </summary>
    private const int DefaultPaymentTermDays = 30;

    /// <summary>
    /// Business Voucher Source 数据记录。
    /// </summary>
    private sealed record BusinessVoucherSource(
        string SourceType,
        Guid SourceId,
        string SourceNo,
        decimal Amount,
        string DefaultSummary,
        string DebitSummary,
        string CreditSummary);

    /// <summary>
    /// Aging Source Entry 数据记录。
    /// </summary>
    private sealed record AgingSourceEntry(
        Guid Id,
        string DocumentNo,
        string CounterpartyName,
        string SourceNo,
        decimal Amount,
        decimal SettledAmount,
        decimal RemainingAmount,
        string CurrencyCode,
        DateOnly DueDate,
        string Status);

    /// <summary>
    /// Tax Snapshot 数据记录。
    /// </summary>
    private sealed record TaxSnapshot(
        string TaxInvoiceType,
        decimal TaxRate);

    /// <summary>
    /// 查询Accounting Accounts。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<AccountingAccountDto>> ListAccountingAccountsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.AccountingAccounts
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
        var accountMap = entities.ToDictionary(x => x.Id);

        return entities
            .Select(entity => MapAccountingAccount(entity, accountMap))
            .ToList();
    }

    /// <summary>
    /// Upsert Accounting Account Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<AccountingAccountDto>> UpsertAccountingAccountAsync(
        UpsertAccountingAccountRequest request,
        CancellationToken cancellationToken)
    {
        var code = NormalizeAccountCode(request.Code);
        var name = NormalizeText(request.Name);
        var type = NormalizeAccountType(request.Type);

        if (string.IsNullOrWhiteSpace(code))
        {
            return OperationResult<AccountingAccountDto>.Failure("会计科目编码不能为空。");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<AccountingAccountDto>.Failure("会计科目名称不能为空。");
        }

        if (!AccountingAccountType.IsValid(type))
        {
            return OperationResult<AccountingAccountDto>.Failure("会计科目类型无效。");
        }

        if (request.ParentAccountId is { } parentId)
        {
            if (request.Id == parentId)
            {
                return OperationResult<AccountingAccountDto>.Failure("上级科目不能指向自身。");
            }

            var parentExists = await dbContext.AccountingAccounts.AnyAsync(x => x.Id == parentId, cancellationToken);
            if (!parentExists)
            {
                return OperationResult<AccountingAccountDto>.Failure("未找到上级会计科目。");
            }
        }

        var duplicate = await dbContext.AccountingAccounts
            .AnyAsync(x => x.Code == code && (!request.Id.HasValue || x.Id != request.Id.Value), cancellationToken);
        if (duplicate)
        {
            return OperationResult<AccountingAccountDto>.Failure("会计科目编码已经存在。");
        }

        var actor = currentUser.GetActor();
        AccountingAccount entity;
        if (request.Id.HasValue)
        {
            var existing = await dbContext.AccountingAccounts.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken);
            if (existing is null)
            {
                return OperationResult<AccountingAccountDto>.Failure("未找到会计科目。");
            }

            entity = existing;
            entity.Update(code, name, type, request.ParentAccountId, request.IsActive, actor);
        }
        else
        {
            entity = new AccountingAccount(code, name, type, request.ParentAccountId, request.IsActive, actor);
            dbContext.AccountingAccounts.Add(entity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "AccountingAccountUpserted", actor, $"{entity.Code}:{entity.Name}", cancellationToken);

        var accounts = await dbContext.AccountingAccounts.AsNoTracking().ToListAsync(cancellationToken);
        return OperationResult<AccountingAccountDto>.Success(MapAccountingAccount(entity, accounts.ToDictionary(x => x.Id)));
    }

    /// <summary>
    /// 查询Accounting Periods。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<AccountingPeriodDto>> ListAccountingPeriodsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.AccountingPeriods
            .AsNoTracking()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync(cancellationToken);

        return entities.Select(MapAccountingPeriod).ToList();
    }

    /// <summary>
    /// 创建Accounting Period。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<AccountingPeriodDto>> CreateAccountingPeriodAsync(
        CreateAccountingPeriodRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Year is < 2000 or > 2100 || request.Month is < 1 or > 12)
        {
            return OperationResult<AccountingPeriodDto>.Failure("会计期间年月无效。");
        }

        var exists = await dbContext.AccountingPeriods
            .AnyAsync(x => x.Year == request.Year && x.Month == request.Month, cancellationToken);
        if (exists)
        {
            return OperationResult<AccountingPeriodDto>.Failure("会计期间已经存在。");
        }

        var actor = currentUser.GetActor();
        var period = new AccountingPeriod(request.Year, request.Month, actor);
        dbContext.AccountingPeriods.Add(period);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "AccountingPeriodCreated", actor, period.Name, cancellationToken);

        return OperationResult<AccountingPeriodDto>.Success(MapAccountingPeriod(period));
    }

    /// <summary>
    /// Close Accounting Period Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<AccountingPeriodDto>> CloseAccountingPeriodAsync(Guid id, CancellationToken cancellationToken)
    {
        var period = await dbContext.AccountingPeriods.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (period is null)
        {
            return OperationResult<AccountingPeriodDto>.Failure("未找到会计期间。");
        }

        var pendingVoucherCount = await dbContext.GeneralLedgerVouchers.CountAsync(
            x => x.AccountingPeriodId == period.Id &&
                 (x.Status == GeneralLedgerVoucherStatus.Draft || x.Status == GeneralLedgerVoucherStatus.Submitted),
            cancellationToken);
        if (pendingVoucherCount > 0)
        {
            return OperationResult<AccountingPeriodDto>.Failure($"会计期间存在 {pendingVoucherCount} 张草稿或待审凭证，不能关账。");
        }

        var actor = currentUser.GetActor();
        try
        {
            period.Close(actor);
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<AccountingPeriodDto>.Failure(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "AccountingPeriodClosed", actor, period.Name, cancellationToken);
        return OperationResult<AccountingPeriodDto>.Success(MapAccountingPeriod(period));
    }

    /// <summary>
    /// Reopen Accounting Period Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<AccountingPeriodDto>> ReopenAccountingPeriodAsync(Guid id, CancellationToken cancellationToken)
    {
        var period = await dbContext.AccountingPeriods.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (period is null)
        {
            return OperationResult<AccountingPeriodDto>.Failure("未找到会计期间。");
        }

        try
        {
            period.Reopen();
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<AccountingPeriodDto>.Failure(ex.Message);
        }

        var actor = currentUser.GetActor();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "AccountingPeriodReopened", actor, period.Name, cancellationToken);
        return OperationResult<AccountingPeriodDto>.Success(MapAccountingPeriod(period));
    }

    /// <summary>
    /// 查询General Ledger Vouchers。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<GeneralLedgerVoucherDto>> ListGeneralLedgerVouchersAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.GeneralLedgerVouchers
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(MapGeneralLedgerVoucher)
            .ToList();
    }

    /// <summary>
    /// 创建Manual Voucher。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<GeneralLedgerVoucherDto>> CreateManualVoucherAsync(
        CreateManualVoucherRequest request,
        CancellationToken cancellationToken)
    {
        var summary = NormalizeText(request.Summary);
        if (string.IsNullOrWhiteSpace(summary))
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("凭证摘要不能为空。");
        }

        var period = await dbContext.AccountingPeriods.FirstOrDefaultAsync(x => x.Id == request.AccountingPeriodId, cancellationToken);
        if (period is null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("未找到会计期间。");
        }

        var periodError = ValidateOpenPeriod(period, request.DocumentDate);
        if (periodError is not null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(periodError);
        }

        var linesResult = await BuildVoucherLinesAsync(request.Lines, summary, cancellationToken);
        if (!linesResult.IsSuccess)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(linesResult.Error!);
        }

        var actor = currentUser.GetActor();
        var voucher = new GeneralLedgerVoucher(
            NextNo("GL"),
            period.Id,
            period.Name,
            request.DocumentDate,
            summary,
            GeneralLedgerVoucherSourceType.Manual,
            null,
            string.Empty,
            actor,
            linesResult.Value!);

        dbContext.GeneralLedgerVouchers.Add(voucher);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "GeneralLedgerVoucherCreated", actor, voucher.VoucherNo, cancellationToken);
        return OperationResult<GeneralLedgerVoucherDto>.Success(MapGeneralLedgerVoucher(voucher));
    }

    /// <summary>
    /// 创建Business Voucher。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<GeneralLedgerVoucherDto>> CreateBusinessVoucherAsync(
        CreateBusinessVoucherRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DebitAccountId == request.CreditAccountId)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("借方科目和贷方科目不能相同。");
        }

        var period = await dbContext.AccountingPeriods.FirstOrDefaultAsync(x => x.Id == request.AccountingPeriodId, cancellationToken);
        if (period is null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("未找到会计期间。");
        }

        var periodError = ValidateOpenPeriod(period, request.DocumentDate);
        if (periodError is not null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(periodError);
        }

        var sourceResult = await ResolveBusinessVoucherSourceAsync(request.SourceType, request.SourceId, cancellationToken);
        if (!sourceResult.IsSuccess)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(sourceResult.Error!);
        }

        var source = sourceResult.Value!;
        var exists = await dbContext.GeneralLedgerVouchers.AnyAsync(
            x => x.SourceType == source.SourceType && x.SourceId == source.SourceId,
            cancellationToken);
        if (exists)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("该业务单据已经生成总账凭证。");
        }

        var summary = NormalizeText(request.Summary);
        if (string.IsNullOrWhiteSpace(summary))
        {
            summary = source.DefaultSummary;
        }

        var linesResult = await BuildVoucherLinesAsync(
            [
                new CreateManualVoucherLineRequest(request.DebitAccountId, source.DebitSummary, source.Amount, 0m),
                new CreateManualVoucherLineRequest(request.CreditAccountId, source.CreditSummary, 0m, source.Amount)
            ],
            summary,
            cancellationToken);
        if (!linesResult.IsSuccess)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(linesResult.Error!);
        }

        var actor = currentUser.GetActor();
        var voucher = new GeneralLedgerVoucher(
            NextNo("GL"),
            period.Id,
            period.Name,
            request.DocumentDate,
            summary,
            source.SourceType,
            source.SourceId,
            source.SourceNo,
            actor,
            linesResult.Value!);

        dbContext.GeneralLedgerVouchers.Add(voucher);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "GeneralLedgerBusinessVoucherCreated", actor, $"{voucher.VoucherNo}:{source.SourceNo}", cancellationToken);
        return OperationResult<GeneralLedgerVoucherDto>.Success(MapGeneralLedgerVoucher(voucher));
    }

    /// <summary>
    /// Submit Voucher Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<GeneralLedgerVoucherDto>> SubmitVoucherAsync(Guid id, CancellationToken cancellationToken)
    {
        var voucher = await dbContext.GeneralLedgerVouchers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (voucher is null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("未找到总账凭证。");
        }

        var period = await dbContext.AccountingPeriods.FirstOrDefaultAsync(x => x.Id == voucher.AccountingPeriodId, cancellationToken);
        if (period is null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("未找到会计期间。");
        }

        var periodError = ValidateOpenPeriod(period, voucher.DocumentDate);
        if (periodError is not null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(periodError);
        }

        var balanceError = ValidateVoucherLines(voucher.Lines);
        if (balanceError is not null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(balanceError);
        }

        var actor = currentUser.GetActor();
        try
        {
            voucher.Submit(actor);
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "GeneralLedgerVoucherSubmitted", actor, voucher.VoucherNo, cancellationToken);
        return OperationResult<GeneralLedgerVoucherDto>.Success(MapGeneralLedgerVoucher(voucher));
    }

    /// <summary>
    /// Approve Voucher Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<GeneralLedgerVoucherDto>> ApproveVoucherAsync(
        Guid id,
        ReviewVoucherRequest request,
        CancellationToken cancellationToken)
    {
        var voucher = await dbContext.GeneralLedgerVouchers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (voucher is null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("未找到总账凭证。");
        }

        var period = await dbContext.AccountingPeriods.FirstOrDefaultAsync(x => x.Id == voucher.AccountingPeriodId, cancellationToken);
        if (period is null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("未找到会计期间。");
        }

        var periodError = ValidateOpenPeriod(period, voucher.DocumentDate);
        if (periodError is not null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(periodError);
        }

        var actor = currentUser.GetActor();
        try
        {
            voucher.Approve(actor, NormalizeText(request.Note));
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "GeneralLedgerVoucherApproved", actor, voucher.VoucherNo, cancellationToken);
        return OperationResult<GeneralLedgerVoucherDto>.Success(MapGeneralLedgerVoucher(voucher));
    }

    /// <summary>
    /// Reject Voucher Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<GeneralLedgerVoucherDto>> RejectVoucherAsync(
        Guid id,
        ReviewVoucherRequest request,
        CancellationToken cancellationToken)
    {
        var voucher = await dbContext.GeneralLedgerVouchers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (voucher is null)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure("未找到总账凭证。");
        }

        var actor = currentUser.GetActor();
        try
        {
            voucher.Reject(actor, NormalizeText(request.Note));
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<GeneralLedgerVoucherDto>.Failure(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "GeneralLedgerVoucherRejected", actor, voucher.VoucherNo, cancellationToken);
        return OperationResult<GeneralLedgerVoucherDto>.Success(MapGeneralLedgerVoucher(voucher));
    }

    /// <summary>
    /// 获取Finance Report Snapshot。
    /// </summary>
    /// <param name="accountingPeriodId">accounting Period Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<FinanceReportSnapshotDto>> GetFinanceReportSnapshotAsync(
        Guid? accountingPeriodId,
        CancellationToken cancellationToken)
    {
        AccountingPeriod? period = null;
        if (accountingPeriodId.HasValue)
        {
            period = await dbContext.AccountingPeriods
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == accountingPeriodId.Value, cancellationToken);
            if (period is null)
            {
                return OperationResult<FinanceReportSnapshotDto>.Failure("未找到会计期间。");
            }
        }

        var vouchersQuery = dbContext.GeneralLedgerVouchers
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.Status == GeneralLedgerVoucherStatus.Approved);

        if (accountingPeriodId.HasValue)
        {
            vouchersQuery = vouchersQuery.Where(x => x.AccountingPeriodId == accountingPeriodId.Value);
        }

        var vouchers = await vouchersQuery.ToListAsync(cancellationToken);
        var accountIds = vouchers
            .SelectMany(x => x.Lines)
            .Select(x => x.AccountingAccountId)
            .Distinct()
            .ToList();
        var accounts = await dbContext.AccountingAccounts
            .AsNoTracking()
            .Where(x => accountIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var trialBalance = vouchers
            .SelectMany(x => x.Lines)
            .GroupBy(x => x.AccountingAccountId)
            .Select(group =>
            {
                var debitAmount = group.Sum(x => x.DebitAmount);
                var creditAmount = group.Sum(x => x.CreditAmount);
                var firstLine = group.First();
                var accountType = accounts.TryGetValue(group.Key, out var account)
                    ? account.Type
                    : string.Empty;
                var netDebit = debitAmount - creditAmount;
                return new TrialBalanceLineDto(
                    group.Key,
                    firstLine.AccountCode,
                    firstLine.AccountName,
                    accountType,
                    debitAmount,
                    creditAmount,
                    netDebit > 0 ? netDebit : 0m,
                    netDebit < 0 ? Math.Abs(netDebit) : 0m);
            })
            .OrderBy(x => x.AccountCode)
            .ToList();

        var totalDebit = trialBalance.Sum(x => x.DebitAmount);
        var totalCredit = trialBalance.Sum(x => x.CreditAmount);
        var revenue = trialBalance
            .Where(x => x.AccountType == AccountingAccountType.Revenue)
            .Sum(x => x.CreditAmount - x.DebitAmount);
        var cost = trialBalance
            .Where(x => x.AccountType == AccountingAccountType.Cost)
            .Sum(x => x.DebitAmount - x.CreditAmount);
        var expense = trialBalance
            .Where(x => x.AccountType == AccountingAccountType.Expense)
            .Sum(x => x.DebitAmount - x.CreditAmount);
        var profit = revenue - cost - expense;

        var assets = trialBalance
            .Where(x => x.AccountType == AccountingAccountType.Asset)
            .Sum(x => x.DebitAmount - x.CreditAmount);
        var liabilities = trialBalance
            .Where(x => x.AccountType == AccountingAccountType.Liability)
            .Sum(x => x.CreditAmount - x.DebitAmount);
        var equity = trialBalance
            .Where(x => x.AccountType == AccountingAccountType.Equity)
            .Sum(x => x.CreditAmount - x.DebitAmount);
        var totalLiabilitiesAndEquity = liabilities + equity + profit;

        var snapshot = new FinanceReportSnapshotDto(
            period?.Id,
            period?.Name ?? "全部期间",
            period?.StartDate,
            period?.EndDate,
            vouchers.Count,
            totalDebit,
            totalCredit,
            totalDebit == totalCredit,
            trialBalance,
            new IncomeStatementDto(revenue, cost, expense, profit),
            new BalanceSheetDto(
                assets,
                liabilities,
                equity,
                profit,
                totalLiabilitiesAndEquity,
                assets - totalLiabilitiesAndEquity));

        return OperationResult<FinanceReportSnapshotDto>.Success(snapshot);
    }

    /// <summary>
    /// 查询Payables。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<PayableDto>> ListPayablesAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Payables.ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => MapPayable(x, CurrentBusinessDate()))
            .ToList();
    }

    /// <summary>
    /// 查询Receivables。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<ReceivableDto>> ListReceivablesAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Receivables.ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => MapReceivable(x, CurrentBusinessDate()))
            .ToList();
    }

    /// <summary>
    /// 获取Aging Snapshot。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<FinanceAgingSnapshotDto> GetAgingSnapshotAsync(CancellationToken cancellationToken)
    {
        var asOfDate = CurrentBusinessDate();
        var payables = await dbContext.Payables
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var receivables = await dbContext.Receivables
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new FinanceAgingSnapshotDto(
            asOfDate,
            BuildAgingSide(payables
                .Where(x => x.RemainingAmount > 0)
                .Select(x => new AgingSourceEntry(
                    x.Id,
                    x.PayableNo,
                    x.SupplierName,
                    string.IsNullOrWhiteSpace(x.InventoryReceiptNo) ? x.ProcurementOrderNo : x.InventoryReceiptNo,
                    x.Amount,
                    x.SettledAmount,
                    x.RemainingAmount,
                    x.CurrencyCode,
                    ResolveDueDate(x.DueDate, x.CreatedAtUtc),
                    x.Status)),
                asOfDate),
            BuildAgingSide(receivables
                .Where(x => x.RemainingAmount > 0)
                .Select(x => new AgingSourceEntry(
                    x.Id,
                    x.ReceivableNo,
                    x.CustomerName,
                    string.IsNullOrWhiteSpace(x.InventoryIssueNo) ? x.SalesOrderNo : x.InventoryIssueNo,
                    x.Amount,
                    x.SettledAmount,
                    x.RemainingAmount,
                    x.CurrencyCode,
                    ResolveDueDate(x.DueDate, x.CreatedAtUtc),
                    x.Status)),
                asOfDate));
    }

    /// <summary>
    /// 查询Finance Invoices。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<FinanceInvoiceDto>> ListFinanceInvoicesAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.FinanceInvoices
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapFinanceInvoice)
            .ToList();
    }

    /// <summary>
    /// 创建Finance Invoice。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<FinanceInvoiceDto>> CreateFinanceInvoiceAsync(
        CreateFinanceInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var direction = NormalizeInvoiceDirection(request.Direction);
        if (string.IsNullOrWhiteSpace(direction))
        {
            return OperationResult<FinanceInvoiceDto>.Failure("发票方向无效。");
        }

        if (request.SourceId == Guid.Empty)
        {
            return OperationResult<FinanceInvoiceDto>.Failure("发票来源不能为空。");
        }

        var exists = await dbContext.FinanceInvoices.AnyAsync(
            x => x.Direction == direction && x.SourceId == request.SourceId,
            cancellationToken);
        if (exists)
        {
            return OperationResult<FinanceInvoiceDto>.Failure("该来源已经登记发票。");
        }

        var actor = currentUser.GetActor();
        FinanceInvoice invoice;
        if (direction == FinanceInvoiceDirection.Payable)
        {
            var payable = await dbContext.Payables.FirstOrDefaultAsync(x => x.Id == request.SourceId, cancellationToken);
            if (payable is null)
            {
                return OperationResult<FinanceInvoiceDto>.Failure("未找到应付记录。");
            }

            invoice = new FinanceInvoice(
                NextNo("INV"),
                direction,
                payable.Id,
                payable.PayableNo,
                payable.SupplierName,
                payable.TaxInvoiceType,
                payable.TaxRate,
                payable.Amount,
                payable.NetAmount,
                payable.TaxAmount,
                payable.CurrencyCode,
                request.InvoiceDate,
                NormalizeText(request.Note),
                actor);
        }
        else
        {
            var receivable = await dbContext.Receivables.FirstOrDefaultAsync(x => x.Id == request.SourceId, cancellationToken);
            if (receivable is null)
            {
                return OperationResult<FinanceInvoiceDto>.Failure("未找到应收记录。");
            }

            invoice = new FinanceInvoice(
                NextNo("INV"),
                direction,
                receivable.Id,
                receivable.ReceivableNo,
                receivable.CustomerName,
                receivable.TaxInvoiceType,
                receivable.TaxRate,
                receivable.Amount,
                receivable.NetAmount,
                receivable.TaxAmount,
                receivable.CurrencyCode,
                request.InvoiceDate,
                NormalizeText(request.Note),
                actor);
        }

        dbContext.FinanceInvoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "FinanceInvoiceCreated", actor, $"{invoice.InvoiceNo}:{invoice.SourceNo}", cancellationToken);
        return OperationResult<FinanceInvoiceDto>.Success(MapFinanceInvoice(invoice));
    }

    /// <summary>
    /// 查询Bank Accounts。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<BankAccountDto>> ListBankAccountsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.BankAccounts
            .AsNoTracking()
            .OrderBy(x => x.AccountNo)
            .ToListAsync(cancellationToken);

        return entities.Select(MapBankAccount).ToList();
    }

    /// <summary>
    /// Upsert Bank Account Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<BankAccountDto>> UpsertBankAccountAsync(
        UpsertBankAccountRequest request,
        CancellationToken cancellationToken)
    {
        var accountNo = NormalizeText(request.AccountNo);
        var accountName = NormalizeText(request.AccountName);
        var bankName = NormalizeText(request.BankName);
        var currencyCode = NormalizeCurrencyCode(request.CurrencyCode);

        if (string.IsNullOrWhiteSpace(accountNo))
        {
            return OperationResult<BankAccountDto>.Failure("银行账号不能为空。");
        }

        if (string.IsNullOrWhiteSpace(accountName))
        {
            return OperationResult<BankAccountDto>.Failure("账户名称不能为空。");
        }

        if (string.IsNullOrWhiteSpace(bankName))
        {
            return OperationResult<BankAccountDto>.Failure("开户行不能为空。");
        }

        var duplicate = await dbContext.BankAccounts.AnyAsync(
            x => x.AccountNo == accountNo && (!request.Id.HasValue || x.Id != request.Id.Value),
            cancellationToken);
        if (duplicate)
        {
            return OperationResult<BankAccountDto>.Failure("银行账号已经存在。");
        }

        var actor = currentUser.GetActor();
        BankAccount entity;
        if (request.Id.HasValue)
        {
            entity = await dbContext.BankAccounts.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken)
                ?? throw new InvalidOperationException("Bank account disappeared during update.");

            entity.Update(accountName, bankName, currencyCode, request.IsEnabled, actor);
        }
        else
        {
            entity = new BankAccount(accountNo, accountName, bankName, currencyCode, request.IsEnabled, actor);
            dbContext.BankAccounts.Add(entity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "BankAccountUpserted", actor, $"{entity.AccountNo}:{entity.AccountName}", cancellationToken);
        return OperationResult<BankAccountDto>.Success(MapBankAccount(entity));
    }

    /// <summary>
    /// 查询Bank Statement Lines。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<BankStatementLineDto>> ListBankStatementLinesAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.BankStatementLines
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(MapBankStatementLine)
            .ToList();
    }

    /// <summary>
    /// 创建Bank Statement Line。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<BankStatementLineDto>> CreateBankStatementLineAsync(
        CreateBankStatementLineRequest request,
        CancellationToken cancellationToken)
    {
        var direction = NormalizeBankStatementDirection(request.Direction);
        if (string.IsNullOrWhiteSpace(direction))
        {
            return OperationResult<BankStatementLineDto>.Failure("银行流水方向无效。");
        }

        if (request.Amount <= 0)
        {
            return OperationResult<BankStatementLineDto>.Failure("银行流水金额必须大于 0。");
        }

        var account = await dbContext.BankAccounts.FirstOrDefaultAsync(x => x.Id == request.BankAccountId, cancellationToken);
        if (account is null || !account.IsEnabled)
        {
            return OperationResult<BankStatementLineDto>.Failure("未找到启用的银行账户。");
        }

        var actor = currentUser.GetActor();
        var line = new BankStatementLine(
            NextNo("BS"),
            account.Id,
            account.AccountNo,
            account.AccountName,
            request.TransactionDate,
            direction,
            request.Amount,
            account.CurrencyCode,
            NormalizeText(request.CounterpartyName),
            NormalizeText(request.BankReferenceNo),
            NormalizeText(request.Summary),
            actor);

        dbContext.BankStatementLines.Add(line);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "BankStatementLineCreated", actor, $"{line.StatementNo}:{line.Amount}", cancellationToken);
        return OperationResult<BankStatementLineDto>.Success(MapBankStatementLine(line));
    }

    /// <summary>
    /// Reconcile Bank Statement Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<BankStatementLineDto>> ReconcileBankStatementAsync(
        ReconcileBankStatementRequest request,
        CancellationToken cancellationToken)
    {
        var line = await dbContext.BankStatementLines.FirstOrDefaultAsync(x => x.Id == request.BankStatementLineId, cancellationToken);
        if (line is null)
        {
            return OperationResult<BankStatementLineDto>.Failure("未找到银行流水。");
        }

        var settlement = await dbContext.Settlements.FirstOrDefaultAsync(x => x.Id == request.SettlementId, cancellationToken);
        if (settlement is null)
        {
            return OperationResult<BankStatementLineDto>.Failure("未找到结算记录。");
        }

        var direction = SettlementBankDirection(settlement.TargetType);
        if (line.BankAccountId != settlement.BankAccountId ||
            line.Direction != direction ||
            line.CurrencyCode != settlement.CurrencyCode ||
            line.Amount != settlement.Amount)
        {
            return OperationResult<BankStatementLineDto>.Failure("银行流水与结算记录的账户、方向、币种或金额不匹配。");
        }

        try
        {
            var actor = currentUser.GetActor();
            line.Reconcile(settlement.Id, settlement.SettlementNo, actor);
            settlement.Reconcile(line.Id, line.StatementNo, actor);
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditWriter.WriteAsync("Finance", "BankStatementReconciled", actor, $"{line.StatementNo}:{settlement.SettlementNo}", cancellationToken);
            return OperationResult<BankStatementLineDto>.Success(MapBankStatementLine(line));
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<BankStatementLineDto>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// 查询Settlements。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<SettlementDto>> ListSettlementsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Settlements.ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapSettlement)
            .ToList();
    }

    /// <summary>
    /// 创建Payable From Receipt。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PayableDto>> CreatePayableFromReceiptAsync(
        CreatePayableFromReceiptRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return OperationResult<PayableDto>.Failure("应付金额必须大于 0。");
        }

        var receipt = await dbContext.InventoryReceipts
            .FirstOrDefaultAsync(x => x.Id == request.InventoryReceiptId, cancellationToken);
        if (receipt is null)
        {
            return OperationResult<PayableDto>.Failure("未找到入库单。");
        }

        if (!string.Equals(receipt.Status, InventoryReceiptStatus.Completed, StringComparison.Ordinal))
        {
            return OperationResult<PayableDto>.Failure("只有已完成入库的单据才能生成应付记录。");
        }

        if (await dbContext.Payables.AnyAsync(x => x.InventoryReceiptId == receipt.Id || x.ProcurementOrderId == receipt.ProcurementOrderId, cancellationToken))
        {
            return OperationResult<PayableDto>.Failure("该入库单或采购订单已经生成应付记录。");
        }

        var tax = await ResolveProcurementTaxAsync(receipt.ProcurementOrderId, cancellationToken);
        var payable = new Payable(
            NextNo("AP"),
            receipt.ProcurementOrderId,
            receipt.ProcurementOrderNo,
            receipt.Id,
            receipt.ReceiptNo,
            receipt.SupplierName,
            request.Amount,
            await ResolveProcurementCurrencyAsync(receipt.ProcurementOrderId, cancellationToken),
            DefaultDueDate(receipt.CreatedAtUtc),
            tax.TaxInvoiceType,
            tax.TaxRate,
            FinanceSourceType.InventoryReceipt,
            currentUser.GetActor());

        dbContext.Payables.Add(payable);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "PayableCreated", currentUser.GetActor(), $"{payable.PayableNo}:{receipt.ReceiptNo}", cancellationToken);
        return OperationResult<PayableDto>.Success(MapPayable(payable, CurrentBusinessDate()));
    }

    /// <summary>
    /// 创建Payable From Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PayableDto>> CreatePayableFromOrderAsync(
        CreatePayableFromOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return OperationResult<PayableDto>.Failure("应付金额必须大于 0。");
        }

        var order = await dbContext.ProcurementOrders
            .FirstOrDefaultAsync(x => x.Id == request.ProcurementOrderId, cancellationToken);
        if (order is null)
        {
            return OperationResult<PayableDto>.Failure("未找到采购订单。");
        }

        if (!string.Equals(order.Status, ProcurementOrderStatus.Received, StringComparison.Ordinal))
        {
            return OperationResult<PayableDto>.Failure("只有已入库的采购订单才能生成应付记录。");
        }

        if (await dbContext.Payables.AnyAsync(x => x.ProcurementOrderId == order.Id, cancellationToken))
        {
            return OperationResult<PayableDto>.Failure("该采购订单已经生成应付记录。");
        }

        var tax = await ResolveProcurementTaxAsync(order.Id, cancellationToken);
        var payable = new Payable(
            NextNo("AP"),
            order.Id,
            order.OrderNo,
            null,
            string.Empty,
            order.SupplierName,
            request.Amount,
            await ResolveSupplierCurrencyAsync(order.SupplierId, cancellationToken),
            DefaultDueDate(order.CreatedAtUtc),
            tax.TaxInvoiceType,
            tax.TaxRate,
            FinanceSourceType.ProcurementOrder,
            currentUser.GetActor());

        dbContext.Payables.Add(payable);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "PayableCreated", currentUser.GetActor(), $"{payable.PayableNo}:{order.OrderNo}", cancellationToken);
        return OperationResult<PayableDto>.Success(MapPayable(payable, CurrentBusinessDate()));
    }

    /// <summary>
    /// 创建Receivable From Issue。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ReceivableDto>> CreateReceivableFromIssueAsync(
        CreateReceivableFromIssueRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return OperationResult<ReceivableDto>.Failure("应收金额必须大于 0。");
        }

        var issue = await dbContext.InventoryIssues
            .FirstOrDefaultAsync(x => x.Id == request.InventoryIssueId, cancellationToken);
        if (issue is null)
        {
            return OperationResult<ReceivableDto>.Failure("未找到出库单。");
        }

        if (!string.Equals(issue.Status, InventoryIssueStatus.Completed, StringComparison.Ordinal))
        {
            return OperationResult<ReceivableDto>.Failure("只有已完成出库的单据才能生成应收记录。");
        }

        if (await dbContext.Receivables.AnyAsync(x => x.InventoryIssueId == issue.Id || x.SalesOrderId == issue.SalesOrderId, cancellationToken))
        {
            return OperationResult<ReceivableDto>.Failure("该出库单或销售订单已经生成应收记录。");
        }

        var tax = await ResolveSalesTaxAsync(issue.SalesOrderId, cancellationToken);
        var receivable = new Receivable(
            NextNo("AR"),
            issue.SalesOrderId,
            issue.SalesOrderNo,
            issue.Id,
            issue.IssueNo,
            issue.CustomerName,
            request.Amount,
            await ResolveSalesCurrencyAsync(issue.SalesOrderId, cancellationToken),
            DefaultDueDate(issue.CreatedAtUtc),
            tax.TaxInvoiceType,
            tax.TaxRate,
            FinanceSourceType.InventoryIssue,
            currentUser.GetActor());

        dbContext.Receivables.Add(receivable);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "ReceivableCreated", currentUser.GetActor(), $"{receivable.ReceivableNo}:{issue.IssueNo}", cancellationToken);
        return OperationResult<ReceivableDto>.Success(MapReceivable(receivable, CurrentBusinessDate()));
    }

    /// <summary>
    /// 创建Receivable From Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ReceivableDto>> CreateReceivableFromOrderAsync(
        CreateReceivableFromOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return OperationResult<ReceivableDto>.Failure("应收金额必须大于 0。");
        }

        var order = await dbContext.SalesOrders
            .FirstOrDefaultAsync(x => x.Id == request.SalesOrderId, cancellationToken);
        if (order is null)
        {
            return OperationResult<ReceivableDto>.Failure("未找到销售订单。");
        }

        if (!string.Equals(order.Status, SalesOrderStatus.Shipped, StringComparison.Ordinal))
        {
            return OperationResult<ReceivableDto>.Failure("只有已出库的销售订单才能生成应收记录。");
        }

        if (await dbContext.Receivables.AnyAsync(x => x.SalesOrderId == order.Id, cancellationToken))
        {
            return OperationResult<ReceivableDto>.Failure("该销售订单已经生成应收记录。");
        }

        var tax = new TaxSnapshot(order.TaxInvoiceType, order.TaxRate);
        var receivable = new Receivable(
            NextNo("AR"),
            order.Id,
            order.OrderNo,
            null,
            string.Empty,
            order.CustomerName,
            request.Amount,
            await ResolveCustomerCurrencyAsync(order.CustomerId, cancellationToken),
            DefaultDueDate(order.CreatedAtUtc),
            tax.TaxInvoiceType,
            tax.TaxRate,
            FinanceSourceType.SalesOrder,
            currentUser.GetActor());

        dbContext.Receivables.Add(receivable);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "ReceivableCreated", currentUser.GetActor(), $"{receivable.ReceivableNo}:{order.OrderNo}", cancellationToken);
        return OperationResult<ReceivableDto>.Success(MapReceivable(receivable, CurrentBusinessDate()));
    }

    /// <summary>
    /// 创建Settlement。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<SettlementDto>> CreateSettlementAsync(
        CreateSettlementRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return OperationResult<SettlementDto>.Failure("结算金额必须大于 0。");
        }

        if (string.IsNullOrWhiteSpace(request.Method))
        {
            return OperationResult<SettlementDto>.Failure("结算方式不能为空。");
        }

        if (request.BankAccountId == Guid.Empty)
        {
            return OperationResult<SettlementDto>.Failure("请选择银行账户。");
        }

        if (string.Equals(request.TargetType, SettlementTargetType.Payable, StringComparison.OrdinalIgnoreCase))
        {
            return await SettlePayableAsync(request, cancellationToken);
        }

        if (string.Equals(request.TargetType, SettlementTargetType.Receivable, StringComparison.OrdinalIgnoreCase))
        {
            return await SettleReceivableAsync(request, cancellationToken);
        }

        return OperationResult<SettlementDto>.Failure("结算对象类型无效。");
    }

    /// <summary>
    /// Settle Payable Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<OperationResult<SettlementDto>> SettlePayableAsync(CreateSettlementRequest request, CancellationToken cancellationToken)
    {
        var payable = await dbContext.Payables.FirstOrDefaultAsync(x => x.Id == request.TargetId, cancellationToken);
        if (payable is null)
        {
            return OperationResult<SettlementDto>.Failure("未找到应付记录。");
        }

        var bankAccount = await dbContext.BankAccounts.FirstOrDefaultAsync(x => x.Id == request.BankAccountId, cancellationToken);
        if (bankAccount is null || !bankAccount.IsEnabled)
        {
            return OperationResult<SettlementDto>.Failure("未找到启用的银行账户。");
        }

        if (bankAccount.CurrencyCode != payable.CurrencyCode)
        {
            return OperationResult<SettlementDto>.Failure("银行账户币种与应付记录币种不一致。");
        }

        try
        {
            payable.Settle(request.Amount);
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<SettlementDto>.Failure(ex.Message);
        }

        var actor = currentUser.GetActor();
        var settlement = new Settlement(
            NextNo("ST"),
            SettlementTargetType.Payable,
            payable.Id,
            payable.PayableNo,
            payable.SupplierName,
            request.Amount,
            payable.CurrencyCode,
            bankAccount.Id,
            bankAccount.AccountNo,
            bankAccount.AccountName,
            request.Method.Trim(),
            request.Note.Trim(),
            actor);

        dbContext.Settlements.Add(settlement);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "PayableSettled", actor, $"{settlement.SettlementNo}:{payable.PayableNo}", cancellationToken);
        return OperationResult<SettlementDto>.Success(MapSettlement(settlement));
    }

    /// <summary>
    /// Settle Receivable Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<OperationResult<SettlementDto>> SettleReceivableAsync(CreateSettlementRequest request, CancellationToken cancellationToken)
    {
        var receivable = await dbContext.Receivables.FirstOrDefaultAsync(x => x.Id == request.TargetId, cancellationToken);
        if (receivable is null)
        {
            return OperationResult<SettlementDto>.Failure("未找到应收记录。");
        }

        var bankAccount = await dbContext.BankAccounts.FirstOrDefaultAsync(x => x.Id == request.BankAccountId, cancellationToken);
        if (bankAccount is null || !bankAccount.IsEnabled)
        {
            return OperationResult<SettlementDto>.Failure("未找到启用的银行账户。");
        }

        if (bankAccount.CurrencyCode != receivable.CurrencyCode)
        {
            return OperationResult<SettlementDto>.Failure("银行账户币种与应收记录币种不一致。");
        }

        try
        {
            receivable.Settle(request.Amount);
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<SettlementDto>.Failure(ex.Message);
        }

        var actor = currentUser.GetActor();
        var settlement = new Settlement(
            NextNo("ST"),
            SettlementTargetType.Receivable,
            receivable.Id,
            receivable.ReceivableNo,
            receivable.CustomerName,
            request.Amount,
            receivable.CurrencyCode,
            bankAccount.Id,
            bankAccount.AccountNo,
            bankAccount.AccountName,
            request.Method.Trim(),
            request.Note.Trim(),
            actor);

        dbContext.Settlements.Add(settlement);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Finance", "ReceivableSettled", actor, $"{settlement.SettlementNo}:{receivable.ReceivableNo}", cancellationToken);
        return OperationResult<SettlementDto>.Success(MapSettlement(settlement));
    }

    /// <summary>
    /// Next No。
    /// </summary>
    /// <param name="prefix">编号前缀。</param>
    private static string NextNo(string prefix) => $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

    /// <summary>
    /// 注册Accounting Account 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    /// <param name="accountMap">account Map 参数。</param>
    private static AccountingAccountDto MapAccountingAccount(
        AccountingAccount entity,
        IReadOnlyDictionary<Guid, AccountingAccount> accountMap)
    {
        var parent = entity.ParentAccountId.HasValue && accountMap.TryGetValue(entity.ParentAccountId.Value, out var parentAccount)
            ? parentAccount
            : null;

        return new AccountingAccountDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Type,
            entity.ParentAccountId,
            parent?.Code ?? string.Empty,
            parent?.Name ?? string.Empty,
            entity.IsActive,
            entity.UpdatedBy,
            entity.UpdatedAtUtc);
    }

    /// <summary>
    /// 注册Accounting Period 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static AccountingPeriodDto MapAccountingPeriod(AccountingPeriod entity) =>
        new(
            entity.Id,
            entity.Year,
            entity.Month,
            entity.Name,
            entity.StartDate,
            entity.EndDate,
            entity.Status,
            entity.CreatedBy,
            entity.ClosedBy,
            entity.ClosedAtUtc,
            entity.UpdatedAtUtc);

    /// <summary>
    /// 注册General Ledger Voucher 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static GeneralLedgerVoucherDto MapGeneralLedgerVoucher(GeneralLedgerVoucher entity) =>
        new(
            entity.Id,
            entity.VoucherNo,
            entity.AccountingPeriodId,
            entity.AccountingPeriodName,
            entity.DocumentDate,
            entity.Summary,
            entity.SourceType,
            entity.SourceId,
            entity.SourceNo,
            entity.Status,
            entity.TotalDebit,
            entity.TotalCredit,
            entity.CreatedBy,
            entity.SubmittedBy,
            entity.SubmittedAtUtc,
            entity.ReviewedBy,
            entity.ReviewedAtUtc,
            entity.ReviewNote,
            entity.Lines
                .OrderBy(x => x.CreatedAtUtc)
                .Select(MapGeneralLedgerVoucherLine)
                .ToList(),
            entity.UpdatedAtUtc);

    /// <summary>
    /// 注册General Ledger Voucher Line 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static GeneralLedgerVoucherLineDto MapGeneralLedgerVoucherLine(GeneralLedgerVoucherLine entity) =>
        new(
            entity.Id,
            entity.AccountingAccountId,
            entity.AccountCode,
            entity.AccountName,
            entity.Summary,
            entity.DebitAmount,
            entity.CreditAmount);

    /// <summary>
    /// 注册Payable 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    /// <param name="asOfDate">as Of Date 参数。</param>
    private static PayableDto MapPayable(Payable entity, DateOnly asOfDate)
    {
        var dueDate = ResolveDueDate(entity.DueDate, entity.CreatedAtUtc);
        return new PayableDto(
            entity.Id,
            entity.PayableNo,
            entity.ProcurementOrderId,
            entity.ProcurementOrderNo,
            entity.InventoryReceiptId,
            entity.InventoryReceiptNo,
            entity.SupplierName,
            entity.Amount,
            entity.NetAmount,
            entity.TaxAmount,
            entity.TaxRate,
            entity.TaxInvoiceType,
            entity.SettledAmount,
            entity.RemainingAmount,
            entity.CurrencyCode,
            dueDate,
            CalculateOverdueDays(dueDate, asOfDate),
            entity.Status,
            entity.SourceType,
            entity.CreatedAtUtc);
    }

    /// <summary>
    /// 注册Receivable 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    /// <param name="asOfDate">as Of Date 参数。</param>
    private static ReceivableDto MapReceivable(Receivable entity, DateOnly asOfDate)
    {
        var dueDate = ResolveDueDate(entity.DueDate, entity.CreatedAtUtc);
        return new ReceivableDto(
            entity.Id,
            entity.ReceivableNo,
            entity.SalesOrderId,
            entity.SalesOrderNo,
            entity.InventoryIssueId,
            entity.InventoryIssueNo,
            entity.CustomerName,
            entity.Amount,
            entity.NetAmount,
            entity.TaxAmount,
            entity.TaxRate,
            entity.TaxInvoiceType,
            entity.SettledAmount,
            entity.RemainingAmount,
            entity.CurrencyCode,
            dueDate,
            CalculateOverdueDays(dueDate, asOfDate),
            entity.Status,
            entity.SourceType,
            entity.CreatedAtUtc);
    }

    /// <summary>
    /// 注册Finance Invoice 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static FinanceInvoiceDto MapFinanceInvoice(FinanceInvoice entity) =>
        new(
            entity.Id,
            entity.InvoiceNo,
            entity.Direction,
            entity.SourceId,
            entity.SourceNo,
            entity.CounterpartyName,
            entity.TaxInvoiceType,
            entity.TaxRate,
            entity.GrossAmount,
            entity.NetAmount,
            entity.TaxAmount,
            entity.CurrencyCode,
            entity.InvoiceDate,
            entity.Note,
            entity.CreatedBy,
            entity.CreatedAtUtc);

    /// <summary>
    /// 注册Bank Account 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static BankAccountDto MapBankAccount(BankAccount entity) =>
        new(
            entity.Id,
            entity.AccountNo,
            entity.AccountName,
            entity.BankName,
            entity.CurrencyCode,
            entity.IsEnabled,
            entity.UpdatedBy,
            entity.UpdatedAtUtc);

    /// <summary>
    /// 注册Bank Statement Line 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static BankStatementLineDto MapBankStatementLine(BankStatementLine entity) =>
        new(
            entity.Id,
            entity.StatementNo,
            entity.BankAccountId,
            entity.BankAccountNo,
            entity.BankAccountName,
            entity.TransactionDate,
            entity.Direction,
            entity.Amount,
            entity.CurrencyCode,
            entity.CounterpartyName,
            entity.BankReferenceNo,
            entity.Summary,
            entity.ReconciliationStatus,
            entity.SettlementId,
            entity.SettlementNo,
            entity.ReconciledBy,
            entity.ReconciledAtUtc,
            entity.CreatedBy,
            entity.CreatedAtUtc);

    /// <summary>
    /// 注册Settlement 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static SettlementDto MapSettlement(Settlement entity) =>
        new(
            entity.Id,
            entity.SettlementNo,
            entity.TargetType,
            entity.TargetId,
            entity.TargetNo,
            entity.CounterpartyName,
            entity.Amount,
            entity.CurrencyCode,
            entity.BankAccountId,
            entity.BankAccountNo,
            entity.BankAccountName,
            entity.Method,
            entity.Note,
            entity.ReconciliationStatus,
            entity.BankStatementLineId,
            entity.BankStatementNo,
            entity.ReconciledBy,
            entity.ReconciledAtUtc,
            entity.SettledBy,
            entity.CreatedAtUtc);

    /// <summary>
    /// Build Aging Side。
    /// </summary>
    /// <param name="sourceEntries">source Entries 参数。</param>
    /// <param name="asOfDate">as Of Date 参数。</param>
    private static AgingSideDto BuildAgingSide(IEnumerable<AgingSourceEntry> sourceEntries, DateOnly asOfDate)
    {
        var entries = sourceEntries
            .Select(entry =>
            {
                var overdueDays = CalculateOverdueDays(entry.DueDate, asOfDate);
                var bucket = ResolveAgingBucket(overdueDays);
                return new AgingEntryDto(
                    entry.Id,
                    entry.DocumentNo,
                    entry.CounterpartyName,
                    entry.SourceNo,
                    entry.Amount,
                    entry.SettledAmount,
                    entry.RemainingAmount,
                    entry.CurrencyCode,
                    entry.DueDate,
                    overdueDays,
                    bucket,
                    entry.Status);
            })
            .OrderByDescending(x => x.OverdueDays)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.DocumentNo)
            .ToList();

        var buckets = AgingBucketDefinitions()
            .Select(definition =>
            {
                var bucketEntries = entries.Where(x => x.Bucket == definition.Key).ToList();
                return new AgingBucketDto(
                    definition.Key,
                    definition.Name,
                    bucketEntries.Count,
                    bucketEntries.Sum(x => x.RemainingAmount));
            })
            .ToList();

        return new AgingSideDto(
            entries.Sum(x => x.RemainingAmount),
            entries.Where(x => x.OverdueDays > 0).Sum(x => x.RemainingAmount),
            entries.Count,
            entries.Count(x => x.OverdueDays > 0),
            buckets,
            entries);
    }

    private static IReadOnlyList<(string Key, string Name)> AgingBucketDefinitions() =>
    [
        ("Current", "未到期"),
        ("Days0To30", "逾期0-30天"),
        ("Days31To60", "逾期31-60天"),
        ("Days61To90", "逾期61-90天"),
        ("Days90Plus", "逾期90天以上")
    ];

    /// <summary>
    /// Resolve Aging Bucket。
    /// </summary>
    /// <param name="overdueDays">overdue Days 参数。</param>
    private static string ResolveAgingBucket(int overdueDays) =>
        overdueDays switch
        {
            <= 0 => "Current",
            <= 30 => "Days0To30",
            <= 60 => "Days31To60",
            <= 90 => "Days61To90",
            _ => "Days90Plus"
        };

    /// <summary>
    /// Current Business Date。
    /// </summary>
    private static DateOnly CurrentBusinessDate() => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Default Due Date。
    /// </summary>
    /// <param name="sourceDate">source Date 参数。</param>
    private static DateOnly DefaultDueDate(DateTimeOffset sourceDate) =>
        DateOnly.FromDateTime(sourceDate.UtcDateTime).AddDays(DefaultPaymentTermDays);

    /// <summary>
    /// Resolve Due Date。
    /// </summary>
    /// <param name="dueDate">due Date 参数。</param>
    /// <param name="createdAtUtc">创建时间，使用 UTC。</param>
    private static DateOnly ResolveDueDate(DateOnly? dueDate, DateTimeOffset createdAtUtc) =>
        dueDate ?? DefaultDueDate(createdAtUtc);

    /// <summary>
    /// Calculate Overdue Days。
    /// </summary>
    /// <param name="dueDate">due Date 参数。</param>
    /// <param name="asOfDate">as Of Date 参数。</param>
    private static int CalculateOverdueDays(DateOnly dueDate, DateOnly asOfDate) =>
        Math.Max(0, asOfDate.DayNumber - dueDate.DayNumber);

    /// <summary>
    /// Resolve Procurement Currency Async。
    /// </summary>
    /// <param name="procurementOrderId">procurement Order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<string> ResolveProcurementCurrencyAsync(Guid procurementOrderId, CancellationToken cancellationToken)
    {
        var order = await dbContext.ProcurementOrders.FirstOrDefaultAsync(x => x.Id == procurementOrderId, cancellationToken);
        return order is null ? "CNY" : await ResolveSupplierCurrencyAsync(order.SupplierId, cancellationToken);
    }

    /// <summary>
    /// Resolve Procurement Tax Async。
    /// </summary>
    /// <param name="procurementOrderId">procurement Order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<TaxSnapshot> ResolveProcurementTaxAsync(Guid procurementOrderId, CancellationToken cancellationToken)
    {
        var order = await dbContext.ProcurementOrders.FirstOrDefaultAsync(x => x.Id == procurementOrderId, cancellationToken);
        if (order is null)
        {
            return await ResolveDefaultTaxAsync(cancellationToken);
        }

        var request = await dbContext.ProcurementRequests.FirstOrDefaultAsync(x => x.Id == order.RequestId, cancellationToken);
        return request is null
            ? await ResolveDefaultTaxAsync(cancellationToken)
            : new TaxSnapshot(request.TaxInvoiceType, request.TaxRate);
    }

    /// <summary>
    /// Resolve Supplier Currency Async。
    /// </summary>
    /// <param name="supplierId">供应商标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<string> ResolveSupplierCurrencyAsync(Guid supplierId, CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == supplierId, cancellationToken);
        return string.IsNullOrWhiteSpace(supplier?.CurrencyCode) ? "CNY" : supplier.CurrencyCode;
    }

    /// <summary>
    /// Resolve Sales Currency Async。
    /// </summary>
    /// <param name="salesOrderId">sales Order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<string> ResolveSalesCurrencyAsync(Guid salesOrderId, CancellationToken cancellationToken)
    {
        var order = await dbContext.SalesOrders.FirstOrDefaultAsync(x => x.Id == salesOrderId, cancellationToken);
        return order is null ? "CNY" : await ResolveCustomerCurrencyAsync(order.CustomerId, cancellationToken);
    }

    /// <summary>
    /// Resolve Sales Tax Async。
    /// </summary>
    /// <param name="salesOrderId">sales Order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<TaxSnapshot> ResolveSalesTaxAsync(Guid salesOrderId, CancellationToken cancellationToken)
    {
        var order = await dbContext.SalesOrders.FirstOrDefaultAsync(x => x.Id == salesOrderId, cancellationToken);
        return order is null
            ? await ResolveDefaultTaxAsync(cancellationToken)
            : new TaxSnapshot(order.TaxInvoiceType, order.TaxRate);
    }

    /// <summary>
    /// Resolve Customer Currency Async。
    /// </summary>
    /// <param name="customerId">客户标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<string> ResolveCustomerCurrencyAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);
        return string.IsNullOrWhiteSpace(customer?.CurrencyCode) ? "CNY" : customer.CurrencyCode;
    }

    /// <summary>
    /// Resolve Default Tax Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<TaxSnapshot> ResolveDefaultTaxAsync(CancellationToken cancellationToken)
    {
        var settings = await dbContext.LocalizationSettings.FirstOrDefaultAsync(cancellationToken);
        return new TaxSnapshot(settings?.TaxInvoiceType ?? "增值税普通发票", settings?.DefaultTaxRate ?? 0.13m);
    }

    /// <summary>
    /// Normalize Account Code。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeAccountCode(string value) => NormalizeText(value).ToUpperInvariant();

    /// <summary>
    /// Normalize Text。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeText(string? value) => value?.Trim() ?? string.Empty;

    /// <summary>
    /// Normalize Currency Code。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeCurrencyCode(string? value)
    {
        var normalized = NormalizeText(value).ToUpperInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? "CNY" : normalized;
    }

    /// <summary>
    /// Normalize Invoice Direction。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeInvoiceDirection(string value)
    {
        return NormalizeText(value).ToLowerInvariant() switch
        {
            "payable" => FinanceInvoiceDirection.Payable,
            "receivable" => FinanceInvoiceDirection.Receivable,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Normalize Bank Statement Direction。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeBankStatementDirection(string value)
    {
        return NormalizeText(value).ToLowerInvariant() switch
        {
            "inflow" => BankStatementDirection.Inflow,
            "income" => BankStatementDirection.Inflow,
            "收款" => BankStatementDirection.Inflow,
            "收入" => BankStatementDirection.Inflow,
            "outflow" => BankStatementDirection.Outflow,
            "expense" => BankStatementDirection.Outflow,
            "付款" => BankStatementDirection.Outflow,
            "支出" => BankStatementDirection.Outflow,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Settlement Bank Direction。
    /// </summary>
    /// <param name="targetType">target Type 参数。</param>
    private static string SettlementBankDirection(string targetType) =>
        string.Equals(targetType, SettlementTargetType.Receivable, StringComparison.Ordinal)
            ? BankStatementDirection.Inflow
            : BankStatementDirection.Outflow;

    /// <summary>
    /// Resolve Business Voucher Source Async。
    /// </summary>
    /// <param name="requestedSourceType">requested Source Type 参数。</param>
    /// <param name="sourceId">来源单据标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<OperationResult<BusinessVoucherSource>> ResolveBusinessVoucherSourceAsync(
        string requestedSourceType,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        if (sourceId == Guid.Empty)
        {
            return OperationResult<BusinessVoucherSource>.Failure("业务来源不能为空。");
        }

        var sourceType = NormalizeVoucherSourceType(requestedSourceType);
        if (sourceType == GeneralLedgerVoucherSourceType.Payable)
        {
            var payable = await dbContext.Payables.FirstOrDefaultAsync(x => x.Id == sourceId, cancellationToken);
            if (payable is null)
            {
                return OperationResult<BusinessVoucherSource>.Failure("未找到应付记录。");
            }

            return OperationResult<BusinessVoucherSource>.Success(new BusinessVoucherSource(
                GeneralLedgerVoucherSourceType.Payable,
                payable.Id,
                payable.PayableNo,
                payable.Amount,
                $"应付入账 {payable.PayableNo}",
                $"确认应付来源 {payable.PayableNo}",
                $"确认应付 {payable.SupplierName}"));
        }

        if (sourceType == GeneralLedgerVoucherSourceType.Receivable)
        {
            var receivable = await dbContext.Receivables.FirstOrDefaultAsync(x => x.Id == sourceId, cancellationToken);
            if (receivable is null)
            {
                return OperationResult<BusinessVoucherSource>.Failure("未找到应收记录。");
            }

            return OperationResult<BusinessVoucherSource>.Success(new BusinessVoucherSource(
                GeneralLedgerVoucherSourceType.Receivable,
                receivable.Id,
                receivable.ReceivableNo,
                receivable.Amount,
                $"应收入账 {receivable.ReceivableNo}",
                $"确认应收 {receivable.CustomerName}",
                $"确认应收来源 {receivable.ReceivableNo}"));
        }

        if (sourceType == GeneralLedgerVoucherSourceType.Settlement)
        {
            var settlement = await dbContext.Settlements.FirstOrDefaultAsync(x => x.Id == sourceId, cancellationToken);
            if (settlement is null)
            {
                return OperationResult<BusinessVoucherSource>.Failure("未找到结算记录。");
            }

            var settlementText = string.Equals(settlement.TargetType, SettlementTargetType.Payable, StringComparison.Ordinal)
                ? "付款结算"
                : "收款结算";

            return OperationResult<BusinessVoucherSource>.Success(new BusinessVoucherSource(
                GeneralLedgerVoucherSourceType.Settlement,
                settlement.Id,
                settlement.SettlementNo,
                settlement.Amount,
                $"{settlementText} {settlement.SettlementNo}",
                $"{settlementText}借方 {settlement.TargetNo}",
                $"{settlementText}贷方 {settlement.TargetNo}"));
        }

        return OperationResult<BusinessVoucherSource>.Failure("业务来源类型无效。");
    }

    /// <summary>
    /// Normalize Voucher Source Type。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeVoucherSourceType(string value)
    {
        return NormalizeText(value).ToLowerInvariant() switch
        {
            "payable" => GeneralLedgerVoucherSourceType.Payable,
            "receivable" => GeneralLedgerVoucherSourceType.Receivable,
            "settlement" => GeneralLedgerVoucherSourceType.Settlement,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Normalize Account Type。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeAccountType(string value)
    {
        var normalized = NormalizeText(value);
        return normalized.ToLowerInvariant() switch
        {
            "asset" => AccountingAccountType.Asset,
            "liability" => AccountingAccountType.Liability,
            "equity" => AccountingAccountType.Equity,
            "revenue" => AccountingAccountType.Revenue,
            "expense" => AccountingAccountType.Expense,
            "cost" => AccountingAccountType.Cost,
            _ => normalized
        };
    }

    /// <summary>
    /// Build Voucher Lines Async。
    /// </summary>
    /// <param name="requestLines">request Lines 参数。</param>
    /// <param name="fallbackSummary">fallback Summary 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<OperationResult<List<GeneralLedgerVoucherLine>>> BuildVoucherLinesAsync(
        IReadOnlyList<CreateManualVoucherLineRequest> requestLines,
        string fallbackSummary,
        CancellationToken cancellationToken)
    {
        if (requestLines.Count < 2)
        {
            return OperationResult<List<GeneralLedgerVoucherLine>>.Failure("总账凭证至少需要两条分录。");
        }

        var accountIds = requestLines.Select(x => x.AccountingAccountId).Distinct().ToList();
        var accounts = await dbContext.AccountingAccounts
            .Where(x => accountIds.Contains(x.Id) && x.IsActive)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (accounts.Count != accountIds.Count)
        {
            return OperationResult<List<GeneralLedgerVoucherLine>>.Failure("存在不存在或已停用的会计科目。");
        }

        var lines = new List<GeneralLedgerVoucherLine>();
        foreach (var line in requestLines)
        {
            var debitAmount = line.DebitAmount;
            var creditAmount = line.CreditAmount;
            if (debitAmount < 0 || creditAmount < 0)
            {
                return OperationResult<List<GeneralLedgerVoucherLine>>.Failure("借贷金额不能为负数。");
            }

            if ((debitAmount == 0 && creditAmount == 0) || (debitAmount > 0 && creditAmount > 0))
            {
                return OperationResult<List<GeneralLedgerVoucherLine>>.Failure("每条分录必须且只能填写借方或贷方金额。");
            }

            var account = accounts[line.AccountingAccountId];
            var summary = NormalizeText(line.Summary);
            lines.Add(new GeneralLedgerVoucherLine(
                account.Id,
                account.Code,
                account.Name,
                string.IsNullOrWhiteSpace(summary) ? fallbackSummary : summary,
                debitAmount,
                creditAmount));
        }

        var balanceError = ValidateVoucherLines(lines);
        return balanceError is null
            ? OperationResult<List<GeneralLedgerVoucherLine>>.Success(lines)
            : OperationResult<List<GeneralLedgerVoucherLine>>.Failure(balanceError);
    }

    /// <summary>
    /// Validate Voucher Lines。
    /// </summary>
    /// <param name="lines">明细行集合。</param>
    private static string? ValidateVoucherLines(IEnumerable<GeneralLedgerVoucherLine> lines)
    {
        var materialized = lines.ToList();
        if (materialized.Count < 2)
        {
            return "总账凭证至少需要两条分录。";
        }

        var totalDebit = materialized.Sum(x => x.DebitAmount);
        var totalCredit = materialized.Sum(x => x.CreditAmount);
        if (totalDebit <= 0 || totalCredit <= 0)
        {
            return "借方和贷方总额必须大于 0。";
        }

        return totalDebit == totalCredit ? null : "借贷金额必须平衡。";
    }

    /// <summary>
    /// Validate Open Period。
    /// </summary>
    /// <param name="period">会计期间。</param>
    /// <param name="documentDate">document Date 参数。</param>
    private static string? ValidateOpenPeriod(AccountingPeriod period, DateOnly documentDate)
    {
        if (!string.Equals(period.Status, AccountingPeriodStatus.Open, StringComparison.Ordinal))
        {
            return "会计期间已关账，不能处理凭证。";
        }

        if (documentDate < period.StartDate || documentDate > period.EndDate)
        {
            return "凭证日期必须落在会计期间内。";
        }

        return null;
    }
}
