import { createContext, useContext, useEffect, useMemo, useRef, useState, type PropsWithChildren } from "react";
import { api, setAccessToken } from "../api/client";
import type { CurrentUser } from "../types/api";

/** 认证上下文对页面暴露的会话状态、登录登出动作和权限判断能力。 */
type AuthContextValue = {
  user: CurrentUser | null;
  loading: boolean;
  login: (userName: string, password: string) => Promise<void>;
  logout: () => void;
  refresh: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
};

const storageKey = "aeroerp.auth.token";
const AuthContext = createContext<AuthContextValue | null>(null);

/**
 * 维护前端登录会话。
 * 启动时从 localStorage 恢复 token 并拉取当前用户，登录/登出时同步 API 客户端令牌。
 */
export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);
  const sessionRestoreStartedRef = useRef(false);

  useEffect(() => {
    if (sessionRestoreStartedRef.current) {
      return;
    }

    sessionRestoreStartedRef.current = true;
    const token = window.localStorage.getItem(storageKey);
    if (!token) {
      setLoading(false);
      return;
    }

    setAccessToken(token);
    void api.me()
      .then(setUser)
      .catch(() => {
        setAccessToken(null);
        window.localStorage.removeItem(storageKey);
        setUser(null);
      })
      .finally(() => setLoading(false));
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    user,
    loading,
    login: async (userName, password) => {
      const result = await api.login({ userName, password });
      window.localStorage.setItem(storageKey, result.accessToken);
      setAccessToken(result.accessToken);
      setUser(result.user);
    },
    logout: () => {
      window.localStorage.removeItem(storageKey);
      setAccessToken(null);
      setUser(null);
    },
    refresh: async () => {
      const current = await api.me();
      setUser(current);
    },
    hasPermission: (permission) =>
      user?.permissions.some((item) => item === permission) ?? false,
  }), [loading, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

/** 读取当前认证上下文，供页面和 Shell 判断用户、角色与权限。 */
export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth 必须在 AuthProvider 内使用");
  }

  return context;
}
