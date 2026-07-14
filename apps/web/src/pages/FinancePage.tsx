import { CalendarDays, FilePlus2, HandCoins, Lock, Plus, RefreshCcw, Save, Unlock } from "lucide-react";
import { useCallback, useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { Link } from "react-router-dom";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type {
  AccountingAccount,
  AccountingPeriod,
  AgingSide,
  BankAccount,
  BankStatementLine,
  FinanceInvoice,
  FinanceAgingSnapshot,
  FinanceReportSnapshot,
  GeneralLedgerVoucher,
  InventoryIssue,
  InventoryReceipt,
  Payable,
  ProcurementOrder,
  Receivable,
  SalesOrder,
  Settlement,
} from "../types/api";

const accountTypes = ["Asset", "Liability", "Equity", "Revenue", "Expense", "Cost"] as const;

const loadEmptyAccountingAccounts = () => Promise.resolve<AccountingAccount[]>([]);
const loadEmptyAccountingPeriods = () => Promise.resolve<AccountingPeriod[]>([]);
const loadEmptyGeneralLedgerVouchers = () => Promise.resolve<GeneralLedgerVoucher[]>([]);
const loadEmptyFinanceAging = () => Promise.resolve<FinanceAgingSnapshot>({
  asOfDate: new Date().toISOString().slice(0, 10),
  payables: { totalOpenAmount: 0, totalOverdueAmount: 0, openCount: 0, overdueCount: 0, buckets: [], entries: [] },
  receivables: { totalOpenAmount: 0, totalOverdueAmount: 0, openCount: 0, overdueCount: 0, buckets: [], entries: [] },
});
const loadEmptyFinanceInvoices = () => Promise.resolve<FinanceInvoice[]>([]);
const loadEmptyFinanceReportSnapshot = () => Promise.resolve<FinanceReportSnapshot>({
  accountingPeriodId: null,
  accountingPeriodName: "全部期间",
  startDate: null,
  endDate: null,
  approvedVoucherCount: 0,
  totalDebit: 0,
  totalCredit: 0,
  isBalanced: true,
  trialBalance: [],
  incomeStatement: { revenue: 0, cost: 0, expense: 0, profit: 0 },
  balanceSheet: { assets: 0, liabilities: 0, equity: 0, retainedEarnings: 0, totalLiabilitiesAndEquity: 0, difference: 0 },
});
const loadEmptyBankAccounts = () => Promise.resolve<BankAccount[]>([]);
const loadEmptyBankStatementLines = () => Promise.resolve<BankStatementLine[]>([]);
const loadEmptyPayables = () => Promise.resolve<Payable[]>([]);
const loadEmptyReceivables = () => Promise.resolve<Receivable[]>([]);
const loadEmptySettlements = () => Promise.resolve<Settlement[]>([]);
const loadEmptyReceipts = () => Promise.resolve<InventoryReceipt[]>([]);
const loadEmptyIssues = () => Promise.resolve<InventoryIssue[]>([]);
const loadEmptyProcurementOrders = () => Promise.resolve<ProcurementOrder[]>([]);
const loadEmptySalesOrders = () => Promise.resolve<SalesOrder[]>([]);

type SettlementForm = {
  amount: number;
  bankAccountId: string;
  method: string;
  note: string;
};

type AccountingAccountForm = {
  id?: string | null;
  code: string;
  name: string;
  type: string;
  parentAccountId: string;
  isActive: boolean;
};

type AccountingPeriodForm = {
  year: number;
  month: number;
};

type VoucherLineForm = {
  accountingAccountId: string;
  summary: string;
  debitAmount: number;
  creditAmount: number;
};

type VoucherForm = {
  accountingPeriodId: string;
  documentDate: string;
  summary: string;
  lines: VoucherLineForm[];
};

type BusinessVoucherSourceType = "Payable" | "Receivable" | "Settlement";
type FinanceInvoiceDirection = "Payable" | "Receivable";

type BusinessVoucherForm = {
  accountingPeriodId: string;
  documentDate: string;
  debitAccountId: string;
  creditAccountId: string;
  summary: string;
};

type InvoiceForm = {
  invoiceDate: string;
  note: string;
};

type BankAccountForm = {
  id?: string | null;
  accountNo: string;
  accountName: string;
  bankName: string;
  currencyCode: string;
  isEnabled: boolean;
};

type BankStatementLineForm = {
  bankAccountId: string;
  transactionDate: string;
  direction: "Inflow" | "Outflow";
  amount: number;
  counterpartyName: string;
  bankReferenceNo: string;
  summary: string;
};

function formatDate(value: string) {
  return new Intl.DateTimeFormat("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatDateOnly(value: string) {
  return new Intl.DateTimeFormat("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  }).format(new Date(`${value}T00:00:00`));
}

function formatAmount(value: number) {
  return new Intl.NumberFormat("zh-CN", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

function formatTaxRate(value: number) {
  return `${new Intl.NumberFormat("zh-CN", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value * 100)}%`;
}

function accountTypeText(type: string) {
  switch (type) {
    case "Asset":
      return "资产";
    case "Liability":
      return "负债";
    case "Equity":
      return "权益";
    case "Revenue":
      return "收入";
    case "Expense":
      return "费用";
    case "Cost":
      return "成本";
    default:
      return type;
  }
}

function financeStatusText(status: string) {
  switch (status) {
    case "Open":
      return "未结";
    case "Partial":
      return "部分结算";
    case "Settled":
      return "已结清";
    default:
      return status;
  }
}

function periodStatusText(status: string) {
  return status === "Closed" ? "已关账" : status === "Open" ? "打开" : status;
}

function voucherStatusText(status: string) {
  switch (status) {
    case "Draft":
      return "草稿";
    case "Submitted":
      return "待审核";
    case "Approved":
      return "已审核";
    case "Rejected":
      return "已驳回";
    default:
      return status;
  }
}

function sourceTypeText(sourceType: string) {
  switch (sourceType) {
    case "Manual":
      return "手工凭证";
    case "Payable":
      return "应付记录";
    case "Receivable":
      return "应收记录";
    case "Settlement":
      return "结算记录";
    case "ProcurementOrder":
      return "采购订单";
    case "InventoryReceipt":
      return "采购入库";
    case "SalesOrder":
      return "销售订单";
    case "InventoryIssue":
      return "销售出库";
    default:
      return sourceType;
  }
}

function targetTypeText(targetType: string) {
  return targetType === "Payable" ? "应付" : targetType === "Receivable" ? "应收" : targetType;
}

function bankDirectionText(direction: string) {
  return direction === "Inflow" ? "收入" : direction === "Outflow" ? "支出" : direction;
}

function reconciliationStatusText(status: string) {
  return status === "Matched" ? "已对账" : status === "Unmatched" ? "未对账" : status;
}

function settlementDirection(entry: Settlement) {
  return entry.targetType === "Receivable" ? "Inflow" : "Outflow";
}

function amountsEqual(left: number, right: number) {
  return Math.abs(left - right) < 0.005;
}

function businessVoucherSourceKey(sourceType: string, sourceId: string) {
  return `${sourceType}:${sourceId}`;
}

function invoiceSourceKey(direction: string, sourceId: string) {
  return `${direction}:${sourceId}`;
}

function agingRiskText(overdueDays: number) {
  return overdueDays > 0 ? `逾期 ${overdueDays} 天` : "未到期";
}

/** 财务页面，覆盖会计科目、期间、凭证、报表、往来发票、银行流水和结算闭环。 */
export function FinancePage() {
  const { hasPermission } = useAuth();
  const canReadFinance = hasPermission(platformPermissions.financeRead);
  const canManagePayables = hasPermission(platformPermissions.financePayableManage);
  const canManageReceivables = hasPermission(platformPermissions.financeReceivableManage);
  const canManageSettlements = hasPermission(platformPermissions.financeSettlementManage);
  const canManageAccounting = hasPermission(platformPermissions.financeAccountingManage);
  const canManageVouchers = hasPermission(platformPermissions.financeVoucherManage);
  const canReviewVouchers = hasPermission(platformPermissions.financeVoucherReview);
  const canReadInventory = hasPermission(platformPermissions.inventoryRead);
  const canReadProcurement = hasPermission(platformPermissions.procurementRead);
  const canReadSales = hasPermission(platformPermissions.salesRead);

  const [reportPeriodId, setReportPeriodId] = useState("");
  const loadFinanceReportSnapshot = useCallback(
    () => (canReadFinance ? api.getFinanceReportSnapshot(reportPeriodId || undefined) : loadEmptyFinanceReportSnapshot()),
    [canReadFinance, reportPeriodId],
  );
  const accountsQuery = useAsyncData(canReadFinance ? api.listAccountingAccounts : loadEmptyAccountingAccounts);
  const periodsQuery = useAsyncData(canReadFinance ? api.listAccountingPeriods : loadEmptyAccountingPeriods);
  const vouchersQuery = useAsyncData(canReadFinance ? api.listGeneralLedgerVouchers : loadEmptyGeneralLedgerVouchers);
  const agingQuery = useAsyncData(canReadFinance ? api.getFinanceAging : loadEmptyFinanceAging);
  const reportsQuery = useAsyncData(loadFinanceReportSnapshot, `${canReadFinance}|${reportPeriodId}`);
  const invoicesQuery = useAsyncData(canReadFinance ? api.listFinanceInvoices : loadEmptyFinanceInvoices);
  const bankAccountsQuery = useAsyncData(canReadFinance ? api.listBankAccounts : loadEmptyBankAccounts);
  const bankStatementLinesQuery = useAsyncData(canReadFinance ? api.listBankStatementLines : loadEmptyBankStatementLines);
  const payablesQuery = useAsyncData(canReadFinance ? api.listPayables : loadEmptyPayables);
  const receivablesQuery = useAsyncData(canReadFinance ? api.listReceivables : loadEmptyReceivables);
  const settlementsQuery = useAsyncData(canReadFinance ? api.listSettlements : loadEmptySettlements);
  const receiptsQuery = useAsyncData(canReadInventory ? api.listInventoryReceipts : loadEmptyReceipts);
  const issuesQuery = useAsyncData(canReadInventory ? api.listInventoryIssues : loadEmptyIssues);
  const procurementOrdersQuery = useAsyncData(canReadProcurement ? api.listOrders : loadEmptyProcurementOrders);
  const salesOrdersQuery = useAsyncData(canReadSales ? api.listSalesOrders : loadEmptySalesOrders);

  const [payableAmounts, setPayableAmounts] = useState<Record<string, number>>({});
  const [receivableAmounts, setReceivableAmounts] = useState<Record<string, number>>({});
  const [settlementForms, setSettlementForms] = useState<Record<string, SettlementForm>>({});
  const [accountForm, setAccountForm] = useState<AccountingAccountForm>({
    code: "",
    name: "",
    type: "Asset",
    parentAccountId: "",
    isActive: true,
  });
  const [periodForm, setPeriodForm] = useState<AccountingPeriodForm>(() => {
    const now = new Date();
    return { year: now.getFullYear(), month: now.getMonth() + 1 };
  });
  const [voucherForm, setVoucherForm] = useState<VoucherForm>(() => {
    const today = new Date().toISOString().slice(0, 10);
    return {
      accountingPeriodId: "",
      documentDate: today,
      summary: "",
      lines: [
        { accountingAccountId: "", summary: "", debitAmount: 0, creditAmount: 0 },
        { accountingAccountId: "", summary: "", debitAmount: 0, creditAmount: 0 },
      ],
    };
  });
  const [businessVoucherForms, setBusinessVoucherForms] = useState<Record<string, BusinessVoucherForm>>({});
  const [invoiceForms, setInvoiceForms] = useState<Record<string, InvoiceForm>>({});
  const [bankAccountForm, setBankAccountForm] = useState<BankAccountForm>({
    accountNo: "",
    accountName: "",
    bankName: "",
    currencyCode: "CNY",
    isEnabled: true,
  });
  const [bankStatementLineForm, setBankStatementLineForm] = useState<BankStatementLineForm>(() => ({
    bankAccountId: "",
    transactionDate: new Date().toISOString().slice(0, 10),
    direction: "Outflow",
    amount: 0,
    counterpartyName: "",
    bankReferenceNo: "",
    summary: "",
  }));
  const [reviewNotes, setReviewNotes] = useState<Record<string, string>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const accounts = accountsQuery.data ?? [];
  const periods = periodsQuery.data ?? [];
  const vouchers = vouchersQuery.data ?? [];
  const aging = agingQuery.data;
  const financeReport = reportsQuery.data;
  const invoices = invoicesQuery.data ?? [];
  const bankAccounts = bankAccountsQuery.data ?? [];
  const bankStatementLines = bankStatementLinesQuery.data ?? [];
  const payables = payablesQuery.data ?? [];
  const receivables = receivablesQuery.data ?? [];
  const settlements = settlementsQuery.data ?? [];
  const receipts = receiptsQuery.data ?? [];
  const issues = issuesQuery.data ?? [];
  const procurementOrders = procurementOrdersQuery.data ?? [];
  const salesOrders = salesOrdersQuery.data ?? [];

  const coveredPayableOrderIds = useMemo(() => new Set(payables.map((entry) => entry.procurementOrderId)), [payables]);
  const coveredPayableReceiptIds = useMemo(
    () => new Set(payables.map((entry) => entry.inventoryReceiptId).filter(Boolean)),
    [payables],
  );
  const coveredReceivableOrderIds = useMemo(() => new Set(receivables.map((entry) => entry.salesOrderId)), [receivables]);
  const coveredReceivableIssueIds = useMemo(
    () => new Set(receivables.map((entry) => entry.inventoryIssueId).filter(Boolean)),
    [receivables],
  );

  const payableReceiptCandidates = receipts.filter(
    (entry) => !coveredPayableReceiptIds.has(entry.id) && !coveredPayableOrderIds.has(entry.procurementOrderId),
  );
  const receiptOrderIds = new Set(receipts.map((entry) => entry.procurementOrderId));
  const payableOrderCandidates = procurementOrders.filter(
    (entry) => entry.status === "Received" && !coveredPayableOrderIds.has(entry.id) && !receiptOrderIds.has(entry.id),
  );

  const receivableIssueCandidates = issues.filter(
    (entry) => !coveredReceivableIssueIds.has(entry.id) && !coveredReceivableOrderIds.has(entry.salesOrderId),
  );
  const issueOrderIds = new Set(issues.map((entry) => entry.salesOrderId));
  const receivableOrderCandidates = salesOrders.filter(
    (entry) => entry.status === "Shipped" && !coveredReceivableOrderIds.has(entry.id) && !issueOrderIds.has(entry.id),
  );

  const openPayables = payables.filter((entry) => entry.status !== "Settled");
  const openReceivables = receivables.filter((entry) => entry.status !== "Settled");
  const activeAccounts = accounts.filter((entry) => entry.isActive);
  const activeBankAccounts = bankAccounts.filter((entry) => entry.isEnabled);
  const unmatchedBankStatementLines = bankStatementLines.filter((entry) => entry.reconciliationStatus !== "Matched");
  const unmatchedSettlements = settlements.filter((entry) => entry.reconciliationStatus !== "Matched");
  const openPeriods = periods.filter((entry) => entry.status === "Open");
  const pendingVouchers = vouchers.filter((entry) => entry.status === "Draft" || entry.status === "Submitted");
  const voucherSourceKeys = useMemo(
    () => new Set(vouchers.filter((entry) => entry.sourceId).map((entry) => businessVoucherSourceKey(entry.sourceType, entry.sourceId!))),
    [vouchers],
  );
  const invoiceSourceKeys = useMemo(
    () => new Set(invoices.map((entry) => invoiceSourceKey(entry.direction, entry.sourceId))),
    [invoices],
  );
  const pendingVoucherCountsByPeriodId = useMemo(() => {
    const counts = new Map<string, number>();
    for (const entry of pendingVouchers) {
      counts.set(entry.accountingPeriodId, (counts.get(entry.accountingPeriodId) ?? 0) + 1);
    }
    return counts;
  }, [pendingVouchers]);
  const voucherDebitTotal = voucherForm.lines.reduce((total, line) => total + (Number(line.debitAmount) || 0), 0);
  const voucherCreditTotal = voucherForm.lines.reduce((total, line) => total + (Number(line.creditAmount) || 0), 0);
  const voucherCanSubmit =
    Boolean(voucherForm.accountingPeriodId) &&
    Boolean(voucherForm.documentDate) &&
    Boolean(voucherForm.summary.trim()) &&
    voucherForm.lines.length >= 2 &&
    voucherForm.lines.every((line) => line.accountingAccountId && ((line.debitAmount > 0 && line.creditAmount === 0) || (line.creditAmount > 0 && line.debitAmount === 0))) &&
    voucherDebitTotal > 0 &&
    voucherDebitTotal === voucherCreditTotal;

  async function runAction(actionKey: string, action: () => Promise<void>, successText?: string) {
    setBusyKey(actionKey);
    setMessage(null);
    setError(null);
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

  async function reloadAll() {
    const tasks: Promise<unknown>[] = [];
    if (canReadFinance) {
      tasks.push(
        accountsQuery.reload(),
        periodsQuery.reload(),
        vouchersQuery.reload(),
        agingQuery.reload(),
        reportsQuery.reload(),
        invoicesQuery.reload(),
        bankAccountsQuery.reload(),
        bankStatementLinesQuery.reload(),
        payablesQuery.reload(),
        receivablesQuery.reload(),
        settlementsQuery.reload(),
      );
    }
    if (canReadInventory) {
      tasks.push(receiptsQuery.reload(), issuesQuery.reload());
    }
    if (canReadProcurement) {
      tasks.push(procurementOrdersQuery.reload());
    }
    if (canReadSales) {
      tasks.push(salesOrdersQuery.reload());
    }
    await Promise.all(tasks);
  }

  function defaultSettlementForm(entry: Payable | Receivable): SettlementForm {
    return {
      amount: entry.remainingAmount,
      bankAccountId: activeBankAccounts.find((account) => account.currencyCode === entry.currencyCode)?.id ?? "",
      method: "银行转账",
      note: "",
    };
  }

  function getSettlementForm(key: string, entry: Payable | Receivable) {
    return settlementForms[key] ?? defaultSettlementForm(entry);
  }

  function setSettlementForm(key: string, entry: Payable | Receivable, patch: Partial<SettlementForm>) {
    setSettlementForms((current) => ({
      ...current,
      [key]: {
        ...(current[key] ?? defaultSettlementForm(entry)),
        ...patch,
      },
    }));
  }

  function defaultBusinessVoucherForm(defaultSummary: string): BusinessVoucherForm {
    return {
      accountingPeriodId: openPeriods[0]?.id ?? "",
      documentDate: new Date().toISOString().slice(0, 10),
      debitAccountId: "",
      creditAccountId: "",
      summary: defaultSummary,
    };
  }

  function getBusinessVoucherForm(key: string, defaultSummary: string) {
    return businessVoucherForms[key] ?? defaultBusinessVoucherForm(defaultSummary);
  }

  function setBusinessVoucherForm(key: string, defaultSummary: string, patch: Partial<BusinessVoucherForm>) {
    setBusinessVoucherForms((current) => ({
      ...current,
      [key]: {
        ...(current[key] ?? defaultBusinessVoucherForm(defaultSummary)),
        ...patch,
      },
    }));
  }

  function businessVoucherCanSubmit(form: BusinessVoucherForm) {
    return Boolean(
      form.accountingPeriodId &&
        form.documentDate &&
        form.debitAccountId &&
        form.creditAccountId &&
        form.debitAccountId !== form.creditAccountId,
    );
  }

  function defaultInvoiceForm(): InvoiceForm {
    return {
      invoiceDate: new Date().toISOString().slice(0, 10),
      note: "",
    };
  }

  function getInvoiceForm(key: string) {
    return invoiceForms[key] ?? defaultInvoiceForm();
  }

  function setInvoiceForm(key: string, patch: Partial<InvoiceForm>) {
    setInvoiceForms((current) => ({
      ...current,
      [key]: {
        ...(current[key] ?? defaultInvoiceForm()),
        ...patch,
      },
    }));
  }

  function resetAccountForm() {
    setAccountForm({ code: "", name: "", type: "Asset", parentAccountId: "", isActive: true });
  }

  function resetBankAccountForm() {
    setBankAccountForm({
      accountNo: "",
      accountName: "",
      bankName: "",
      currencyCode: "CNY",
      isEnabled: true,
    });
  }

  function editBankAccount(entry: BankAccount) {
    setBankAccountForm({
      id: entry.id,
      accountNo: entry.accountNo,
      accountName: entry.accountName,
      bankName: entry.bankName,
      currencyCode: entry.currencyCode,
      isEnabled: entry.isEnabled,
    });
  }

  function resetBankStatementLineForm() {
    setBankStatementLineForm({
      bankAccountId: activeBankAccounts[0]?.id ?? "",
      transactionDate: new Date().toISOString().slice(0, 10),
      direction: "Outflow",
      amount: 0,
      counterpartyName: "",
      bankReferenceNo: "",
      summary: "",
    });
  }

  function editAccount(entry: AccountingAccount) {
    setAccountForm({
      id: entry.id,
      code: entry.code,
      name: entry.name,
      type: entry.type,
      parentAccountId: entry.parentAccountId ?? "",
      isActive: entry.isActive,
    });
  }

  async function submitAccountingAccount() {
    if (!accountForm.code.trim() || !accountForm.name.trim() || !accountTypes.includes(accountForm.type as (typeof accountTypes)[number])) {
      setError("请填写有效的科目编码、名称和类型。");
      return;
    }

    await runAction("accounting-account-save", async () => {
      await api.upsertAccountingAccount({
        id: accountForm.id ?? null,
        code: accountForm.code.trim(),
        name: accountForm.name.trim(),
        type: accountForm.type,
        parentAccountId: accountForm.parentAccountId || null,
        isActive: accountForm.isActive,
      });
      resetAccountForm();
      await accountsQuery.reload();
    }, accountForm.id ? "会计科目已保存。" : "会计科目已创建。");
  }

  async function toggleAccount(entry: AccountingAccount) {
    await runAction(`account-toggle-${entry.id}`, async () => {
      await api.upsertAccountingAccount({
        id: entry.id,
        code: entry.code,
        name: entry.name,
        type: entry.type,
        parentAccountId: entry.parentAccountId ?? null,
        isActive: !entry.isActive,
      });
      await accountsQuery.reload();
    }, entry.isActive ? `${entry.code} 已停用。` : `${entry.code} 已启用。`);
  }

  async function submitBankAccount() {
    if (!bankAccountForm.accountNo.trim() || !bankAccountForm.accountName.trim() || !bankAccountForm.bankName.trim()) {
      setError("请填写银行账号、账户名称和开户行。");
      return;
    }

    await runAction("bank-account-save", async () => {
      await api.upsertBankAccount({
        id: bankAccountForm.id ?? null,
        accountNo: bankAccountForm.accountNo.trim(),
        accountName: bankAccountForm.accountName.trim(),
        bankName: bankAccountForm.bankName.trim(),
        currencyCode: bankAccountForm.currencyCode.trim() || "CNY",
        isEnabled: bankAccountForm.isEnabled,
      });
      resetBankAccountForm();
      await bankAccountsQuery.reload();
    }, bankAccountForm.id ? "银行账户已保存。" : "银行账户已创建。");
  }

  async function toggleBankAccount(entry: BankAccount) {
    await runAction(`bank-account-toggle-${entry.id}`, async () => {
      await api.upsertBankAccount({
        id: entry.id,
        accountNo: entry.accountNo,
        accountName: entry.accountName,
        bankName: entry.bankName,
        currencyCode: entry.currencyCode,
        isEnabled: !entry.isEnabled,
      });
      await bankAccountsQuery.reload();
    }, entry.isEnabled ? `${entry.accountNo} 已停用。` : `${entry.accountNo} 已启用。`);
  }

  async function createBankStatementLine() {
    if (!bankStatementLineForm.bankAccountId || !bankStatementLineForm.transactionDate || bankStatementLineForm.amount <= 0) {
      setError("请选择银行账户、交易日期，并填写大于 0 的流水金额。");
      return;
    }

    await runAction("bank-statement-create", async () => {
      await api.createBankStatementLine({
        bankAccountId: bankStatementLineForm.bankAccountId,
        transactionDate: bankStatementLineForm.transactionDate,
        direction: bankStatementLineForm.direction,
        amount: bankStatementLineForm.amount,
        counterpartyName: bankStatementLineForm.counterpartyName.trim(),
        bankReferenceNo: bankStatementLineForm.bankReferenceNo.trim(),
        summary: bankStatementLineForm.summary.trim(),
      });
      resetBankStatementLineForm();
      await bankStatementLinesQuery.reload();
    }, "银行流水已录入。");
  }

  async function reconcileBankStatement(line: BankStatementLine, settlement: Settlement) {
    await runAction(`bank-reconcile-${line.id}-${settlement.id}`, async () => {
      await api.reconcileBankStatement({
        bankStatementLineId: line.id,
        settlementId: settlement.id,
      });
      await Promise.all([bankStatementLinesQuery.reload(), settlementsQuery.reload()]);
    }, `${line.statementNo} 已与 ${settlement.settlementNo} 完成对账。`);
  }

  async function createAccountingPeriod() {
    if (periodForm.year < 2000 || periodForm.year > 2100 || periodForm.month < 1 || periodForm.month > 12) {
      setError("请填写有效的会计期间年月。");
      return;
    }

    await runAction("accounting-period-create", async () => {
      await api.createAccountingPeriod(periodForm);
      await periodsQuery.reload();
    }, `${periodForm.year}-${String(periodForm.month).padStart(2, "0")} 会计期间已创建。`);
  }

  async function closeAccountingPeriod(entry: AccountingPeriod) {
    await runAction(`period-close-${entry.id}`, async () => {
      await api.closeAccountingPeriod(entry.id);
      await periodsQuery.reload();
    }, `${entry.name} 已关账。`);
  }

  async function reopenAccountingPeriod(entry: AccountingPeriod) {
    await runAction(`period-reopen-${entry.id}`, async () => {
      await api.reopenAccountingPeriod(entry.id);
      await periodsQuery.reload();
    }, `${entry.name} 已重新打开。`);
  }

  function updateVoucherLine(index: number, patch: Partial<VoucherLineForm>) {
    setVoucherForm((current) => ({
      ...current,
      lines: current.lines.map((line, lineIndex) => (lineIndex === index ? { ...line, ...patch } : line)),
    }));
  }

  function addVoucherLine() {
    setVoucherForm((current) => ({
      ...current,
      lines: [...current.lines, { accountingAccountId: "", summary: "", debitAmount: 0, creditAmount: 0 }],
    }));
  }

  function removeVoucherLine(index: number) {
    setVoucherForm((current) => ({
      ...current,
      lines: current.lines.filter((_, lineIndex) => lineIndex !== index),
    }));
  }

  function resetVoucherForm() {
    setVoucherForm({
      accountingPeriodId: "",
      documentDate: new Date().toISOString().slice(0, 10),
      summary: "",
      lines: [
        { accountingAccountId: "", summary: "", debitAmount: 0, creditAmount: 0 },
        { accountingAccountId: "", summary: "", debitAmount: 0, creditAmount: 0 },
      ],
    });
  }

  async function createManualVoucher() {
    if (!voucherCanSubmit) {
      setError("请填写打开期间、凭证日期、摘要，并确保分录借贷平衡。");
      return;
    }

    await runAction("voucher-create", async () => {
      await api.createManualVoucher({
        accountingPeriodId: voucherForm.accountingPeriodId,
        documentDate: voucherForm.documentDate,
        summary: voucherForm.summary.trim(),
        lines: voucherForm.lines.map((line) => ({
          accountingAccountId: line.accountingAccountId,
          summary: line.summary.trim(),
          debitAmount: Number(line.debitAmount) || 0,
          creditAmount: Number(line.creditAmount) || 0,
        })),
      });
      resetVoucherForm();
      await vouchersQuery.reload();
    }, "手工总账凭证已创建。");
  }

  async function createBusinessVoucher(sourceType: BusinessVoucherSourceType, sourceId: string, sourceNo: string, defaultSummary: string) {
    const key = businessVoucherSourceKey(sourceType, sourceId);
    const form = getBusinessVoucherForm(key, defaultSummary);
    if (!businessVoucherCanSubmit(form)) {
      setError("请选择打开期间、凭证日期，并填写不同的借方和贷方科目。");
      return;
    }

    await runAction(`business-voucher-${key}`, async () => {
      await api.createBusinessVoucher({
        accountingPeriodId: form.accountingPeriodId,
        documentDate: form.documentDate,
        sourceType,
        sourceId,
        debitAccountId: form.debitAccountId,
        creditAccountId: form.creditAccountId,
        summary: form.summary.trim() || defaultSummary,
      });
      setBusinessVoucherForms((current) => {
        const next = { ...current };
        delete next[key];
        return next;
      });
      await vouchersQuery.reload();
    }, `${sourceTypeText(sourceType)} ${sourceNo} 已生成总账凭证。`);
  }

  function renderBusinessVoucherControls(sourceType: BusinessVoucherSourceType, sourceId: string, sourceNo: string, defaultSummary: string) {
    const key = businessVoucherSourceKey(sourceType, sourceId);
    if (voucherSourceKeys.has(key)) {
      return <small>已生成总账凭证。</small>;
    }

    if (!canManageVouchers) {
      return <small>当前账号不能生成总账凭证。</small>;
    }

    if (openPeriods.length === 0 || activeAccounts.length < 2) {
      return <small>需要打开期间和至少两个启用科目后才能生成凭证。</small>;
    }

    const form = getBusinessVoucherForm(key, defaultSummary);
    const canSubmit = businessVoucherCanSubmit(form);
    return (
      <>
        <select
          value={form.accountingPeriodId}
          onChange={(event) => setBusinessVoucherForm(key, defaultSummary, { accountingPeriodId: event.target.value })}
        >
          {openPeriods.map((period) => (
            <option key={period.id} value={period.id}>{period.name}</option>
          ))}
        </select>
        <input
          type="date"
          value={form.documentDate}
          onChange={(event) => setBusinessVoucherForm(key, defaultSummary, { documentDate: event.target.value })}
        />
        <select
          value={form.debitAccountId}
          onChange={(event) => setBusinessVoucherForm(key, defaultSummary, { debitAccountId: event.target.value })}
        >
          <option value="">借方科目</option>
          {activeAccounts.map((account) => (
            <option key={account.id} value={account.id}>{account.code} {account.name}</option>
          ))}
        </select>
        <select
          value={form.creditAccountId}
          onChange={(event) => setBusinessVoucherForm(key, defaultSummary, { creditAccountId: event.target.value })}
        >
          <option value="">贷方科目</option>
          {activeAccounts.map((account) => (
            <option key={account.id} value={account.id}>{account.code} {account.name}</option>
          ))}
        </select>
        <input
          placeholder="凭证摘要"
          value={form.summary}
          onChange={(event) => setBusinessVoucherForm(key, defaultSummary, { summary: event.target.value })}
        />
        <button
          disabled={busyKey === `business-voucher-${key}` || !canSubmit}
          onClick={async () => createBusinessVoucher(sourceType, sourceId, sourceNo, defaultSummary)}
        >
          <FilePlus2 size={16} />
          <span>生成凭证</span>
        </button>
      </>
    );
  }

  async function createInvoice(direction: FinanceInvoiceDirection, sourceId: string, sourceNo: string) {
    const key = invoiceSourceKey(direction, sourceId);
    const form = getInvoiceForm(key);
    if (!form.invoiceDate) {
      setError("请选择税票日期。");
      return;
    }

    await runAction(`invoice-${key}`, async () => {
      await api.createFinanceInvoice({
        direction,
        sourceId,
        invoiceDate: form.invoiceDate,
        note: form.note.trim(),
      });
      setInvoiceForms((current) => {
        const next = { ...current };
        delete next[key];
        return next;
      });
      await invoicesQuery.reload();
    }, `${targetTypeText(direction)} ${sourceNo} 已登记税票。`);
  }

  function renderInvoiceControls(direction: FinanceInvoiceDirection, sourceId: string, sourceNo: string) {
    const key = invoiceSourceKey(direction, sourceId);
    if (invoiceSourceKeys.has(key)) {
      return <small>已登记税票。</small>;
    }

    if (!canManageSettlements) {
      return <small>当前账号不能登记税票。</small>;
    }

    const form = getInvoiceForm(key);
    return (
      <>
        <input
          type="date"
          value={form.invoiceDate}
          onChange={(event) => setInvoiceForm(key, { invoiceDate: event.target.value })}
        />
        <input
          placeholder="税票备注"
          value={form.note}
          onChange={(event) => setInvoiceForm(key, { note: event.target.value })}
        />
        <button
          type="button"
          disabled={busyKey === `invoice-${key}` || !form.invoiceDate}
          onClick={() => createInvoice(direction, sourceId, sourceNo)}
        >
          <FilePlus2 size={16} />
          <span>登记税票</span>
        </button>
      </>
    );
  }

  function renderAgingSide(title: string, side?: AgingSide) {
    const snapshot = side ?? { totalOpenAmount: 0, totalOverdueAmount: 0, openCount: 0, overdueCount: 0, buckets: [], entries: [] };
    const overdueEntries = snapshot.entries.filter((entry) => entry.overdueDays > 0);
    return (
      <div className="finance-source-panel">
        <div className="inventory-lines">
          <strong>{title}</strong>
          <span>未结 {formatAmount(snapshot.totalOpenAmount)}</span>
          <span>逾期 {formatAmount(snapshot.totalOverdueAmount)}</span>
          <span>{snapshot.overdueCount} 笔逾期</span>
        </div>
        {snapshot.buckets.length > 0 ? (
          <div className="stats-grid compact-stats">
            {snapshot.buckets.map((bucket) => (
              <StatTile
                key={bucket.bucket}
                label={bucket.bucketName}
                value={formatAmount(bucket.amount)}
                tone={bucket.bucket === "Current" ? "success" : bucket.amount > 0 ? "warning" : "default"}
              />
            ))}
          </div>
        ) : (
          <div className="section-note">暂无未结账龄数据。</div>
        )}
        {overdueEntries.length > 0 ? (
          <div className="table-shell">
            {overdueEntries.slice(0, 6).map((entry) => (
              <div key={entry.id} className="review-card">
                <div>
                  <strong>{entry.documentNo}</strong>
                  <p>{entry.counterpartyName} · {entry.sourceNo || "无来源单号"}</p>
                  <small>到期 {formatDateOnly(entry.dueDate)} · {agingRiskText(entry.overdueDays)} · {financeStatusText(entry.status)}</small>
                </div>
                <div className="inventory-balance">
                  <span className="inventory-movement-chip movement-negative">{formatAmount(entry.remainingAmount)}</span>
                  <small>{entry.currencyCode}</small>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="section-note">当前没有逾期未结记录。</div>
        )}
      </div>
    );
  }

  function matchingSettlementsForLine(line: BankStatementLine) {
    return unmatchedSettlements.filter(
      (settlement) =>
        settlement.bankAccountId === line.bankAccountId &&
        settlementDirection(settlement) === line.direction &&
        settlement.currencyCode === line.currencyCode &&
        amountsEqual(settlement.amount, line.amount),
    );
  }

  async function submitVoucher(entry: GeneralLedgerVoucher) {
    await runAction(`voucher-submit-${entry.id}`, async () => {
      await api.submitGeneralLedgerVoucher(entry.id);
      await vouchersQuery.reload();
    }, `${entry.voucherNo} 已提交审核。`);
  }

  async function approveVoucher(entry: GeneralLedgerVoucher) {
    await runAction(`voucher-approve-${entry.id}`, async () => {
      await api.approveGeneralLedgerVoucher(entry.id, reviewNotes[entry.id] ?? "");
      setReviewNotes((current) => {
        const next = { ...current };
        delete next[entry.id];
        return next;
      });
      await vouchersQuery.reload();
    }, `${entry.voucherNo} 已审核通过。`);
  }

  async function rejectVoucher(entry: GeneralLedgerVoucher) {
    await runAction(`voucher-reject-${entry.id}`, async () => {
      await api.rejectGeneralLedgerVoucher(entry.id, reviewNotes[entry.id] ?? "");
      setReviewNotes((current) => {
        const next = { ...current };
        delete next[entry.id];
        return next;
      });
      await vouchersQuery.reload();
    }, `${entry.voucherNo} 已驳回。`);
  }

  async function createPayableFromReceipt(entry: InventoryReceipt) {
    const amount = payableAmounts[entry.id] ?? 0;
    if (amount <= 0) {
      setError("请填写大于 0 的应付金额。");
      return;
    }

    await runAction(`payable-receipt-${entry.id}`, async () => {
      await api.createPayableFromReceipt({ inventoryReceiptId: entry.id, amount });
      setPayableAmounts((current) => {
        const next = { ...current };
        delete next[entry.id];
        return next;
      });
      await reloadAll();
    }, `${entry.receiptNo} 已生成应付记录。`);
  }

  async function createPayableFromOrder(entry: ProcurementOrder) {
    const amount = payableAmounts[entry.id] ?? 0;
    if (amount <= 0) {
      setError("请填写大于 0 的应付金额。");
      return;
    }

    await runAction(`payable-order-${entry.id}`, async () => {
      await api.createPayableFromOrder({ procurementOrderId: entry.id, amount });
      setPayableAmounts((current) => {
        const next = { ...current };
        delete next[entry.id];
        return next;
      });
      await reloadAll();
    }, `${entry.orderNo} 已生成应付记录。`);
  }

  async function createReceivableFromIssue(entry: InventoryIssue) {
    const amount = receivableAmounts[entry.id] ?? 0;
    if (amount <= 0) {
      setError("请填写大于 0 的应收金额。");
      return;
    }

    await runAction(`receivable-issue-${entry.id}`, async () => {
      await api.createReceivableFromIssue({ inventoryIssueId: entry.id, amount });
      setReceivableAmounts((current) => {
        const next = { ...current };
        delete next[entry.id];
        return next;
      });
      await reloadAll();
    }, `${entry.issueNo} 已生成应收记录。`);
  }

  async function createReceivableFromOrder(entry: SalesOrder) {
    const amount = receivableAmounts[entry.id] ?? 0;
    if (amount <= 0) {
      setError("请填写大于 0 的应收金额。");
      return;
    }

    await runAction(`receivable-order-${entry.id}`, async () => {
      await api.createReceivableFromOrder({ salesOrderId: entry.id, amount });
      setReceivableAmounts((current) => {
        const next = { ...current };
        delete next[entry.id];
        return next;
      });
      await reloadAll();
    }, `${entry.orderNo} 已生成应收记录。`);
  }

  async function createSettlement(targetType: "Payable" | "Receivable", entry: Payable | Receivable) {
    const key = `${targetType}-${entry.id}`;
    const form = getSettlementForm(key, entry);
    if (form.amount <= 0 || form.amount > entry.remainingAmount || !form.method.trim()) {
      setError("请填写有效结算金额和结算方式。");
      return;
    }

    if (!form.bankAccountId) {
      setError("请选择启用且币种匹配的银行账户。");
      return;
    }

    await runAction(`settlement-${key}`, async () => {
      await api.createSettlement({
        targetType,
        targetId: entry.id,
        amount: form.amount,
        bankAccountId: form.bankAccountId,
        method: form.method.trim(),
        note: form.note.trim(),
      });
      setSettlementForms((current) => {
        const next = { ...current };
        delete next[key];
        return next;
      });
      await reloadAll();
    }, `${targetTypeText(targetType)} ${"payableNo" in entry ? entry.payableNo : entry.receivableNo} 已完成结算。`);
  }

  if (!canReadFinance) {
    return (
      <PageShell title="财务结算台">
        <EmptyState title="无财务查看权限" description="当前账号不能读取应收、应付和结算记录。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="财务结算台"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "finance-refresh"}
          onClick={async () => {
            await runAction("finance-refresh", reloadAll, "财务数据已刷新。");
          }}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      }
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <section className="stats-grid">
        <StatTile label="会计科目" value={accounts.length} tone={activeAccounts.length > 0 ? "success" : "default"} />
        <StatTile label="打开期间" value={openPeriods.length} tone={openPeriods.length > 0 ? "success" : "warning"} />
        <StatTile label="总账凭证" value={vouchers.length} tone={pendingVouchers.length > 0 ? "warning" : "default"} />
        <StatTile label="未决凭证" value={pendingVouchers.length} tone={pendingVouchers.length > 0 ? "warning" : "success"} />
        <StatTile label="银行账户" value={activeBankAccounts.length} tone={activeBankAccounts.length > 0 ? "success" : "warning"} />
        <StatTile label="未对账流水" value={unmatchedBankStatementLines.length} tone={unmatchedBankStatementLines.length > 0 ? "warning" : "success"} />
        <StatTile label="未对账结算" value={unmatchedSettlements.length} tone={unmatchedSettlements.length > 0 ? "warning" : "success"} />
        <StatTile label="应付记录" value={payables.length} tone={openPayables.length > 0 ? "warning" : "default"} />
        <StatTile label="应收记录" value={receivables.length} tone={openReceivables.length > 0 ? "warning" : "default"} />
        <StatTile label="待结应付" value={openPayables.length} tone={openPayables.length > 0 ? "warning" : "success"} />
        <StatTile label="待结应收" value={openReceivables.length} tone={openReceivables.length > 0 ? "warning" : "success"} />
        <StatTile label="逾期应付" value={formatAmount(aging?.payables.totalOverdueAmount ?? 0)} tone={(aging?.payables.totalOverdueAmount ?? 0) > 0 ? "warning" : "success"} />
        <StatTile label="逾期应收" value={formatAmount(aging?.receivables.totalOverdueAmount ?? 0)} tone={(aging?.receivables.totalOverdueAmount ?? 0) > 0 ? "warning" : "success"} />
      </section>

      <SectionBlock title="账龄与到期风险" hint="按到期日汇总未结应付和应收，逾期记录会进入关账前风险检查视图。">
        {agingQuery.loading ? (
          <div className="section-note">正在加载账龄数据...</div>
        ) : agingQuery.error ? (
          <div className="section-note error">{agingQuery.error}</div>
        ) : aging ? (
          <div className="split-grid">
            {renderAgingSide("应付账龄", aging.payables)}
            {renderAgingSide("应收账龄", aging.receivables)}
          </div>
        ) : (
          <EmptyState title="暂无账龄数据" description="生成未结应付或应收后，这里会展示到期和逾期分布。" />
        )}
      </SectionBlock>

      <SectionBlock title="财务报表" hint="基于已审核总账凭证生成试算平衡、利润表和资产负债表基础口径。">
        <div className="finance-source-panel">
          <div className="section-note">报表期间</div>
          <div className="inventory-actions">
            <select value={reportPeriodId} onChange={(event) => setReportPeriodId(event.target.value)}>
              <option value="">全部期间</option>
              {periods.map((period) => (
                <option key={period.id} value={period.id}>{period.name}</option>
              ))}
            </select>
            <button
              type="button"
              className="secondary"
              disabled={busyKey === "finance-report-refresh" || reportsQuery.loading}
              onClick={() => runAction("finance-report-refresh", reportsQuery.reload, "财务报表已刷新。")}
            >
              <RefreshCcw size={16} />
              <span>刷新报表</span>
            </button>
          </div>
        </div>

        {reportsQuery.loading ? (
          <div className="section-note">正在加载财务报表...</div>
        ) : reportsQuery.error ? (
          <div className="section-note error">{reportsQuery.error}</div>
        ) : financeReport && financeReport.approvedVoucherCount > 0 ? (
          <>
            <div className="stats-grid compact-stats">
              <StatTile label="已审核凭证" value={financeReport.approvedVoucherCount} tone="success" />
              <StatTile label="借方合计" value={formatAmount(financeReport.totalDebit)} tone={financeReport.isBalanced ? "success" : "warning"} />
              <StatTile label="贷方合计" value={formatAmount(financeReport.totalCredit)} tone={financeReport.isBalanced ? "success" : "warning"} />
              <StatTile label="试算状态" value={financeReport.isBalanced ? "平衡" : "不平衡"} tone={financeReport.isBalanced ? "success" : "warning"} />
            </div>

            <div className="split-grid">
              <div className="finance-source-panel">
                <div className="inventory-lines">
                  <strong>利润表</strong>
                  <span>{financeReport.accountingPeriodName}</span>
                </div>
                <div className="inventory-lines">
                  <span>收入 {formatAmount(financeReport.incomeStatement.revenue)}</span>
                  <span>成本 {formatAmount(financeReport.incomeStatement.cost)}</span>
                  <span>费用 {formatAmount(financeReport.incomeStatement.expense)}</span>
                  <span>利润 {formatAmount(financeReport.incomeStatement.profit)}</span>
                </div>
              </div>
              <div className="finance-source-panel">
                <div className="inventory-lines">
                  <strong>资产负债表</strong>
                  <span>{financeReport.accountingPeriodName}</span>
                </div>
                <div className="inventory-lines">
                  <span>资产 {formatAmount(financeReport.balanceSheet.assets)}</span>
                  <span>负债 {formatAmount(financeReport.balanceSheet.liabilities)}</span>
                  <span>权益 {formatAmount(financeReport.balanceSheet.equity)}</span>
                  <span>未分配利润 {formatAmount(financeReport.balanceSheet.retainedEarnings)}</span>
                  <span>差额 {formatAmount(financeReport.balanceSheet.difference)}</span>
                </div>
              </div>
            </div>

            <div className="table-shell">
              {financeReport.trialBalance.map((line) => (
                <div key={line.accountingAccountId} className="review-card finance-card">
                  <div>
                    <strong>{line.accountCode} · {line.accountName}</strong>
                    <p>{accountTypeText(line.accountType)}</p>
                  </div>
                  <div className="inventory-actions">
                    <small>借方发生 {formatAmount(line.debitAmount)}</small>
                    <small>贷方发生 {formatAmount(line.creditAmount)}</small>
                    <small>借方余额 {formatAmount(line.endingDebit)}</small>
                    <small>贷方余额 {formatAmount(line.endingCredit)}</small>
                  </div>
                </div>
              ))}
            </div>
          </>
        ) : (
          <EmptyState
            title="暂无已审核凭证报表"
            description="审核通过总账凭证后，这里会生成试算平衡和基础财务报表。"
          />
        )}
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="银行账户" hint="维护付款、收款和对账使用的资金账户。">
          {canManageSettlements ? (
            <div className="finance-source-panel">
              <div className="section-note">{bankAccountForm.id ? "编辑银行账户" : "新增银行账户"}</div>
              <div className="inventory-actions">
                <input
                  placeholder="银行账号"
                  value={bankAccountForm.accountNo}
                  disabled={Boolean(bankAccountForm.id)}
                  onChange={(event) => setBankAccountForm({ ...bankAccountForm, accountNo: event.target.value })}
                />
                <input
                  placeholder="账户名称"
                  value={bankAccountForm.accountName}
                  onChange={(event) => setBankAccountForm({ ...bankAccountForm, accountName: event.target.value })}
                />
                <input
                  placeholder="开户行"
                  value={bankAccountForm.bankName}
                  onChange={(event) => setBankAccountForm({ ...bankAccountForm, bankName: event.target.value })}
                />
                <input
                  placeholder="币种"
                  value={bankAccountForm.currencyCode}
                  onChange={(event) => setBankAccountForm({ ...bankAccountForm, currencyCode: event.target.value.toUpperCase() })}
                />
                <label className="inline-check">
                  <input
                    type="checkbox"
                    checked={bankAccountForm.isEnabled}
                    onChange={(event) => setBankAccountForm({ ...bankAccountForm, isEnabled: event.target.checked })}
                  />
                  启用
                </label>
                <button
                  disabled={
                    busyKey === "bank-account-save" ||
                    !bankAccountForm.accountNo.trim() ||
                    !bankAccountForm.accountName.trim() ||
                    !bankAccountForm.bankName.trim()
                  }
                  onClick={submitBankAccount}
                >
                  {bankAccountForm.id ? <Save size={16} /> : <Plus size={16} />}
                  <span>{bankAccountForm.id ? "保存账户" : "新增账户"}</span>
                </button>
                {bankAccountForm.id ? (
                  <button type="button" className="secondary" onClick={resetBankAccountForm}>
                    取消编辑
                  </button>
                ) : null}
              </div>
            </div>
          ) : (
            <div className="section-note">当前账号只能查看银行账户，不能维护账户。</div>
          )}

          {bankAccountsQuery.loading ? (
            <div className="section-note">正在加载银行账户...</div>
          ) : bankAccountsQuery.error ? (
            <div className="section-note error">{bankAccountsQuery.error}</div>
          ) : bankAccounts.length > 0 ? (
            <div className="table-shell">
              {bankAccounts.map((entry) => (
                <div key={entry.id} className="review-card">
                  <div>
                    <strong>{entry.accountNo} · {entry.accountName}</strong>
                    <p>{entry.bankName} · {entry.currencyCode} · {entry.isEnabled ? "启用" : "停用"}</p>
                    <small>{entry.updatedBy} · {formatDate(entry.updatedAtUtc)}</small>
                  </div>
                  {canManageSettlements ? (
                    <div className="inventory-actions">
                      <button type="button" className="secondary" onClick={() => editBankAccount(entry)}>
                        编辑
                      </button>
                      <button
                        type="button"
                        disabled={busyKey === `bank-account-toggle-${entry.id}`}
                        onClick={() => toggleBankAccount(entry)}
                      >
                        {entry.isEnabled ? "停用" : "启用"}
                      </button>
                    </div>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <EmptyState
              title="暂无银行账户"
              description={canManageSettlements ? "新增第一个银行账户后，付款和收款单才能选择资金账户。" : "需要结算维护权限后才能新增银行账户。"}
            />
          )}
        </SectionBlock>

        <SectionBlock title="银行流水" hint="录入银行交易明细，并保留银行参考号、摘要和对账状态。">
          {canManageSettlements && activeBankAccounts.length > 0 ? (
            <div className="finance-source-panel">
              <div className="section-note">新增银行流水</div>
              <div className="inventory-actions">
                <select
                  value={bankStatementLineForm.bankAccountId}
                  onChange={(event) => setBankStatementLineForm({ ...bankStatementLineForm, bankAccountId: event.target.value })}
                >
                  <option value="">选择银行账户</option>
                  {activeBankAccounts.map((account) => (
                    <option key={account.id} value={account.id}>{account.accountNo} · {account.accountName} · {account.currencyCode}</option>
                  ))}
                </select>
                <input
                  type="date"
                  value={bankStatementLineForm.transactionDate}
                  onChange={(event) => setBankStatementLineForm({ ...bankStatementLineForm, transactionDate: event.target.value })}
                />
                <select
                  value={bankStatementLineForm.direction}
                  onChange={(event) => setBankStatementLineForm({ ...bankStatementLineForm, direction: event.target.value as "Inflow" | "Outflow" })}
                >
                  <option value="Outflow">支出</option>
                  <option value="Inflow">收入</option>
                </select>
                <input
                  type="number"
                  min={0.01}
                  step="0.01"
                  placeholder="金额"
                  value={bankStatementLineForm.amount || ""}
                  onChange={(event) => setBankStatementLineForm({ ...bankStatementLineForm, amount: Number(event.target.value) })}
                />
                <input
                  placeholder="对方户名"
                  value={bankStatementLineForm.counterpartyName}
                  onChange={(event) => setBankStatementLineForm({ ...bankStatementLineForm, counterpartyName: event.target.value })}
                />
                <input
                  placeholder="银行参考号"
                  value={bankStatementLineForm.bankReferenceNo}
                  onChange={(event) => setBankStatementLineForm({ ...bankStatementLineForm, bankReferenceNo: event.target.value })}
                />
                <input
                  placeholder="摘要"
                  value={bankStatementLineForm.summary}
                  onChange={(event) => setBankStatementLineForm({ ...bankStatementLineForm, summary: event.target.value })}
                />
                <button
                  disabled={
                    busyKey === "bank-statement-create" ||
                    !bankStatementLineForm.bankAccountId ||
                    !bankStatementLineForm.transactionDate ||
                    bankStatementLineForm.amount <= 0
                  }
                  onClick={createBankStatementLine}
                >
                  <Plus size={16} />
                  <span>录入流水</span>
                </button>
              </div>
            </div>
          ) : (
            <div className="section-note">
              {canManageSettlements ? "先维护启用的银行账户后，才能录入银行流水。" : "当前账号只能查看银行流水，不能录入流水。"}
            </div>
          )}

          {bankStatementLinesQuery.loading ? (
            <div className="section-note">正在加载银行流水...</div>
          ) : bankStatementLinesQuery.error ? (
            <div className="section-note error">{bankStatementLinesQuery.error}</div>
          ) : bankStatementLines.length > 0 ? (
            <div className="table-shell">
              {bankStatementLines.map((entry) => (
                <div key={entry.id} className="review-card finance-card">
                  <div>
                    <strong>{entry.statementNo}</strong>
                    <p>{entry.bankAccountNo} · {entry.bankAccountName}</p>
                    <small>{formatDateOnly(entry.transactionDate)} · {entry.counterpartyName || "无对方户名"} · {entry.bankReferenceNo || "无参考号"}</small>
                    <small>{entry.summary || "无摘要"} · {reconciliationStatusText(entry.reconciliationStatus)}</small>
                  </div>
                  <div className="inventory-actions">
                    <span className={`inventory-movement-chip ${entry.direction === "Inflow" ? "movement-positive" : "movement-negative"}`}>
                      {bankDirectionText(entry.direction)} {formatAmount(entry.amount)}
                    </span>
                    <small>{entry.currencyCode}</small>
                    {entry.settlementNo ? <small>结算 {entry.settlementNo}</small> : null}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState
              title="暂无银行流水"
              description={canManageSettlements ? "录入第一笔银行流水后，可与付款或收款结算记录对账。" : "需要结算维护权限后才能录入银行流水。"}
            />
          )}
        </SectionBlock>
      </div>

      <SectionBlock title="银行对账" hint="按账户、方向、币种和金额匹配银行流水与付款/收款结算。">
        {bankStatementLinesQuery.loading || settlementsQuery.loading ? (
          <div className="section-note">正在加载对账数据...</div>
        ) : bankStatementLinesQuery.error || settlementsQuery.error ? (
          <div className="section-note error">{bankStatementLinesQuery.error ?? settlementsQuery.error}</div>
        ) : unmatchedBankStatementLines.length > 0 ? (
          <div className="table-shell">
            {unmatchedBankStatementLines.map((line) => {
              const candidates = matchingSettlementsForLine(line);
              return (
                <div key={line.id} className="review-card finance-card">
                  <div>
                    <strong>{line.statementNo}</strong>
                    <p>{line.bankAccountNo} · {line.bankAccountName} · {bankDirectionText(line.direction)}</p>
                    <small>{formatDateOnly(line.transactionDate)} · {line.counterpartyName || "无对方户名"}</small>
                    <div className="inventory-lines">
                      <span>{formatAmount(line.amount)}</span>
                      <span>{line.currencyCode}</span>
                      <span>{reconciliationStatusText(line.reconciliationStatus)}</span>
                    </div>
                  </div>
                  <div className="inventory-actions">
                    {candidates.length > 0 && canManageSettlements ? (
                      candidates.map((settlement) => (
                        <button
                          key={settlement.id}
                          type="button"
                          disabled={busyKey === `bank-reconcile-${line.id}-${settlement.id}`}
                          onClick={() => reconcileBankStatement(line, settlement)}
                        >
                          匹配 {settlement.settlementNo}
                        </button>
                      ))
                    ) : (
                      <small>{canManageSettlements ? "暂无匹配的未对账结算记录。" : "当前账号不能执行银行对账。"}</small>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        ) : (
          <EmptyState
            title="暂无待对账银行流水"
            description="录入银行流水并创建付款或收款结算后，未匹配项会出现在这里。"
          />
        )}
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="会计科目" hint="维护总账凭证所需的科目编码、类型、层级和启用状态。">
          {canManageAccounting ? (
            <div className="finance-source-panel">
              <div className="section-note">{accountForm.id ? "编辑会计科目" : "新增会计科目"}</div>
              <div className="inventory-actions">
                <input
                  placeholder="科目编码"
                  value={accountForm.code}
                  onChange={(event) => setAccountForm({ ...accountForm, code: event.target.value })}
                />
                <input
                  placeholder="科目名称"
                  value={accountForm.name}
                  onChange={(event) => setAccountForm({ ...accountForm, name: event.target.value })}
                />
                <select value={accountForm.type} onChange={(event) => setAccountForm({ ...accountForm, type: event.target.value })}>
                  {accountTypes.map((type) => (
                    <option key={type} value={type}>{accountTypeText(type)}</option>
                  ))}
                </select>
                <select
                  value={accountForm.parentAccountId}
                  onChange={(event) => setAccountForm({ ...accountForm, parentAccountId: event.target.value })}
                >
                  <option value="">无上级科目</option>
                  {accounts
                    .filter((entry) => entry.id !== accountForm.id)
                    .map((entry) => (
                      <option key={entry.id} value={entry.id}>{entry.code} {entry.name}</option>
                    ))}
                </select>
                <label className="inline-check">
                  <input
                    type="checkbox"
                    checked={accountForm.isActive}
                    onChange={(event) => setAccountForm({ ...accountForm, isActive: event.target.checked })}
                  />
                  启用
                </label>
                <button
                  disabled={busyKey === "accounting-account-save" || !accountForm.code.trim() || !accountForm.name.trim()}
                  onClick={submitAccountingAccount}
                >
                  {accountForm.id ? <Save size={16} /> : <Plus size={16} />}
                  <span>{accountForm.id ? "保存科目" : "新增科目"}</span>
                </button>
                {accountForm.id ? (
                  <button type="button" className="secondary" onClick={resetAccountForm}>
                    取消编辑
                  </button>
                ) : null}
              </div>
            </div>
          ) : (
            <div className="section-note">当前账号只能查看会计科目，不能维护科目主数据。</div>
          )}

          {accountsQuery.loading ? (
            <div className="section-note">正在加载会计科目...</div>
          ) : accountsQuery.error ? (
            <div className="section-note error">{accountsQuery.error}</div>
          ) : accounts.length > 0 ? (
            <div className="table-shell">
              {accounts.map((entry) => (
                <div key={entry.id} className="review-card">
                  <div>
                    <strong>{entry.code} · {entry.name}</strong>
                    <p>{accountTypeText(entry.type)} · {entry.isActive ? "启用" : "停用"}</p>
                    <small>{entry.parentAccountCode ? `上级 ${entry.parentAccountCode} ${entry.parentAccountName}` : "一级科目"} · {entry.updatedBy}</small>
                  </div>
                  {canManageAccounting ? (
                    <div className="inventory-actions">
                      <button type="button" className="secondary" onClick={() => editAccount(entry)}>
                        编辑
                      </button>
                      <button
                        type="button"
                        disabled={busyKey === `account-toggle-${entry.id}`}
                        onClick={() => toggleAccount(entry)}
                      >
                        {entry.isActive ? "停用" : "启用"}
                      </button>
                    </div>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <EmptyState
              title="暂无会计科目"
              description={canManageAccounting ? "先创建资产、负债、权益、收入、费用或成本科目。" : "需要会计基础维护权限后才能创建科目。"}
            />
          )}
        </SectionBlock>

        <SectionBlock title="会计期间" hint="建立月度会计期间，并控制关账与重开状态。">
          {canManageAccounting ? (
            <div className="finance-source-panel">
              <div className="section-note">创建会计期间</div>
              <div className="inventory-actions">
                <input
                  type="number"
                  min={2000}
                  max={2100}
                  value={periodForm.year}
                  onChange={(event) => setPeriodForm({ ...periodForm, year: Number(event.target.value) })}
                />
                <input
                  type="number"
                  min={1}
                  max={12}
                  value={periodForm.month}
                  onChange={(event) => setPeriodForm({ ...periodForm, month: Number(event.target.value) })}
                />
                <button
                  disabled={busyKey === "accounting-period-create" || periodForm.year < 2000 || periodForm.year > 2100 || periodForm.month < 1 || periodForm.month > 12}
                  onClick={createAccountingPeriod}
                >
                  <CalendarDays size={16} />
                  <span>创建期间</span>
                </button>
              </div>
            </div>
          ) : (
            <div className="section-note">当前账号只能查看会计期间，不能开账或关账。</div>
          )}

          {periodsQuery.loading ? (
            <div className="section-note">正在加载会计期间...</div>
          ) : periodsQuery.error ? (
            <div className="section-note error">{periodsQuery.error}</div>
          ) : periods.length > 0 ? (
            <div className="table-shell">
              {periods.map((entry) => {
                const pendingCount = pendingVoucherCountsByPeriodId.get(entry.id) ?? 0;
                return (
                  <div key={entry.id} className="review-card">
                    <div>
                      <strong>{entry.name}</strong>
                      <p>{formatDateOnly(entry.startDate)} 至 {formatDateOnly(entry.endDate)}</p>
                      <small>
                        {periodStatusText(entry.status)}
                        {entry.closedBy ? ` · ${entry.closedBy}` : ""}
                        {pendingCount > 0 ? ` · ${pendingCount} 张未决凭证` : ""}
                      </small>
                    </div>
                    {canManageAccounting ? (
                      <div className="inventory-actions">
                        {entry.status === "Open" ? (
                          <button
                            type="button"
                            disabled={busyKey === `period-close-${entry.id}` || vouchersQuery.loading || pendingCount > 0}
                            onClick={() => closeAccountingPeriod(entry)}
                          >
                            <Lock size={16} />
                            <span>关账</span>
                          </button>
                        ) : (
                          <button
                            type="button"
                            disabled={busyKey === `period-reopen-${entry.id}`}
                            onClick={() => reopenAccountingPeriod(entry)}
                          >
                            <Unlock size={16} />
                            <span>重开</span>
                          </button>
                        )}
                      </div>
                    ) : null}
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState
              title="暂无会计期间"
              description={canManageAccounting ? "创建第一个月度期间后，凭证和结账流程才有可归属的期间。" : "需要会计基础维护权限后才能创建会计期间。"}
            />
          )}
        </SectionBlock>
      </div>

      <SectionBlock title="总账凭证" hint="手工录入借贷分录，提交后由具备审核权限的账号完成总账审核。">
        {canManageVouchers ? (
          <div className="finance-source-panel">
            <div className="section-note">新增手工凭证</div>
            <div className="inventory-actions">
              <select
                value={voucherForm.accountingPeriodId}
                onChange={(event) => setVoucherForm({ ...voucherForm, accountingPeriodId: event.target.value })}
              >
                <option value="">选择打开期间</option>
                {openPeriods.map((period) => (
                  <option key={period.id} value={period.id}>{period.name}</option>
                ))}
              </select>
              <input
                type="date"
                value={voucherForm.documentDate}
                onChange={(event) => setVoucherForm({ ...voucherForm, documentDate: event.target.value })}
              />
              <input
                placeholder="凭证摘要"
                value={voucherForm.summary}
                onChange={(event) => setVoucherForm({ ...voucherForm, summary: event.target.value })}
              />
            </div>
            <div className="table-shell">
              {voucherForm.lines.map((line, index) => (
                <div key={index} className="review-card">
                  <div className="inventory-actions">
                    <select
                      value={line.accountingAccountId}
                      onChange={(event) => updateVoucherLine(index, { accountingAccountId: event.target.value })}
                    >
                      <option value="">选择科目</option>
                      {activeAccounts.map((account) => (
                        <option key={account.id} value={account.id}>{account.code} {account.name}</option>
                      ))}
                    </select>
                    <input
                      placeholder="分录摘要"
                      value={line.summary}
                      onChange={(event) => updateVoucherLine(index, { summary: event.target.value })}
                    />
                  </div>
                  <div className="inventory-actions">
                    <input
                      type="number"
                      min={0}
                      step="0.01"
                      placeholder="借方金额"
                      value={line.debitAmount || ""}
                      onChange={(event) => updateVoucherLine(index, { debitAmount: Number(event.target.value), creditAmount: Number(event.target.value) > 0 ? 0 : line.creditAmount })}
                    />
                    <input
                      type="number"
                      min={0}
                      step="0.01"
                      placeholder="贷方金额"
                      value={line.creditAmount || ""}
                      onChange={(event) => updateVoucherLine(index, { creditAmount: Number(event.target.value), debitAmount: Number(event.target.value) > 0 ? 0 : line.debitAmount })}
                    />
                    {voucherForm.lines.length > 2 ? (
                      <button type="button" className="secondary" onClick={() => removeVoucherLine(index)}>
                        删除分录
                      </button>
                    ) : null}
                  </div>
                </div>
              ))}
            </div>
            <div className="inventory-lines">
              <span>借方合计 {formatAmount(voucherDebitTotal)}</span>
              <span>贷方合计 {formatAmount(voucherCreditTotal)}</span>
              <span>{voucherDebitTotal === voucherCreditTotal && voucherDebitTotal > 0 ? "借贷平衡" : "借贷未平衡"}</span>
            </div>
            <div className="inventory-actions">
              <button type="button" className="secondary" onClick={addVoucherLine}>
                <Plus size={16} />
                <span>增加分录</span>
              </button>
              <button disabled={busyKey === "voucher-create" || !voucherCanSubmit} onClick={createManualVoucher}>
                <Save size={16} />
                <span>创建凭证</span>
              </button>
            </div>
          </div>
        ) : (
          <div className="section-note">当前账号只能查看总账凭证，不能创建或提交凭证。</div>
        )}

        {vouchersQuery.loading ? (
          <div className="section-note">正在加载总账凭证...</div>
        ) : vouchersQuery.error ? (
          <div className="section-note error">{vouchersQuery.error}</div>
        ) : vouchers.length > 0 ? (
          <div className="table-shell">
            {vouchers.map((entry) => (
              <div key={entry.id} className="review-card finance-card">
                <div>
                  <strong>{entry.voucherNo}</strong>
                  <p>{entry.accountingPeriodName} · {formatDateOnly(entry.documentDate)} · {voucherStatusText(entry.status)}</p>
                  <small>
                    {sourceTypeText(entry.sourceType)}
                    {entry.sourceNo ? ` · ${entry.sourceNo}` : ""}
                    {" · "}
                    {entry.summary} · 制单 {entry.createdBy}
                  </small>
                  <div className="inventory-lines">
                    <span>借 {formatAmount(entry.totalDebit)}</span>
                    <span>贷 {formatAmount(entry.totalCredit)}</span>
                    {entry.reviewedBy ? <span>审核 {entry.reviewedBy}</span> : null}
                  </div>
                  <div className="inventory-lines">
                    {entry.lines.map((line) => (
                      <span key={line.id}>{line.accountCode} {line.accountName} · 借 {formatAmount(line.debitAmount)} · 贷 {formatAmount(line.creditAmount)}</span>
                    ))}
                  </div>
                </div>
                <div className="inventory-actions">
                  {entry.status === "Draft" && canManageVouchers ? (
                    <button
                      type="button"
                      disabled={busyKey === `voucher-submit-${entry.id}`}
                      onClick={() => submitVoucher(entry)}
                    >
                      提交审核
                    </button>
                  ) : null}
                  {entry.status === "Submitted" && canReviewVouchers ? (
                    <>
                      <input
                        placeholder="审核意见"
                        value={reviewNotes[entry.id] ?? ""}
                        onChange={(event) => setReviewNotes({ ...reviewNotes, [entry.id]: event.target.value })}
                      />
                      <button
                        type="button"
                        disabled={busyKey === `voucher-approve-${entry.id}`}
                        onClick={() => approveVoucher(entry)}
                      >
                        审核通过
                      </button>
                      <button
                        type="button"
                        className="secondary"
                        disabled={busyKey === `voucher-reject-${entry.id}`}
                        onClick={() => rejectVoucher(entry)}
                      >
                        驳回
                      </button>
                    </>
                  ) : null}
                  {entry.status === "Draft" && !canManageVouchers ? (
                    <small>当前账号不能提交草稿凭证。</small>
                  ) : null}
                  {entry.status === "Submitted" && !canReviewVouchers ? (
                    <small>当前账号不能审核凭证。</small>
                  ) : null}
                  {entry.status === "Approved" || entry.status === "Rejected" ? (
                    <small>{entry.reviewNote || "无可执行动作。"}</small>
                  ) : null}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            title="暂无总账凭证"
            description={canManageVouchers ? "创建第一张借贷平衡的手工凭证，作为总账主干起点。" : "需要总账凭证维护权限后才能创建凭证。"}
          />
        )}
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="应付记录" hint="采购入库完成后，在这里形成真实应付并推进付款结算。">
          {payablesQuery.loading ? (
            <div className="section-note">正在加载应付记录...</div>
          ) : payablesQuery.error ? (
            <div className="section-note error">{payablesQuery.error}</div>
          ) : payables.length > 0 ? (
            <div className="table-shell">
              {payables.map((entry) => {
                const key = `Payable-${entry.id}`;
                const form = getSettlementForm(key, entry);
                const bankAccountOptions = activeBankAccounts.filter((account) => account.currencyCode === entry.currencyCode);
                return (
                  <div key={entry.id} className="review-card finance-card">
                    <div>
                      <strong>{entry.payableNo}</strong>
                      <p>{entry.procurementOrderNo} · {entry.supplierName}</p>
                      <small>{sourceTypeText(entry.sourceType)} · {financeStatusText(entry.status)} · {formatDate(entry.createdAtUtc)}</small>
                      <small>到期 {formatDateOnly(entry.dueDate)} · {agingRiskText(entry.overdueDays)}</small>
                      <div className="inventory-lines">
                        <span>价税合计 {formatAmount(entry.amount)}</span>
                        <span>未税 {formatAmount(entry.netAmount)}</span>
                        <span>税额 {formatAmount(entry.taxAmount)}</span>
                      </div>
                      <div className="inventory-lines">
                        <span>税率 {formatTaxRate(entry.taxRate)}</span>
                        <span>{entry.taxInvoiceType}</span>
                        <span>已结 {formatAmount(entry.settledAmount)}</span>
                        <span>未结 {formatAmount(entry.remainingAmount)}</span>
                      </div>
                    </div>
                    <div className="inventory-actions">
                      {entry.status !== "Settled" && canManageSettlements ? (
                        <>
                          <select
                            value={form.bankAccountId}
                            onChange={(event) => setSettlementForm(key, entry, { bankAccountId: event.target.value })}
                          >
                            <option value="">选择银行账户</option>
                            {bankAccountOptions.map((account) => (
                              <option key={account.id} value={account.id}>{account.accountNo} · {account.accountName}</option>
                            ))}
                          </select>
                          <input
                            type="number"
                            min={0.01}
                            max={entry.remainingAmount}
                            step="0.01"
                            value={form.amount}
                            onChange={(event) => setSettlementForm(key, entry, { amount: Number(event.target.value) })}
                          />
                          <input value={form.method} onChange={(event) => setSettlementForm(key, entry, { method: event.target.value })} />
                          <input placeholder="结算备注" value={form.note} onChange={(event) => setSettlementForm(key, entry, { note: event.target.value })} />
                          <button
                            disabled={busyKey === `settlement-${key}` || form.amount <= 0 || form.amount > entry.remainingAmount || !form.bankAccountId || !form.method.trim()}
                            onClick={async () => createSettlement("Payable", entry)}
                          >
                            付款结算
                          </button>
                        </>
                      ) : (
                        <small>{entry.status === "Settled" ? "该应付已结清。" : "当前账号不能执行付款结算。"}</small>
                      )}
                      {renderInvoiceControls("Payable", entry.id, entry.payableNo)}
                      {renderBusinessVoucherControls("Payable", entry.id, entry.payableNo, `应付入账 ${entry.payableNo}`)}
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无应付记录" description="完成采购入库后，可从来源单据生成第一笔应付。" />
          )}

          {canManagePayables ? (
            <div className="finance-source-panel">
              <div className="section-note">可生成应付来源</div>
              {payableReceiptCandidates.length > 0 || payableOrderCandidates.length > 0 ? (
                <div className="table-shell">
                  {payableReceiptCandidates.map((entry) => (
                    <div key={entry.id} className="review-card">
                      <div>
                        <strong>{entry.receiptNo}</strong>
                        <p>{entry.procurementOrderNo} · {entry.supplierName}</p>
                        <small>采购入库 · {formatDate(entry.receivedAtUtc)}</small>
                      </div>
                      <div className="inventory-actions">
                        <input
                          type="number"
                          min={0.01}
                          step="0.01"
                          placeholder="应付金额"
                          value={payableAmounts[entry.id] ?? ""}
                          onChange={(event) => setPayableAmounts({ ...payableAmounts, [entry.id]: Number(event.target.value) })}
                        />
                        <button
                          disabled={busyKey === `payable-receipt-${entry.id}` || (payableAmounts[entry.id] ?? 0) <= 0}
                          onClick={async () => createPayableFromReceipt(entry)}
                        >
                          生成应付
                        </button>
                      </div>
                    </div>
                  ))}
                  {payableOrderCandidates.map((entry) => (
                    <div key={entry.id} className="review-card">
                      <div>
                        <strong>{entry.orderNo}</strong>
                        <p>{entry.requestNo} · {entry.supplierName}</p>
                        <small>采购订单 · {formatDate(entry.createdAtUtc)}</small>
                      </div>
                      <div className="inventory-actions">
                        <input
                          type="number"
                          min={0.01}
                          step="0.01"
                          placeholder="应付金额"
                          value={payableAmounts[entry.id] ?? ""}
                          onChange={(event) => setPayableAmounts({ ...payableAmounts, [entry.id]: Number(event.target.value) })}
                        />
                        <button
                          disabled={busyKey === `payable-order-${entry.id}` || (payableAmounts[entry.id] ?? 0) <= 0}
                          onClick={async () => createPayableFromOrder(entry)}
                        >
                          生成应付
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  title="暂无可生成应付的来源"
                  description="采购订单完成入库后，会在这里出现。"
                  action={<Link to="/procurement"><button type="button">去采购管理</button></Link>}
                />
              )}
            </div>
          ) : (
            <div className="section-note">当前账号只能查看应付记录，不能生成应付。</div>
          )}
        </SectionBlock>

        <SectionBlock title="应收记录" hint="销售出库完成后，在这里形成真实应收并推进收款结算。">
          {receivablesQuery.loading ? (
            <div className="section-note">正在加载应收记录...</div>
          ) : receivablesQuery.error ? (
            <div className="section-note error">{receivablesQuery.error}</div>
          ) : receivables.length > 0 ? (
            <div className="table-shell">
              {receivables.map((entry) => {
                const key = `Receivable-${entry.id}`;
                const form = getSettlementForm(key, entry);
                const bankAccountOptions = activeBankAccounts.filter((account) => account.currencyCode === entry.currencyCode);
                return (
                  <div key={entry.id} className="review-card finance-card">
                    <div>
                      <strong>{entry.receivableNo}</strong>
                      <p>{entry.salesOrderNo} · {entry.customerName}</p>
                      <small>{sourceTypeText(entry.sourceType)} · {financeStatusText(entry.status)} · {formatDate(entry.createdAtUtc)}</small>
                      <small>到期 {formatDateOnly(entry.dueDate)} · {agingRiskText(entry.overdueDays)}</small>
                      <div className="inventory-lines">
                        <span>价税合计 {formatAmount(entry.amount)}</span>
                        <span>未税 {formatAmount(entry.netAmount)}</span>
                        <span>税额 {formatAmount(entry.taxAmount)}</span>
                      </div>
                      <div className="inventory-lines">
                        <span>税率 {formatTaxRate(entry.taxRate)}</span>
                        <span>{entry.taxInvoiceType}</span>
                        <span>已结 {formatAmount(entry.settledAmount)}</span>
                        <span>未结 {formatAmount(entry.remainingAmount)}</span>
                      </div>
                    </div>
                    <div className="inventory-actions">
                      {entry.status !== "Settled" && canManageSettlements ? (
                        <>
                          <select
                            value={form.bankAccountId}
                            onChange={(event) => setSettlementForm(key, entry, { bankAccountId: event.target.value })}
                          >
                            <option value="">选择银行账户</option>
                            {bankAccountOptions.map((account) => (
                              <option key={account.id} value={account.id}>{account.accountNo} · {account.accountName}</option>
                            ))}
                          </select>
                          <input
                            type="number"
                            min={0.01}
                            max={entry.remainingAmount}
                            step="0.01"
                            value={form.amount}
                            onChange={(event) => setSettlementForm(key, entry, { amount: Number(event.target.value) })}
                          />
                          <input value={form.method} onChange={(event) => setSettlementForm(key, entry, { method: event.target.value })} />
                          <input placeholder="结算备注" value={form.note} onChange={(event) => setSettlementForm(key, entry, { note: event.target.value })} />
                          <button
                            disabled={busyKey === `settlement-${key}` || form.amount <= 0 || form.amount > entry.remainingAmount || !form.bankAccountId || !form.method.trim()}
                            onClick={async () => createSettlement("Receivable", entry)}
                          >
                            收款结算
                          </button>
                        </>
                      ) : (
                        <small>{entry.status === "Settled" ? "该应收已结清。" : "当前账号不能执行收款结算。"}</small>
                      )}
                      {renderInvoiceControls("Receivable", entry.id, entry.receivableNo)}
                      {renderBusinessVoucherControls("Receivable", entry.id, entry.receivableNo, `应收入账 ${entry.receivableNo}`)}
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无应收记录" description="完成销售出库后，可从来源单据生成第一笔应收。" />
          )}

          {canManageReceivables ? (
            <div className="finance-source-panel">
              <div className="section-note">可生成应收来源</div>
              {receivableIssueCandidates.length > 0 || receivableOrderCandidates.length > 0 ? (
                <div className="table-shell">
                  {receivableIssueCandidates.map((entry) => (
                    <div key={entry.id} className="review-card">
                      <div>
                        <strong>{entry.issueNo}</strong>
                        <p>{entry.salesOrderNo} · {entry.customerName}</p>
                        <small>销售出库 · {formatDate(entry.issuedAtUtc)}</small>
                      </div>
                      <div className="inventory-actions">
                        <input
                          type="number"
                          min={0.01}
                          step="0.01"
                          placeholder="应收金额"
                          value={receivableAmounts[entry.id] ?? ""}
                          onChange={(event) => setReceivableAmounts({ ...receivableAmounts, [entry.id]: Number(event.target.value) })}
                        />
                        <button
                          disabled={busyKey === `receivable-issue-${entry.id}` || (receivableAmounts[entry.id] ?? 0) <= 0}
                          onClick={async () => createReceivableFromIssue(entry)}
                        >
                          生成应收
                        </button>
                      </div>
                    </div>
                  ))}
                  {receivableOrderCandidates.map((entry) => (
                    <div key={entry.id} className="review-card">
                      <div>
                        <strong>{entry.orderNo}</strong>
                        <p>{entry.quotationNo} · {entry.customerName}</p>
                        <small>销售订单 · {formatDate(entry.createdAtUtc)}</small>
                      </div>
                      <div className="inventory-actions">
                        <input
                          type="number"
                          min={0.01}
                          step="0.01"
                          placeholder="应收金额"
                          value={receivableAmounts[entry.id] ?? ""}
                          onChange={(event) => setReceivableAmounts({ ...receivableAmounts, [entry.id]: Number(event.target.value) })}
                        />
                        <button
                          disabled={busyKey === `receivable-order-${entry.id}` || (receivableAmounts[entry.id] ?? 0) <= 0}
                          onClick={async () => createReceivableFromOrder(entry)}
                        >
                          生成应收
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  title="暂无可生成应收的来源"
                  description="销售订单完成出库后，会在这里出现。"
                  action={<Link to="/sales"><button type="button">去销售管理</button></Link>}
                />
              )}
            </div>
          ) : (
            <div className="section-note">当前账号只能查看应收记录，不能生成应收。</div>
          )}
        </SectionBlock>
      </div>

      <SectionBlock title="税票记录" hint="应付和应收登记税票后，会按来源单据保留价税拆分、票据日期和经办人。">
        {invoicesQuery.loading ? (
          <div className="section-note">正在加载税票记录...</div>
        ) : invoicesQuery.error ? (
          <div className="section-note error">{invoicesQuery.error}</div>
        ) : invoices.length > 0 ? (
          <div className="table-shell">
            {invoices.map((entry) => (
              <div key={entry.id} className="review-card finance-card">
                <div>
                  <strong>{entry.invoiceNo}</strong>
                  <p>{targetTypeText(entry.direction)} · {entry.sourceNo} · {entry.counterpartyName}</p>
                  <small>{entry.taxInvoiceType} · 税率 {formatTaxRate(entry.taxRate)} · 票据日 {formatDateOnly(entry.invoiceDate)}</small>
                  <div className="inventory-lines">
                    <span>价税合计 {formatAmount(entry.grossAmount)}</span>
                    <span>未税 {formatAmount(entry.netAmount)}</span>
                    <span>税额 {formatAmount(entry.taxAmount)}</span>
                    <span>{entry.currencyCode}</span>
                  </div>
                </div>
                <div className="inventory-actions">
                  {entry.note ? <small>{entry.note}</small> : null}
                  <small>{entry.createdBy}</small>
                  <small>{formatDate(entry.createdAtUtc)}</small>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            title="暂无税票记录"
            description="在应付或应收卡片登记税票后，这里会形成可追踪的票据历史。"
            action={<FilePlus2 size={28} />}
          />
        )}
      </SectionBlock>

      <SectionBlock title="结算记录" hint="每次付款或收款都会形成可追踪的结算历史。">
        {settlementsQuery.loading ? (
          <div className="section-note">正在加载结算记录...</div>
        ) : settlementsQuery.error ? (
          <div className="section-note error">{settlementsQuery.error}</div>
        ) : settlements.length > 0 ? (
          <div className="table-shell">
            {settlements.map((entry) => (
              <div key={entry.id} className="review-card finance-card">
                <div>
                  <strong>{entry.settlementNo}</strong>
                  <p>{targetTypeText(entry.targetType)} · {entry.targetNo} · {entry.counterpartyName}</p>
                  <small>{entry.bankAccountNo} · {entry.bankAccountName}</small>
                  <small>{entry.method}{entry.note ? ` · ${entry.note}` : ""} · {reconciliationStatusText(entry.reconciliationStatus)}</small>
                </div>
                <div className="inventory-actions">
                  <span className={`inventory-movement-chip ${settlementDirection(entry) === "Inflow" ? "movement-positive" : "movement-negative"}`}>
                    {bankDirectionText(settlementDirection(entry))} {formatAmount(entry.amount)}
                  </span>
                  <small>{entry.settledBy}</small>
                  <small>{formatDate(entry.settledAtUtc)}</small>
                  {entry.bankStatementNo ? <small>流水 {entry.bankStatementNo}</small> : null}
                  {renderBusinessVoucherControls("Settlement", entry.id, entry.settlementNo, `${targetTypeText(entry.targetType)}结算 ${entry.settlementNo}`)}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            title="暂无结算记录"
            description="完成一笔应收或应付结算后，这里会显示真实历史。"
            action={<HandCoins size={28} />}
          />
        )}
      </SectionBlock>
    </PageShell>
  );
}
