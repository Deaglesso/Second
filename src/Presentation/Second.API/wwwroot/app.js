const requestLog = document.getElementById("request-log");
const responseLog = document.getElementById("response-log");
const tokenInput = document.getElementById("access-token");
const tokenStatus = document.getElementById("token-status");
const clearTokenButton = document.getElementById("clear-token");

const setLog = (element, payload) => {
  element.textContent = typeof payload === "string" ? payload : JSON.stringify(payload, null, 2);
};

const parseBody = (text) => {
  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
};

const getToken = () => tokenInput.value.trim();

const setToken = (token) => {
  tokenInput.value = token ?? "";
  refreshTokenStatus();
};

const refreshTokenStatus = () => {
  const token = getToken();
  tokenStatus.textContent = token ? "Token loaded (Authorization header will be sent)." : "No token loaded.";
};

const jsonRequest = async (method, url, body, { authenticated = false } = {}) => {
  const headers = { "Content-Type": "application/json" };

  if (authenticated) {
    const token = getToken();
    if (!token) {
      throw new Error("No access token loaded. Login first or paste token in Session box.");
    }

    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });

  const text = await response.text();
  return { status: response.status, body: parseBody(text) };
};

const handlers = {
  register: async (form) => {
    const payload = {
      email: form.email.value,
      password: form.password.value,
    };

    const endpoint = "/api/auth/register";
    setLog(requestLog, { method: "POST", endpoint, payload });
    const result = await jsonRequest("POST", endpoint, payload);
    if (result?.body?.accessToken) {
      setToken(result.body.accessToken);
    }

    return result;
  },
  "request-email-verification": async (form) => {
    const payload = { email: form.email.value };
    const endpoint = "/api/auth/request-email-verification";
    setLog(requestLog, { method: "POST", endpoint, payload });
    return jsonRequest("POST", endpoint, payload);
  },
  "verify-email": async (form) => {
    const payload = { token: form.token.value };
    const endpoint = "/api/auth/verify-email";
    setLog(requestLog, { method: "POST", endpoint, payload });
    return jsonRequest("POST", endpoint, payload);
  },
  login: async (form) => {
    const payload = {
      email: form.email.value,
      password: form.password.value,
    };

    const endpoint = "/api/auth/login";
    setLog(requestLog, { method: "POST", endpoint, payload });
    const result = await jsonRequest("POST", endpoint, payload);

    if (result?.body?.accessToken) {
      setToken(result.body.accessToken);
    }

    return result;
  },
  me: async () => {
    const endpoint = "/api/auth/me";
    setLog(requestLog, { method: "GET", endpoint, authenticated: true });
    return jsonRequest("GET", endpoint, null, { authenticated: true });
  },
  logout: async () => {
    const endpoint = "/api/auth/logout";
    setLog(requestLog, { method: "POST", endpoint, authenticated: true });
    const result = await jsonRequest("POST", endpoint, {}, { authenticated: true });
    return result;
  },
  "forgot-password": async (form) => {
    const payload = { email: form.email.value };
    const endpoint = "/api/auth/forgot-password";
    setLog(requestLog, { method: "POST", endpoint, payload });
    return jsonRequest("POST", endpoint, payload);
  },
  "reset-password": async (form) => {
    const payload = {
      token: form.token.value,
      newPassword: form.newPassword.value,
    };

    const endpoint = "/api/auth/reset-password";
    setLog(requestLog, { method: "POST", endpoint, payload });
    return jsonRequest("POST", endpoint, payload);
  },
  "become-seller": async () => {
    const endpoint = "/api/auth/become-seller";
    setLog(requestLog, { method: "POST", endpoint, authenticated: true });
    const result = await jsonRequest("POST", endpoint, {}, { authenticated: true });

    if (result?.body?.accessToken) {
      setToken(result.body.accessToken);
    }

    return result;
  },
};

document.querySelectorAll("form[data-endpoint]").forEach((form) => {
  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    responseLog.textContent = "Loading...";
    const key = form.dataset.endpoint;
    const handler = handlers[key];

    if (!handler) {
      responseLog.textContent = "No handler registered.";
      return;
    }

    try {
      const result = await handler(form);
      setLog(responseLog, result);
    } catch (error) {
      responseLog.textContent = `Request failed: ${error.message}`;
    }
  });
});

clearTokenButton.addEventListener("click", () => {
  setToken("");
});

tokenInput.addEventListener("input", refreshTokenStatus);
refreshTokenStatus();
