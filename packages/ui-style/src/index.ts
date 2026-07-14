/** AeroERP 前端设计令牌，集中定义颜色、圆角、阴影和间距。 */
export const tokens = {
  color: {
    canvas: "#0f172a",
    surface: "#111827",
    surfaceRaised: "#172033",
    surfaceMuted: "#0b1220",
    border: "#23314d",
    text: "#e5ecf6",
    textMuted: "#94a3b8",
    accent: "#0ea5e9",
    accentStrong: "#0284c7",
    success: "#10b981",
    warning: "#f59e0b",
    danger: "#ef4444"
  },
  radius: {
    sm: "6px",
    md: "8px",
    lg: "10px"
  },
  shadow: {
    panel: "0 16px 48px rgba(2, 6, 23, 0.28)"
  },
  spacing: {
    xs: "4px",
    sm: "8px",
    md: "12px",
    lg: "16px",
    xl: "24px",
    xxl: "32px"
  }
} as const;

/** 路由过渡、卡片进入等动效使用的统一节奏配置。 */
export const motion = {
  duration: {
    quick: 0.16,
    normal: 0.24,
    slow: 0.36
  },
  easing: [0.22, 1, 0.36, 1] as [number, number, number, number]
} as const;

/** 注入到应用根节点的基础 CSS 变量，供 UI Kit 和业务页面共享。 */
export const shellStyles = `
:root {
  color-scheme: dark;
  --ae-color-canvas: ${tokens.color.canvas};
  --ae-color-surface: ${tokens.color.surface};
  --ae-color-surface-raised: ${tokens.color.surfaceRaised};
  --ae-color-surface-muted: ${tokens.color.surfaceMuted};
  --ae-color-border: ${tokens.color.border};
  --ae-color-text: ${tokens.color.text};
  --ae-color-text-muted: ${tokens.color.textMuted};
  --ae-color-accent: ${tokens.color.accent};
  --ae-color-accent-strong: ${tokens.color.accentStrong};
  --ae-color-success: ${tokens.color.success};
  --ae-color-warning: ${tokens.color.warning};
  --ae-color-danger: ${tokens.color.danger};
  --ae-radius-sm: ${tokens.radius.sm};
  --ae-radius-md: ${tokens.radius.md};
  --ae-radius-lg: ${tokens.radius.lg};
  --ae-shadow-panel: ${tokens.shadow.panel};
}
`;
