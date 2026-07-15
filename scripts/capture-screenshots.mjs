import { chromium } from "playwright";
import { mkdir } from "node:fs/promises";
import path from "node:path";

const webBaseUrl = process.env.AEROERP_WEB_URL ?? "http://localhost:5173";
const apiBaseUrl = process.env.AEROERP_API_URL ?? "http://localhost:5099";
const outputDir = process.env.AEROERP_SCREENSHOT_DIR ?? "docs/images";
const userName = process.env.AEROERP_SCREENSHOT_USER ?? "admin";
const password = process.env.AEROERP_SCREENSHOT_PASSWORD ?? "Admin@123456";

const screenshots = [
  { file: "login.png", path: "/login", auth: false },
  { file: "workspace.png", path: "/workspace", auth: true },
  { file: "platform.png", path: "/platform", auth: true },
  { file: "people-management.png", path: "/people-management", auth: true },
  { file: "plugin-center.png", path: "/plugin-center", auth: true },
  { file: "master-data.png", path: "/master-data", auth: true },
  { file: "inventory.png", path: "/inventory", auth: true },
  { file: "finance.png", path: "/finance", auth: true },
  { file: "manufacturing.png", path: "/manufacturing", auth: true },
  { file: "integration.png", path: "/integration", auth: true },
];

async function loginToken() {
  const response = await fetch(`${apiBaseUrl}/api/platform/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userName, password }),
  });

  if (!response.ok) {
    throw new Error(`登录失败：${response.status}`);
  }

  const body = await response.json();
  return body.accessToken;
}

async function waitForApp(page) {
  await page.waitForLoadState("networkidle", { timeout: 20000 }).catch(() => undefined);
  await page.locator(".app-loading").waitFor({ state: "detached", timeout: 20000 }).catch(() => undefined);
  await page.locator(".ae-page-header, .workspace-hero, .login-panel").first().waitFor({ state: "visible", timeout: 20000 });
}

await mkdir(outputDir, { recursive: true });

const token = await loginToken();
const browser = await chromium.launch();
const context = await browser.newContext({
  viewport: { width: 1440, height: 920 },
  deviceScaleFactor: 1,
});

try {
  for (const item of screenshots) {
    const page = await context.newPage();
    if (item.auth) {
      await page.addInitScript((accessToken) => {
        window.localStorage.setItem("aeroerp.auth.token", accessToken);
      }, token);
    }

    await page.goto(`${webBaseUrl}${item.path}`, { waitUntil: "domcontentloaded", timeout: 30000 });
    await waitForApp(page);
    await page.screenshot({
      path: path.join(outputDir, item.file),
      fullPage: true,
    });
    await page.close();
    console.log(`captured ${item.file}`);
  }
} finally {
  await browser.close();
}
