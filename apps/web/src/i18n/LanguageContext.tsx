import { createContext, useCallback, useContext, useEffect, useMemo, useState, type PropsWithChildren } from "react";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import type { LocalizationContent } from "../types/api";

/** 当前 Web Shell 支持的界面语言。 */
export type LanguageCode = "zh-CN" | "en-US";

/** 多语言上下文对 Shell 和页面暴露的语言状态、词条刷新和翻译函数。 */
type LanguageContextValue = {
  language: LanguageCode;
  setLanguage: (language: LanguageCode) => void;
  entries: LocalizationContent[];
  reloadContent: () => Promise<void>;
  t: (key: string, fallback?: string) => string;
};

const storageKey = "aeroerp.language";
const LanguageContext = createContext<LanguageContextValue | null>(null);

// 后端本地化内容不可用时的基础词条，保证登录后导航和通用动作仍可读。
const fallbackEntries: LocalizationContent[] = [
  entry("app.workspace", "应用框架", "AeroERP 工作台", "AeroERP Workspace"),
  entry("app.tagline", "应用框架", "模块化企业运营平台", "Modular enterprise operations platform"),
  entry("action.logout", "通用动作", "退出登录", "Sign out"),
  entry("action.refresh", "通用动作", "刷新数据", "Refresh"),
  entry("language.zh", "语言切换", "中文", "Chinese"),
  entry("language.en", "语言切换", "英文", "English"),
  entry("language.current", "语言切换", "界面语言", "Language"),
  entry("module.platform", "模块导航", "平台治理", "Platform Governance"),
  entry("module.organization-collaboration", "模块导航", "组织协同", "Organization Collaboration"),
  entry("module.people-management", "模块导航", "人员管理", "People Management"),
  entry("module.plugin-center", "模块导航", "插件中心", "Plugin Center"),
  entry("module.master-data", "模块导航", "主数据", "Master Data"),
  entry("module.crm", "模块导航", "客户CRM", "Customer CRM"),
  entry("module.procurement", "模块导航", "采购管理", "Procurement"),
  entry("module.sales", "模块导航", "销售管理", "Sales"),
  entry("module.inventory", "模块导航", "库存管理", "Inventory"),
  entry("module.wms", "模块导航", "WMS 执行", "WMS Execution"),
  entry("module.mobile-work", "模块导航", "移动作业", "Mobile Work"),
  entry("module.integration", "模块导航", "通知与集成", "Integration"),
  entry("module.channel-integration", "模块导航", "渠道集成", "Channel Integration"),
  entry("module.document-exchange", "模块导航", "文档交换", "Document Exchange"),
  entry("module.finance", "模块导航", "财务结算", "Finance"),
  entry("module.workflow", "模块导航", "审批中心", "Workflow"),
  entry("module.control", "模块导航", "经营管控", "Business Control"),
  entry("module.localization", "模块导航", "语言与本地化", "Language and Localization"),
  entry("module.position-permissions", "模块导航", "岗位权限", "Position Permissions"),
  entry("module.manufacturing", "模块导航", "制造管理", "Manufacturing"),
  entry("module.advanced-manufacturing", "模块导航", "高级制造", "Advanced Manufacturing"),
  entry("module.reporting", "模块导航", "报表中心", "Reporting Center"),
  entry("module.quality", "模块导航", "质量追溯", "Quality Traceability"),
  entry("module.planning", "模块导航", "计划执行", "Planning Execution"),
  entry("role.platform-admin", "职位角色", "平台管理员", "Platform Administrator"),
  entry("role.operations-manager", "职位角色", "运营经理", "Operations Manager"),
  entry("role.purchaser", "职位角色", "采购专员", "Purchaser"),
];

/** 构造内置本地化词条，保持 fallback 数据与后端 DTO 形状一致。 */
function entry(key: string, category: string, chineseText: string, englishText: string): LocalizationContent {
  return {
    id: key,
    key,
    category,
    chineseText,
    englishText,
    isEnabled: true,
    updatedAtUtc: "",
  };
}

/** 从浏览器存储读取初始语言，非法值统一回到中文。 */
function readInitialLanguage(): LanguageCode {
  return window.localStorage.getItem(storageKey) === "en-US" ? "en-US" : "zh-CN";
}

/**
 * 提供运行时语言切换和词条加载。
 * 已登录时会合并后端本地化内容，未登录或接口失败时使用内置 fallback。
 */
export function LanguageProvider({ children }: PropsWithChildren) {
  const { user } = useAuth();
  const [language, setLanguageState] = useState<LanguageCode>(readInitialLanguage);
  const [entries, setEntries] = useState<LocalizationContent[]>(fallbackEntries);

  const setLanguage = useCallback((next: LanguageCode) => {
    window.localStorage.setItem(storageKey, next);
    setLanguageState(next);
  }, []);

  const reloadContent = useCallback(async () => {
    if (!user) {
      setEntries(fallbackEntries);
      return;
    }

    try {
      const remoteEntries = await api.listLocalizationContent();
      setEntries(mergeEntries(fallbackEntries, remoteEntries));
    } catch {
      setEntries(fallbackEntries);
    }
  }, [user]);

  useEffect(() => {
    void reloadContent();
  }, [reloadContent]);

  const entryMap = useMemo(() => {
    return new Map(entries.map((item) => [item.key, item]));
  }, [entries]);

  const t = useCallback((key: string, fallback?: string) => {
    const item = entryMap.get(key);
    if (!item || !item.isEnabled) {
      return fallback ?? key;
    }

    if (language === "en-US") {
      return (item.englishText || item.chineseText || fallback) ?? key;
    }

    return (item.chineseText || fallback) ?? key;
  }, [entryMap, language]);

  const value = useMemo<LanguageContextValue>(() => ({
    language,
    setLanguage,
    entries,
    reloadContent,
    t,
  }), [entries, language, reloadContent, setLanguage, t]);

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>;
}

/** 读取多语言上下文，页面通过 t 函数获取当前语言文案。 */
export function useLanguage() {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error("useLanguage 必须在 LanguageProvider 内使用");
  }

  return context;
}

/** 后端词条覆盖同 key 的内置词条，未配置项继续保留 fallback。 */
function mergeEntries(baseEntries: LocalizationContent[], remoteEntries: LocalizationContent[]) {
  const merged = new Map(baseEntries.map((item) => [item.key, item]));
  for (const item of remoteEntries) {
    merged.set(item.key, item);
  }

  return Array.from(merged.values());
}
