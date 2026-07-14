import { motion } from "framer-motion";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

/** 登录页面，提交账号密码并把认证结果交给 AuthProvider 管理。 */
export function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [form, setForm] = useState({ userName: "", password: "" });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  return (
    <div className="login-screen">
      <motion.div
        className="login-panel"
        initial={{ opacity: 0, y: 18, scale: 0.98 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        transition={{ duration: 0.26 }}
      >
        <div className="login-brand">
          <div className="brand-mark large">AE</div>
          <div>
            <strong>AeroERP</strong>
            <span>模块化企业运营平台</span>
          </div>
        </div>

        <div className="login-copy">
          <h1>登录工作台</h1>
          <p>使用系统账号进入平台治理、主数据和采购闭环。</p>
        </div>

        <form
          className="stack-form"
          onSubmit={async (event) => {
            event.preventDefault();
            setSubmitting(true);
            setError(null);
            try {
              await login(form.userName, form.password);
              navigate("/", { replace: true });
            } catch (err) {
              setError(err instanceof Error ? err.message : "登录失败");
            } finally {
              setSubmitting(false);
            }
          }}
        >
          <input
            placeholder="账号"
            value={form.userName}
            onChange={(e) => setForm({ ...form, userName: e.target.value })}
          />
          <input
            type="password"
            placeholder="密码"
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
          />
          {error ? <div className="form-error">{error}</div> : null}
          <button type="submit" disabled={submitting}>
            {submitting ? "登录中..." : "进入系统"}
          </button>
        </form>

        <div className="login-hint">
          <span>默认账号：admin</span>
          <span>默认密码：Admin@123456</span>
        </div>
      </motion.div>
    </div>
  );
}
