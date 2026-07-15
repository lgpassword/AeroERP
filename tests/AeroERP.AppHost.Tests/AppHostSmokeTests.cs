using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace AeroERP.AppHost.Tests;

/// <summary>
/// App Host Smoke Tests 业务对象。
/// </summary>
public sealed class AppHostSmokeTests : IClassFixture<AppHostFactory>
{
    /// <summary>
    /// client。
    /// </summary>
    private readonly HttpClient client;

    /// <summary>
    /// 初始化App Host Smoke Tests实例。
    /// </summary>
    /// <param name="factory">测试应用工厂。</param>
    public AppHostSmokeTests(AppHostFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    /// <summary>
    /// Root_endpoint_exposes_module_and_plugin_catalog。
    /// </summary>
    public async Task Root_endpoint_exposes_module_and_plugin_catalog()
    {
        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();

        Assert.Equal("AeroERP API", body?["name"]?.GetValue<string>());
        Assert.Contains("platform", body?["modules"]?.AsArray().Select(x => x?.GetValue<string>()) ?? []);
        Assert.Contains("aeroerp.core", body?["plugins"]?.AsArray()
            .Select(x => x?["key"]?.GetValue<string>()) ?? []);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    /// <summary>
    /// Health_endpoints_are_public_and_healthy。
    /// </summary>
    /// <param name="path">请求路径。</param>
    public async Task Health_endpoints_are_public_and_healthy(string path)
    {
        var response = await client.GetAsync(path);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();

        Assert.Equal("Healthy", body?["status"]?.GetValue<string>());
    }

    [Fact]
    /// <summary>
    /// Swagger_document_is_available。
    /// </summary>
    public async Task Swagger_document_is_available()
    {
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    /// <summary>
    /// Secured_platform_endpoint_rejects_anonymous_requests。
    /// </summary>
    public async Task Secured_platform_endpoint_rejects_anonymous_requests()
    {
        var response = await client.GetAsync("/api/platform/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    /// <summary>
    /// Admin_login_returns_token_and_platform_claims。
    /// </summary>
    public async Task Admin_login_returns_token_and_platform_claims()
    {
        var response = await client.PostAsJsonAsync("/api/platform/auth/login", new
        {
            userName = "admin",
            password = "Admin@123456"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        var accessToken = body?["accessToken"]?.GetValue<string>();
        var user = body?["user"]?.AsObject();

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.Equal("admin", user?["userName"]?.GetValue<string>());
        Assert.Contains("platform-admin", JsonValues(user?["roles"]));
        Assert.Contains("platform", JsonValues(user?["visibleModuleKeys"]));
        Assert.Contains("finance", JsonValues(user?["visibleModuleKeys"]));
        Assert.Contains("plugin.manage", JsonValues(user?["permissions"]));
        Assert.Contains("finance.accounting.manage", JsonValues(user?["permissions"]));
        Assert.Contains("finance.voucher.manage", JsonValues(user?["permissions"]));
        Assert.Contains("finance.voucher.review", JsonValues(user?["permissions"]));
    }

    [Fact]
    /// <summary>
    /// Authenticated_admin_can_read_current_user_and_visible_modules。
    /// </summary>
    public async Task Authenticated_admin_can_read_current_user_and_visible_modules()
    {
        var accessToken = await LoginAsAdminAsync();

        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/platform/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var meResponse = await client.SendAsync(meRequest);

        meResponse.EnsureSuccessStatusCode();
        var currentUser = await meResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("admin", currentUser?["userName"]?.GetValue<string>());
        Assert.Contains("planning", JsonValues(currentUser?["visibleModuleKeys"]));

        var modulesRequest = new HttpRequestMessage(HttpMethod.Get, "/api/platform/visible-modules");
        modulesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var modulesResponse = await client.SendAsync(modulesRequest);

        modulesResponse.EnsureSuccessStatusCode();
        var modules = await modulesResponse.Content.ReadFromJsonAsync<JsonArray>();
        var moduleKeys = modules?.Select(x => x?["key"]?.GetValue<string>()).Where(x => x is not null).Select(x => x!) ?? [];

        Assert.Contains("platform", moduleKeys);
        Assert.Contains("finance", moduleKeys);
        Assert.Contains("planning", moduleKeys);
    }

    [Fact]
    /// <summary>
    /// Authenticated_admin_can_manage_finance_accounting_foundation。
    /// </summary>
    public async Task Authenticated_admin_can_manage_finance_accounting_foundation()
    {
        var accessToken = await LoginAsAdminAsync();
        var accountCode = $"10{DateTime.UtcNow:HHmmssfff}";

        var createAccountResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/accounting-accounts", new
        {
            code = accountCode,
            name = "测试现金科目",
            type = "Asset",
            parentAccountId = (Guid?)null,
            isActive = true
        });

        createAccountResponse.EnsureSuccessStatusCode();
        var account = await createAccountResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(accountCode, account?["code"]?.GetValue<string>());
        var debitAccountId = account?["id"]?.GetValue<Guid>() ?? Guid.Empty;
        Assert.NotEqual(Guid.Empty, debitAccountId);

        var creditAccountCode = $"20{DateTime.UtcNow:HHmmssfff}";
        var createCreditAccountResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/accounting-accounts", new
        {
            code = creditAccountCode,
            name = "测试收入科目",
            type = "Revenue",
            parentAccountId = (Guid?)null,
            isActive = true
        });

        createCreditAccountResponse.EnsureSuccessStatusCode();
        var creditAccount = await createCreditAccountResponse.Content.ReadFromJsonAsync<JsonObject>();
        var creditAccountId = creditAccount?["id"]?.GetValue<Guid>() ?? Guid.Empty;
        Assert.NotEqual(Guid.Empty, creditAccountId);

        var accountsResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Get, "/api/finance/accounting-accounts");
        accountsResponse.EnsureSuccessStatusCode();
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.Contains(accountCode, accounts?.Select(x => x?["code"]?.GetValue<string>()) ?? []);

        var createPeriodResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/accounting-periods", new
        {
            year = 2099,
            month = 12
        });

        createPeriodResponse.EnsureSuccessStatusCode();
        var period = await createPeriodResponse.Content.ReadFromJsonAsync<JsonObject>();
        var periodId = period?["id"]?.GetValue<Guid>() ?? Guid.Empty;
        Assert.Equal("Open", period?["status"]?.GetValue<string>());
        Assert.NotEqual(Guid.Empty, periodId);

        var createVoucherResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/vouchers/manual", new
        {
            accountingPeriodId = periodId,
            documentDate = "2099-12-15",
            summary = "测试手工凭证",
            lines = new[]
            {
                new
                {
                    accountingAccountId = debitAccountId,
                    summary = "测试借方",
                    debitAmount = 100m,
                    creditAmount = 0m
                },
                new
                {
                    accountingAccountId = creditAccountId,
                    summary = "测试贷方",
                    debitAmount = 0m,
                    creditAmount = 100m
                }
            }
        });

        createVoucherResponse.EnsureSuccessStatusCode();
        var voucher = await createVoucherResponse.Content.ReadFromJsonAsync<JsonObject>();
        var voucherId = voucher?["id"]?.GetValue<Guid>() ?? Guid.Empty;
        Assert.Equal("Draft", voucher?["status"]?.GetValue<string>());
        Assert.Equal("Manual", voucher?["sourceType"]?.GetValue<string>());
        Assert.Equal(string.Empty, voucher?["sourceNo"]?.GetValue<string>());
        Assert.True(voucher?["sourceId"] is null || voucher["sourceId"]!.GetValueKind() == JsonValueKind.Null);
        Assert.Equal(100m, voucher?["totalDebit"]?.GetValue<decimal>());
        Assert.Equal(100m, voucher?["totalCredit"]?.GetValue<decimal>());
        Assert.NotEqual(Guid.Empty, voucherId);

        var submitVoucherResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, $"/api/finance/vouchers/{voucherId}/submit");
        submitVoucherResponse.EnsureSuccessStatusCode();
        var submittedVoucher = await submitVoucherResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Submitted", submittedVoucher?["status"]?.GetValue<string>());

        var approveVoucherResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, $"/api/finance/vouchers/{voucherId}/approve", new
        {
            note = "测试审核通过"
        });
        approveVoucherResponse.EnsureSuccessStatusCode();
        var approvedVoucher = await approveVoucherResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Approved", approvedVoucher?["status"]?.GetValue<string>());

        var closeResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, $"/api/finance/accounting-periods/{periodId}/close");
        closeResponse.EnsureSuccessStatusCode();
        var closedPeriod = await closeResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Closed", closedPeriod?["status"]?.GetValue<string>());

