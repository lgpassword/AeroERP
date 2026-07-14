import { useCallback, useEffect, useRef, useState } from "react";

/**
 * 加载页面远程数据的通用 hook。
 * 负责 loading/error 状态、手动刷新、卸载保护，以及开发环境 StrictMode 下的重复请求复用。
 */
export function useAsyncData<T>(loader: () => Promise<T>, dependencyKey: unknown = loader) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const loaderRef = useRef(loader);
  const mountedRef = useRef(false);
  const requestIdRef = useRef(0);
  const inFlightLoaderRef = useRef<(() => Promise<T>) | null>(null);
  const inFlightRef = useRef<Promise<T> | null>(null);

  loaderRef.current = loader;

  const runLoader = useCallback(() => {
    const currentLoader = loaderRef.current;
    // React StrictMode can start the same load twice in development; reuse the active request.
    if (inFlightRef.current && inFlightLoaderRef.current === currentLoader) {
      return inFlightRef.current;
    }

    const promise = currentLoader().finally(() => {
      if (inFlightRef.current === promise) {
        inFlightRef.current = null;
        inFlightLoaderRef.current = null;
      }
    });

    inFlightLoaderRef.current = currentLoader;
    inFlightRef.current = promise;
    return promise;
  }, []);

  const reload = useCallback(async () => {
    const requestId = requestIdRef.current + 1;
    requestIdRef.current = requestId;
    setLoading(true);
    setError(null);
    try {
      const result = await runLoader();
      // Ignore older requests that finish after a newer navigation or reload.
      if (mountedRef.current && requestIdRef.current === requestId) {
        setData(result);
      }
    } catch (err) {
      if (mountedRef.current && requestIdRef.current === requestId) {
        setError(err instanceof Error ? err.message : "Unknown error");
      }
    } finally {
      if (mountedRef.current && requestIdRef.current === requestId) {
        setLoading(false);
      }
    }
  }, [runLoader]);

  useEffect(() => {
    mountedRef.current = true;
    return () => {
      mountedRef.current = false;
      requestIdRef.current += 1;
    };
  }, []);

  useEffect(() => {
    void reload();
  }, [reload, dependencyKey]);

  return { data, loading, error, reload, setData };
}
