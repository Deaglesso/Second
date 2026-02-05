const requestLog = document.getElementById("request-log");
const responseLog = document.getElementById("response-log");

const setLog = (element, payload) => {
  element.textContent = typeof payload === "string" ? payload : JSON.stringify(payload, null, 2);
};

const buildQuery = (params) => {
  const entries = Object.entries(params).filter(([, value]) => value !== "" && value !== undefined);
  if (!entries.length) {
    return "";
  }
  const query = new URLSearchParams(entries).toString();
  return `?${query}`;
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

const postJson = async (url, body) => {
  const response = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  const text = await response.text();
  return { status: response.status, body: parseBody(text) };
};

const getJson = async (url) => {
  const response = await fetch(url);
  const text = await response.text();
  return { status: response.status, body: parseBody(text) };
};

const handlers = {
  "create-seller": async (form) => {
    const payload = {
      displayName: form.displayName.value,
      bio: form.bio.value || null,
    };
    if (form.userId.value.trim()) {
      payload.userId = form.userId.value.trim();
    }
    const endpoint = "/api/sellerprofiles";
    setLog(requestLog, { method: "POST", endpoint, payload });
    return postJson(endpoint, payload);
  },
  "create-product": async (form) => {
    const imageUrls = form.imageUrls.value
      ? form.imageUrls.value.split(",").map((item) => item.trim()).filter(Boolean)
      : [];
    const payload = {
      sellerProfileId: form.sellerProfileId.value,
      title: form.title.value,
      priceText: form.priceText.value,
      condition: Number(form.condition.value),
      description: form.description.value,
      imageUrls,
    };
    const endpoint = "/api/products";
    setLog(requestLog, { method: "POST", endpoint, payload });
    return postJson(endpoint, payload);
  },
  "list-products": async (form) => {
    const query = buildQuery({
      pageNumber: form.pageNumber.value,
      pageSize: form.pageSize.value,
    });
    const endpoint = `/api/products/active${query}`;
    setLog(requestLog, { method: "GET", endpoint });
    return getJson(endpoint);
  },
  "list-products-by-seller": async (form) => {
    const query = buildQuery({
      pageNumber: form.pageNumber.value,
      pageSize: form.pageSize.value,
    });
    const endpoint = `/api/products/by-seller/${form.sellerProfileId.value}${query}`;
    setLog(requestLog, { method: "GET", endpoint });
    return getJson(endpoint);
  },
  "start-chat": async (form) => {
    const payload = {
      productId: form.productId.value,
      buyerId: form.buyerId.value,
      sellerId: form.sellerId.value,
    };
    const endpoint = "/api/chats";
    setLog(requestLog, { method: "POST", endpoint, payload });
    return postJson(endpoint, payload);
  },
  "send-message": async (form) => {
    const payload = {
      senderId: form.senderId.value,
      content: form.content.value,
    };
    const endpoint = `/api/chats/${form.chatRoomId.value}/messages`;
    setLog(requestLog, { method: "POST", endpoint, payload });
    return postJson(endpoint, payload);
  },
  "list-chats": async (form) => {
    const query = buildQuery({
      pageNumber: form.pageNumber.value,
      pageSize: form.pageSize.value,
    });
    const endpoint = `/api/chats/by-user/${form.userId.value}${query}`;
    setLog(requestLog, { method: "GET", endpoint });
    return getJson(endpoint);
  },
  "list-messages": async (form) => {
    const query = buildQuery({
      pageNumber: form.pageNumber.value,
      pageSize: form.pageSize.value,
    });
    const endpoint = `/api/chats/${form.chatRoomId.value}/messages${query}`;
    setLog(requestLog, { method: "GET", endpoint });
    return getJson(endpoint);
  },
  "create-report": async (form) => {
    const payload = {
      reporterId: form.reporterId.value,
      targetType: Number(form.targetType.value),
      targetId: form.targetId.value,
      reason: form.reason.value,
    };
    const endpoint = "/api/reports";
    setLog(requestLog, { method: "POST", endpoint, payload });
    return postJson(endpoint, payload);
  },
  "list-reports-by-target": async (form) => {
    const query = buildQuery({
      targetType: form.targetType.value,
      targetId: form.targetId.value,
      pageNumber: form.pageNumber.value,
      pageSize: form.pageSize.value,
    });
    const endpoint = `/api/reports/by-target${query}`;
    setLog(requestLog, { method: "GET", endpoint });
    return getJson(endpoint);
  },
  "list-reports-by-reporter": async (form) => {
    const query = buildQuery({
      pageNumber: form.pageNumber.value,
      pageSize: form.pageSize.value,
    });
    const endpoint = `/api/reports/by-reporter/${form.reporterId.value}${query}`;
    setLog(requestLog, { method: "GET", endpoint });
    return getJson(endpoint);
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