        var reopenResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, $"/api/finance/accounting-periods/{periodId}/reopen");
        reopenResponse.EnsureSuccessStatusCode();
        var reopenedPeriod = await reopenResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Open", reopenedPeriod?["status"]?.GetValue<string>());

        var createBlockedPeriodResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/accounting-periods", new
        {
            year = 2099,
            month = 11
        });
        createBlockedPeriodResponse.EnsureSuccessStatusCode();
        var blockedPeriod = await createBlockedPeriodResponse.Content.ReadFromJsonAsync<JsonObject>();
        var blockedPeriodId = blockedPeriod?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var draftVoucherResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/vouchers/manual", new
        {
            accountingPeriodId = blockedPeriodId,
            documentDate = "2099-11-15",
            summary = "测试未决凭证",
            lines = new[]
            {
                new
                {
                    accountingAccountId = debitAccountId,
                    summary = "未决借方",
                    debitAmount = 50m,
                    creditAmount = 0m
                },
                new
                {
                    accountingAccountId = creditAccountId,
                    summary = "未决贷方",
                    debitAmount = 0m,
                    creditAmount = 50m
                }
            }
        });
        draftVoucherResponse.EnsureSuccessStatusCode();

        var blockedCloseResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, $"/api/finance/accounting-periods/{blockedPeriodId}/close");
        Assert.Equal(HttpStatusCode.BadRequest, blockedCloseResponse.StatusCode);
        var blockedCloseBody = await blockedCloseResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Contains("草稿或待审凭证", blockedCloseBody?["message"]?.GetValue<string>());

        var ignoredDraftVoucherResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/vouchers/manual", new
        {
            accountingPeriodId = periodId,
            documentDate = "2099-12-16",
            summary = "测试报表忽略草稿凭证",
            lines = new[]
            {
                new
                {
                    accountingAccountId = debitAccountId,
                    summary = "报表忽略借方",
                    debitAmount = 40m,
                    creditAmount = 0m
                },
                new
                {
                    accountingAccountId = creditAccountId,
                    summary = "报表忽略贷方",
                    debitAmount = 0m,
                    creditAmount = 40m
                }
            }
        });
        ignoredDraftVoucherResponse.EnsureSuccessStatusCode();

        var reportResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Get, $"/api/finance/reports?accountingPeriodId={periodId}");
        reportResponse.EnsureSuccessStatusCode();
        var report = await reportResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(periodId, report?["accountingPeriodId"]?.GetValue<Guid>());
        Assert.Equal(1, report?["approvedVoucherCount"]?.GetValue<int>());
        Assert.Equal(100m, report?["totalDebit"]?.GetValue<decimal>());
        Assert.Equal(100m, report?["totalCredit"]?.GetValue<decimal>());
        Assert.True(report?["isBalanced"]?.GetValue<bool>());
        Assert.Contains(report?["trialBalance"]?.AsArray() ?? [], line =>
            line?["accountCode"]?.GetValue<string>() == accountCode &&
            line?["debitAmount"]?.GetValue<decimal>() == 100m &&
            line?["endingDebit"]?.GetValue<decimal>() == 100m);
        Assert.Contains(report?["trialBalance"]?.AsArray() ?? [], line =>
            line?["accountCode"]?.GetValue<string>() == creditAccountCode &&
            line?["creditAmount"]?.GetValue<decimal>() == 100m &&
            line?["endingCredit"]?.GetValue<decimal>() == 100m);
        Assert.Equal(100m, report?["incomeStatement"]?["revenue"]?.GetValue<decimal>());
        Assert.Equal(100m, report?["incomeStatement"]?["profit"]?.GetValue<decimal>());
        Assert.Equal(100m, report?["balanceSheet"]?["assets"]?.GetValue<decimal>());
        Assert.Equal(100m, report?["balanceSheet"]?["retainedEarnings"]?.GetValue<decimal>());
        Assert.Equal(0m, report?["balanceSheet"]?["difference"]?.GetValue<decimal>());
    }

    [Fact]
    /// <summary>
    /// Authenticated_admin_can_create_business_voucher_from_payable_source。
    /// </summary>
    public async Task Authenticated_admin_can_create_business_voucher_from_payable_source()
    {
        var accessToken = await LoginAsAdminAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var debitAccountResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/accounting-accounts", new
        {
            code = $"51{suffix}",
            name = "测试采购成本科目",
            type = "Cost",
            parentAccountId = (Guid?)null,
            isActive = true
        });
        debitAccountResponse.EnsureSuccessStatusCode();
        var debitAccount = await debitAccountResponse.Content.ReadFromJsonAsync<JsonObject>();
        var debitAccountId = debitAccount?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var creditAccountResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/accounting-accounts", new
        {
            code = $"22{suffix}",
            name = "测试应付科目",
            type = "Liability",
            parentAccountId = (Guid?)null,
            isActive = true
        });
        creditAccountResponse.EnsureSuccessStatusCode();
        var creditAccount = await creditAccountResponse.Content.ReadFromJsonAsync<JsonObject>();
        var creditAccountId = creditAccount?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var periodResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/accounting-periods", new
        {
            year = 2099,
            month = 10
        });
        periodResponse.EnsureSuccessStatusCode();
        var period = await periodResponse.Content.ReadFromJsonAsync<JsonObject>();
        var periodId = period?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var supplierResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/master-data/suppliers", new
        {
            code = $"SUP{suffix}",
            name = "测试供应商",
            contactName = "采购联系人",
            phone = "13800000000",
            isEnabled = true,
            organizationId = (Guid?)null,
            currencyCode = "CNY",
            taxpayerId = string.Empty,
            invoiceTitle = string.Empty
        });
        supplierResponse.EnsureSuccessStatusCode();
        var supplier = await supplierResponse.Content.ReadFromJsonAsync<JsonObject>();
        var supplierId = supplier?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var itemResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/master-data/items", new
        {
            code = $"ITM{suffix}",
            name = "测试物料",
            specification = "EA",
            unit = "件",
            isEnabled = true
        });
        itemResponse.EnsureSuccessStatusCode();
        var item = await itemResponse.Content.ReadFromJsonAsync<JsonObject>();
        var itemId = item?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var warehouseResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/master-data/warehouses", new
        {
            code = $"WH{suffix}",
            name = "测试仓库",
            location = "测试地址",
            isEnabled = true,
            organizationId = (Guid?)null
        });
        warehouseResponse.EnsureSuccessStatusCode();
        var warehouse = await warehouseResponse.Content.ReadFromJsonAsync<JsonObject>();
        var warehouseId = warehouse?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var procurementRequestResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/procurement/requests", new
        {
            supplierId,
            title = "测试业务凭证采购申请",
            currencyCode = "CNY",
            taxInvoiceType = "增值税普通发票",
            taxRate = 0.13m,
            lines = new[]
            {
                new
                {
                    itemId,
                    quantity = 3m,
                    unit = "件"
                }
            }
        });
        procurementRequestResponse.EnsureSuccessStatusCode();
        var procurementRequest = await procurementRequestResponse.Content.ReadFromJsonAsync<JsonObject>();
        var procurementRequestId = procurementRequest?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var approveRequestResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, $"/api/procurement/requests/{procurementRequestId}/decision", new
        {
            decision = "Approved"
        });
        approveRequestResponse.EnsureSuccessStatusCode();

        var orderResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, $"/api/procurement/requests/{procurementRequestId}/convert-order");
        orderResponse.EnsureSuccessStatusCode();
        var order = await orderResponse.Content.ReadFromJsonAsync<JsonObject>();
        var orderId = order?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var releaseResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, $"/api/procurement/orders/{orderId}/release");
        releaseResponse.EnsureSuccessStatusCode();

        var receiptResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/inventory/receipts", new
        {
            procurementOrderId = orderId,
            warehouseId,
            locationId = (Guid?)null
        });
        receiptResponse.EnsureSuccessStatusCode();
        var receipt = await receiptResponse.Content.ReadFromJsonAsync<JsonObject>();
        var receiptId = receipt?["id"]?.GetValue<Guid>() ?? Guid.Empty;

        var payableResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/payables/from-receipt", new
        {
            inventoryReceiptId = receiptId,
            amount = 250m
        });
        payableResponse.EnsureSuccessStatusCode();
        var payable = await payableResponse.Content.ReadFromJsonAsync<JsonObject>();
        var payableId = payable?["id"]?.GetValue<Guid>() ?? Guid.Empty;
        var payableNo = payable?["payableNo"]?.GetValue<string>() ?? string.Empty;
        Assert.False(string.IsNullOrWhiteSpace(payable?["dueDate"]?.GetValue<string>()));
        Assert.Equal(0, payable?["overdueDays"]?.GetValue<int>());
        Assert.Equal("增值税普通发票", payable?["taxInvoiceType"]?.GetValue<string>());
        Assert.Equal(0.13m, payable?["taxRate"]?.GetValue<decimal>());
        Assert.Equal(250m, payable?["amount"]?.GetValue<decimal>());
        var payableNetAmount = payable?["netAmount"]?.GetValue<decimal>() ?? 0m;
        var payableTaxAmount = payable?["taxAmount"]?.GetValue<decimal>() ?? 0m;
        Assert.True(payableNetAmount > 0m);
        Assert.True(payableTaxAmount > 0m);
        Assert.Equal(250m, payableNetAmount + payableTaxAmount);

        var invoiceResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/invoices", new
        {
            direction = "Payable",
            sourceId = payableId,
            invoiceDate = "2099-10-15",
            note = "测试发票"
        });
        invoiceResponse.EnsureSuccessStatusCode();
        var invoice = await invoiceResponse.Content.ReadFromJsonAsync<JsonObject>();
        var invoiceNo = invoice?["invoiceNo"]?.GetValue<string>() ?? string.Empty;
        Assert.Equal("Payable", invoice?["direction"]?.GetValue<string>());
        Assert.Equal(payableId, invoice?["sourceId"]?.GetValue<Guid>());
        Assert.Equal(payableNo, invoice?["sourceNo"]?.GetValue<string>());
        Assert.Equal("测试供应商", invoice?["counterpartyName"]?.GetValue<string>());
        Assert.Equal(250m, invoice?["grossAmount"]?.GetValue<decimal>());
        Assert.Equal(payableNetAmount, invoice?["netAmount"]?.GetValue<decimal>());
        Assert.Equal(payableTaxAmount, invoice?["taxAmount"]?.GetValue<decimal>());
        Assert.False(string.IsNullOrWhiteSpace(invoiceNo));

        var invoicesResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Get, "/api/finance/invoices");
        invoicesResponse.EnsureSuccessStatusCode();
        var invoices = await invoicesResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.Contains(invoices ?? [], entry => entry?["invoiceNo"]?.GetValue<string>() == invoiceNo);

        var duplicateInvoiceResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/invoices", new
        {
            direction = "Payable",
            sourceId = payableId,
            invoiceDate = "2099-10-16",
            note = "重复发票"
        });
        Assert.Equal(HttpStatusCode.BadRequest, duplicateInvoiceResponse.StatusCode);

        var voucherResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/vouchers/from-business-document", new
        {
            accountingPeriodId = periodId,
            documentDate = "2099-10-15",
            sourceType = "Payable",
            sourceId = payableId,
            debitAccountId,
            creditAccountId,
            summary = "测试应付业务凭证"
        });
        voucherResponse.EnsureSuccessStatusCode();
        var voucher = await voucherResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Payable", voucher?["sourceType"]?.GetValue<string>());
        Assert.Equal(payableId, voucher?["sourceId"]?.GetValue<Guid>());
        Assert.Equal(payableNo, voucher?["sourceNo"]?.GetValue<string>());
        Assert.Equal(250m, voucher?["totalDebit"]?.GetValue<decimal>());
        Assert.Equal(250m, voucher?["totalCredit"]?.GetValue<decimal>());

        var duplicateVoucherResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/vouchers/from-business-document", new
        {
            accountingPeriodId = periodId,
            documentDate = "2099-10-16",
            sourceType = "Payable",
            sourceId = payableId,
            debitAccountId,
            creditAccountId,
            summary = "重复业务凭证"
        });
        Assert.Equal(HttpStatusCode.BadRequest, duplicateVoucherResponse.StatusCode);

        var agingResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Get, "/api/finance/aging");
        agingResponse.EnsureSuccessStatusCode();
        var aging = await agingResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.True((aging?["payables"]?["totalOpenAmount"]?.GetValue<decimal>() ?? 0m) >= 250m);
        Assert.True((aging?["payables"]?["openCount"]?.GetValue<int>() ?? 0) >= 1);
        Assert.Contains(aging?["payables"]?["buckets"]?.AsArray() ?? [], bucket =>
            bucket?["bucket"]?.GetValue<string>() == "Current" &&
            (bucket?["amount"]?.GetValue<decimal>() ?? 0m) >= 250m);

        var bankAccountNo = $"6222{suffix}";
        var bankAccountResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/bank-accounts", new
        {
            accountNo = bankAccountNo,
            accountName = "测试基本户",
            bankName = "测试银行",
            currencyCode = "CNY",
            isEnabled = true
        });
        bankAccountResponse.EnsureSuccessStatusCode();
        var bankAccount = await bankAccountResponse.Content.ReadFromJsonAsync<JsonObject>();
        var bankAccountId = bankAccount?["id"]?.GetValue<Guid>() ?? Guid.Empty;
        Assert.NotEqual(Guid.Empty, bankAccountId);
        Assert.Equal(bankAccountNo, bankAccount?["accountNo"]?.GetValue<string>());
        Assert.Equal("CNY", bankAccount?["currencyCode"]?.GetValue<string>());

        var bankAccountsResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Get, "/api/finance/bank-accounts");
        bankAccountsResponse.EnsureSuccessStatusCode();
        var bankAccounts = await bankAccountsResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.Contains(bankAccounts ?? [], entry => entry?["accountNo"]?.GetValue<string>() == bankAccountNo);

        var settlementResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/settlements", new
        {
            targetType = "Payable",
            targetId = payableId,
            amount = 250m,
            bankAccountId,
            method = "银行转账",
            note = "测试付款"
        });
        settlementResponse.EnsureSuccessStatusCode();
        var settlement = await settlementResponse.Content.ReadFromJsonAsync<JsonObject>();
        var settlementId = settlement?["id"]?.GetValue<Guid>() ?? Guid.Empty;
        var settlementNo = settlement?["settlementNo"]?.GetValue<string>() ?? string.Empty;
        Assert.NotEqual(Guid.Empty, settlementId);
        Assert.False(string.IsNullOrWhiteSpace(settlementNo));
        Assert.Equal(bankAccountId, settlement?["bankAccountId"]?.GetValue<Guid>());
        Assert.Equal(bankAccountNo, settlement?["bankAccountNo"]?.GetValue<string>());
        Assert.Equal("Unmatched", settlement?["reconciliationStatus"]?.GetValue<string>());

        var bankStatementLineResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/bank-statement-lines", new
        {
            bankAccountId,
            transactionDate = "2099-10-15",
            direction = "Outflow",
            amount = 250m,
            counterpartyName = "测试供应商",
            bankReferenceNo = $"BNK{suffix}",
            summary = "付款"
        });
        bankStatementLineResponse.EnsureSuccessStatusCode();
        var bankStatementLine = await bankStatementLineResponse.Content.ReadFromJsonAsync<JsonObject>();
        var bankStatementLineId = bankStatementLine?["id"]?.GetValue<Guid>() ?? Guid.Empty;
        var statementNo = bankStatementLine?["statementNo"]?.GetValue<string>() ?? string.Empty;
        Assert.NotEqual(Guid.Empty, bankStatementLineId);
        Assert.False(string.IsNullOrWhiteSpace(statementNo));
        Assert.Equal(bankAccountId, bankStatementLine?["bankAccountId"]?.GetValue<Guid>());
        Assert.Equal("Outflow", bankStatementLine?["direction"]?.GetValue<string>());
        Assert.Equal("Unmatched", bankStatementLine?["reconciliationStatus"]?.GetValue<string>());

        var reconcileResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Post, "/api/finance/bank-statement-lines/reconcile", new
        {
            bankStatementLineId,
            settlementId
        });
        reconcileResponse.EnsureSuccessStatusCode();
        var reconciledLine = await reconcileResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Matched", reconciledLine?["reconciliationStatus"]?.GetValue<string>());
        Assert.Equal(settlementId, reconciledLine?["settlementId"]?.GetValue<Guid>());
        Assert.Equal(settlementNo, reconciledLine?["settlementNo"]?.GetValue<string>());

        var bankStatementLinesResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Get, "/api/finance/bank-statement-lines");
        bankStatementLinesResponse.EnsureSuccessStatusCode();
        var bankStatementLines = await bankStatementLinesResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.Contains(bankStatementLines ?? [], entry =>
            entry?["statementNo"]?.GetValue<string>() == statementNo &&
            entry?["reconciliationStatus"]?.GetValue<string>() == "Matched");

        var settlementsResponse = await SendAuthorizedAsync(accessToken, HttpMethod.Get, "/api/finance/settlements");
        settlementsResponse.EnsureSuccessStatusCode();
        var settlements = await settlementsResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.Contains(settlements ?? [], entry =>
            entry?["settlementNo"]?.GetValue<string>() == settlementNo &&
            entry?["reconciliationStatus"]?.GetValue<string>() == "Matched" &&
            entry?["bankStatementNo"]?.GetValue<string>() == statementNo);
    }

    /// <summary>
    /// Login As Admin Async。
    /// </summary>
    private async Task<string> LoginAsAdminAsync()
    {
        var response = await client.PostAsJsonAsync("/api/platform/auth/login", new
        {
            userName = "admin",
            password = "Admin@123456"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        var accessToken = body?["accessToken"]?.GetValue<string>();

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        return accessToken!;
    }

    /// <summary>
    /// Send Authorized Async。
    /// </summary>
    /// <param name="accessToken">access Token 参数。</param>
    /// <param name="method">HTTP 方法或业务处理方式。</param>
    /// <param name="path">请求路径。</param>
    /// <param name="body">请求正文。</param>
    private async Task<HttpResponseMessage> SendAuthorizedAsync(string accessToken, HttpMethod method, string path, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await client.SendAsync(request);
    }

    /// <summary>
    /// Json Values。
    /// </summary>
    /// <param name="node">JSON 节点。</param>
    private static IEnumerable<string> JsonValues(JsonNode? node)
    {
        return node?.AsArray()
            .Select(x => x?.GetValue<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            ?? [];
    }
}
