const runtimeImportMeta = import.meta as ImportMeta & {
  env?: Record<string, string | undefined>;
};

const rawApiBaseUrl = runtimeImportMeta.env?.VITE_API_BASE_URL?.trim();

const resolveDefaultApiBaseUrl = (): string => {
  const currentLocation = window.location;

  // Docker Compose default: frontend on 4173, backend on 8080.
  if (currentLocation.hostname === 'localhost' && currentLocation.port === '4173') {
    return 'http://localhost:8080';
  }

  // Kubernetes ingress default: use same host/origin (savepoint.local).
  return currentLocation.origin;
};

const resolveApiBaseUrl = (): string => {
  const currentLocation = window.location;

  // In ingress mode, force same-origin API to avoid cross-origin issues.
  if (currentLocation.hostname !== 'localhost' || currentLocation.port !== '4173') {
    return currentLocation.origin;
  }

  // In local Docker Compose mode, honor explicit env override if provided.
  if (rawApiBaseUrl && rawApiBaseUrl.length > 0) {
    return rawApiBaseUrl;
  }

  return resolveDefaultApiBaseUrl();
};

export const API_BASE_URL = resolveApiBaseUrl().replace(/\/+$/, '');

export const apiUrl = (path: string): string =>
  `${API_BASE_URL}${path.startsWith('/') ? path : `/${path}`}`;

const isRelativeApiPath = (value: string): boolean => value.startsWith('/api/');

// Patch global fetch once so existing relative /api calls work in containerized environments.
export const installApiFetchPatch = (): void => {
  const globalObject = globalThis as typeof globalThis & {
    __savepointFetchPatched?: boolean;
  };

  if (globalObject.__savepointFetchPatched) {
    return;
  }

  const originalFetch = globalThis.fetch.bind(globalThis);

  globalThis.fetch = (async (input: RequestInfo | URL, init?: RequestInit): Promise<Response> => {
    if (typeof input === 'string' && isRelativeApiPath(input)) {
      return originalFetch(apiUrl(input), init);
    }

    if (input instanceof URL && isRelativeApiPath(input.pathname) && input.origin === window.location.origin) {
      return originalFetch(apiUrl(`${input.pathname}${input.search}`), init);
    }

    if (input instanceof Request) {
      const requestUrl = new URL(input.url, window.location.origin);
      if (isRelativeApiPath(requestUrl.pathname) && requestUrl.origin === window.location.origin) {
        const rewrittenUrl = apiUrl(`${requestUrl.pathname}${requestUrl.search}`);
        const rewrittenRequest = new Request(rewrittenUrl, input);
        return originalFetch(rewrittenRequest, init);
      }
    }

    return originalFetch(input, init);
  }) as typeof fetch;

  globalObject.__savepointFetchPatched = true;
};
