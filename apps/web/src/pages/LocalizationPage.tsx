import { RefreshCcw } from "lucide-react";
import { useEffect, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import { useLanguage } from "../i18n/LanguageContext";
import type { Currency, LocalizationContent, LocalizationSettings, OrganizationSummary } from "../types/api";

const loadEmptyCurrencies = () => Promise.resolve<Currency[]>([]);
const loadEmptyOrganizations = () => Promise.resolve<OrganizationSummary[]>([]);
const loadEmptyContent = () => Promise.resolve<LocalizationContent[]>([]);
const defaultSettings: LocalizationSettings = {
  id: "",
  defaultCurrencyCode: "CNY",
  taxInvoiceType: "增值税普通发票",
  taxpayerId: "",
  invoiceTitle: "",
  defaultTaxRate: 0.13,
};
const loadDefaultSettings = () => Promise.resolve(defaultSettings);

/** 本地化页面，维护币种、税务默认设置、组织区域和界面多语言词条。 */
export function LocalizationPage() {
  const { hasPermission } = useAuth();
  const { reloadContent, t } = useLanguage();
  const canReadLocalization = hasPermission(platformPermissions.localizationRead);
  const canManageLocalization = hasPermission(platformPermissions.localizationManage);
  const canManageOrganizations = hasPermission(platformPermissions.organizationManage);

  const currenciesQuery = useAsyncData(canReadLocalization ? api.listCurrencies : loadEmptyCurrencies);
  const settingsQuery = useAsyncData(canReadLocalization ? api.getLocalizationSettings : loadDefaultSettings);
  const contentQuery = useAsyncData(canReadLocalization ? api.listLocalizationContent : loadEmptyContent);
  const organizationsQuery = useAsyncData(canManageOrganizations ? api.listOrganizations : loadEmptyOrganizations);

  const [currencyForm, setCurrencyForm] = useState({
    code: "CNY",
    name: "人民币",
    symbol: "¥",
    exchangeRateToBase: 1,
    isBase: true,
    isEnabled: true,
  });
  const [settingsForm, setSettingsForm] = useState(defaultSettings);
  const [contentForm, setContentForm] = useState({
    key: "custom.",
    category: "自定义内容",
    chineseText: "",
    englishText: "",
    isEnabled: true,
  });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const currencies = currenciesQuery.data ?? [];
  const localizationContent = contentQuery.data ?? [];
  const organizations = organizationsQuery.data ?? [];
  const baseCurrency = currencies.find((entry) => entry.isBase);

  useEffect(() => {
    if (settingsQuery.data) {
      setSettingsForm(settingsQuery.data);
    }
  }, [settingsQuery.data]);

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
    if (canReadLocalization) {
      tasks.push(currenciesQuery.reload(), settingsQuery.reload(), contentQuery.reload(), reloadContent());
    }
    if (canManageOrganizations) {
      tasks.push(organizationsQuery.reload());
    }
    await Promise.all(tasks);
  }

  if (!canReadLocalization && !canManageOrganizations) {
    return (
      <PageShell title={t("localization.title", "语言与本地化")}>
        <EmptyState title={t("localization.no-access.title", "无语言与本地化权限")} description={t("localization.no-access.description", "当前账号不能查看组织、币种、本地化设置或界面文本。")} />
      </PageShell>
    );
  }

  return (
    <PageShell
      title={t("localization.title", "语言与本地化")}
      actions={
        <button className="secondary icon-button" disabled={busyKey === "localization-refresh"} onClick={async () => runAction("localization-refresh", reloadAll, "组织本地化数据已刷新。")}>
          <RefreshCcw size={16} />
          <span>{t("action.refresh", "刷新数据")}</span>
        </button>
      }
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <section className="stats-grid">
        <StatTile label="组织数量" value={organizations.length} tone={organizations.length > 0 ? "success" : "default"} />
        <StatTile label="启用币种" value={currencies.filter((entry) => entry.isEnabled).length} tone="success" />
        <StatTile label="本位币" value={baseCurrency?.code ?? "未设置"} tone={baseCurrency ? "success" : "warning"} />
        <StatTile label="默认税率" value={`${Math.round((settingsQuery.data?.defaultTaxRate ?? 0) * 100)}%`} tone="default" />
      </section>

      <SectionBlock title={t("localization.content.title", "界面与内容翻译")} hint={t("localization.content.hint", "维护中文内容对应的英文文本；切换到英文时会优先使用这里保存的英文内容。")}>
        {canReadLocalization ? (
          localizationContent.length > 0 ? (
            <div className="translation-list">
              {localizationContent.map((item) => (
                <div key={item.id} className="translation-row">
                  <div>
                    <strong>{item.chineseText}</strong>
                    <p>{item.englishText || "未设置英文内容"}</p>
                    <small>{item.category} · {item.key} · {item.isEnabled ? "启用" : "停用"}</small>
                  </div>
                  {canManageLocalization ? (
                    <button
                      type="button"
                      className="secondary"
                      onClick={() => setContentForm({
                        key: item.key,
                        category: item.category,
                        chineseText: item.chineseText,
                        englishText: item.englishText,
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
            <EmptyState title={t("localization.content.empty.title", "暂无翻译内容")} description={t("localization.content.empty.description", "系统会自动准备基础界面文本，也可以手工新增业务内容翻译。")} />
          )
        ) : (
          <EmptyState title="无界面文本读取权限" description="当前账号不能查看翻译内容。" />
        )}

        {canManageLocalization ? (
          <form
            className="translation-form"
            onSubmit={async (event) => {
              event.preventDefault();
              await runAction("content-upsert", async () => {
                await api.upsertLocalizationContent(contentForm);
                setContentForm({ key: "custom.", category: "自定义内容", chineseText: "", englishText: "", isEnabled: true });
                await contentQuery.reload();
                await reloadContent();
              }, t("localization.content.saved", "翻译内容已保存。"));
            }}
          >
            <div className="translation-form-grid">
              <input placeholder={t("localization.content.key", "内容键")} value={contentForm.key} onChange={(event) => setContentForm({ ...contentForm, key: event.target.value })} />
              <input placeholder={t("localization.content.category", "分类")} value={contentForm.category} onChange={(event) => setContentForm({ ...contentForm, category: event.target.value })} />
              <textarea placeholder={t("localization.content.zh", "中文内容")} value={contentForm.chineseText} rows={3} onChange={(event) => setContentForm({ ...contentForm, chineseText: event.target.value })} />
              <textarea placeholder={t("localization.content.en", "英文内容")} value={contentForm.englishText} rows={3} onChange={(event) => setContentForm({ ...contentForm, englishText: event.target.value })} />
            </div>
            <div className="button-row wrap">
              <label className="checkbox-row">
                <input type="checkbox" checked={contentForm.isEnabled} onChange={(event) => setContentForm({ ...contentForm, isEnabled: event.target.checked })} />
                <span>{t("localization.content.enabled", "启用翻译")}</span>
              </label>
              <button type="submit" disabled={busyKey === "content-upsert" || !contentForm.key.trim() || !contentForm.chineseText.trim()}>
                {t("localization.content.save", "保存翻译")}
              </button>
            </div>
          </form>
        ) : null}
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="币种设置" hint="币种会被客户、供应商、销售报价、采购申请和财务记录引用。">
          {canReadLocalization ? (
            currencies.length > 0 ? (
              <div className="table-shell">
                {currencies.map((currency) => (
                  <div key={currency.id} className="review-card">
                    <div>
                      <strong>{currency.code} · {currency.name}</strong>
                      <p>{currency.symbol} · 汇率 {currency.exchangeRateToBase}</p>
                      <small>{currency.isBase ? "本位币" : "外币"} · {currency.isEnabled ? "启用" : "停用"}</small>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无币种" description="系统初始化会准备基础币种；如为空请检查后端启动日志。" />
            )
          ) : (
            <EmptyState title="无币种读取权限" description="当前账号不能查看币种。" />
          )}

          {canManageLocalization ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await runAction("currency-upsert", async () => {
                  await api.upsertCurrency(currencyForm);
                  await currenciesQuery.reload();
                }, "币种已保存。");
              }}
            >
              <input placeholder="币种代码" value={currencyForm.code} onChange={(event) => setCurrencyForm({ ...currencyForm, code: event.target.value.toUpperCase() })} />
              <input placeholder="币种名称" value={currencyForm.name} onChange={(event) => setCurrencyForm({ ...currencyForm, name: event.target.value })} />
              <input placeholder="符号" value={currencyForm.symbol} onChange={(event) => setCurrencyForm({ ...currencyForm, symbol: event.target.value })} />
              <input type="number" min={0.0001} step="0.0001" value={currencyForm.exchangeRateToBase} onChange={(event) => setCurrencyForm({ ...currencyForm, exchangeRateToBase: Number(event.target.value) })} />
              <label className="checkbox-row">
                <input type="checkbox" checked={currencyForm.isBase} onChange={(event) => setCurrencyForm({ ...currencyForm, isBase: event.target.checked })} />
                <span>设为本位币</span>
              </label>
              <label className="checkbox-row">
                <input type="checkbox" checked={currencyForm.isEnabled} onChange={(event) => setCurrencyForm({ ...currencyForm, isEnabled: event.target.checked })} />
                <span>启用币种</span>
              </label>
              <button type="submit" disabled={busyKey === "currency-upsert" || !currencyForm.code.trim() || !currencyForm.name.trim()}>保存币种</button>
            </form>
          ) : null}
        </SectionBlock>

        <SectionBlock title="本地化设置" hint="默认币种、税票类型、税号和默认税率会作为新单据的基础值。">
          {canReadLocalization ? (
            <div className="table-shell">
              <div className="review-card">
                <div>
                  <strong>{settingsQuery.data?.defaultCurrencyCode ?? "CNY"} · {settingsQuery.data?.taxInvoiceType ?? "增值税普通发票"}</strong>
                  <p>{settingsQuery.data?.invoiceTitle || "未设置发票抬头"} · {settingsQuery.data?.taxpayerId || "未设置纳税识别号"}</p>
                  <small>默认税率：{Math.round((settingsQuery.data?.defaultTaxRate ?? 0) * 100)}%</small>
                </div>
              </div>
            </div>
          ) : (
            <EmptyState title="无本地化读取权限" description="当前账号不能查看本地化设置。" />
          )}

          {canManageLocalization ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                await runAction("settings-update", async () => {
                  await api.updateLocalizationSettings(settingsForm);
                  await settingsQuery.reload();
                }, "本地化设置已更新。");
              }}
            >
              <select value={settingsForm.defaultCurrencyCode} onChange={(event) => setSettingsForm({ ...settingsForm, defaultCurrencyCode: event.target.value })}>
                {currencies.filter((entry) => entry.isEnabled).map((currency) => (
                  <option key={currency.id} value={currency.code}>{currency.code} · {currency.name}</option>
                ))}
              </select>
              <input placeholder="税票类型" value={settingsForm.taxInvoiceType} onChange={(event) => setSettingsForm({ ...settingsForm, taxInvoiceType: event.target.value })} />
              <input placeholder="纳税识别号" value={settingsForm.taxpayerId} onChange={(event) => setSettingsForm({ ...settingsForm, taxpayerId: event.target.value })} />
              <input placeholder="发票抬头" value={settingsForm.invoiceTitle} onChange={(event) => setSettingsForm({ ...settingsForm, invoiceTitle: event.target.value })} />
              <input type="number" min={0} max={1} step="0.01" value={settingsForm.defaultTaxRate} onChange={(event) => setSettingsForm({ ...settingsForm, defaultTaxRate: Number(event.target.value) })} />
              <button type="submit" disabled={busyKey === "settings-update"}>保存本地化设置</button>
            </form>
          ) : null}
        </SectionBlock>
      </div>

      <SectionBlock title="组织列表" hint="组织由平台治理维护，当前页用于确认多组织基础边界。">
        {canManageOrganizations ? (
          organizations.length > 0 ? (
            <div className="inventory-record-list">
              {organizations.map((org) => (
                <div key={org.id} className="inventory-record-row">
                  <div>
                    <strong>{org.name}</strong>
                    <p>{org.defaultRole} · {org.regionCode}</p>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无组织" description="请先在平台治理中创建组织。" />
          )
        ) : (
          <EmptyState title="无组织读取权限" description="当前账号不能查看组织列表。" />
        )}
      </SectionBlock>
    </PageShell>
  );
}
