import type { PropsWithChildren, ReactNode } from "react";
import { motion } from "framer-motion";
import { shellStyles } from "@aeroerp/ui-style";

/** 将 UI Style 的全局变量注入页面，保证共享组件和业务页面使用同一套视觉令牌。 */
export function StyleRegistry() {
  return <style>{shellStyles}</style>;
}

/** 业务页面的标准外壳，统一标题、动作区和进入动效。 */
export function PageShell({ title, actions, children }: PropsWithChildren<{ title: string; actions?: ReactNode }>) {
  return (
    <motion.section
      initial={{ opacity: 0, y: 18 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.24 }}
      className="ae-page-shell"
    >
      <header className="ae-page-header">
        <div>
          <h1>{title}</h1>
        </div>
        <div className="ae-page-actions">{actions}</div>
      </header>
      {children}
    </motion.section>
  );
}

/** 页面内的功能区块容器，用于呈现一组相关表单、表格或状态信息。 */
export function SectionBlock({ title, hint, children }: PropsWithChildren<{ title: string; hint?: string }>) {
  return (
    <section className="ae-section-block">
      <div className="ae-section-head">
        <h2>{title}</h2>
        {hint ? <p>{hint}</p> : null}
      </div>
      {children}
    </section>
  );
}

/** 空数据或不可用状态提示，允许接入下一步操作按钮。 */
export function EmptyState({ title, description, action }: { title: string; description: string; action?: ReactNode }) {
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.98 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.2 }}
      className="ae-empty-state"
    >
      <h3>{title}</h3>
      <p>{description}</p>
      {action ? <div>{action}</div> : null}
    </motion.div>
  );
}

/** 仪表盘式指标块，用固定 tone 表达普通、成功和预警状态。 */
export function StatTile({ label, value, tone = "default" }: { label: string; value: string | number; tone?: "default" | "success" | "warning" }) {
  return (
    <div className={`ae-stat-tile ae-stat-${tone}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}
