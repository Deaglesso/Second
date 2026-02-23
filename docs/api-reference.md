# Second API Documentation (Frontend-Complete)

> **Audience:** Mid-level frontend/backend engineers integrating with the Second marketplace API.
>
> **Goal:** You should be able to build a complete client integration without opening backend source code.

---

## 1) Overview & Introduction

Second API powers a marketplace-style workflow with:

- account creation and sign-in,
- email verification and password recovery,
- seller onboarding,
- product listing management,
- buyer/seller chat,
- moderation reports,
- admin controls for seller limits.

### What problem it solves

It provides a single HTTP API for all primary marketplace operations so web/mobile clients can:

1. onboard users,
2. enforce role-based flows (`User`, `Seller`, `Admin`),
3. browse and manage listings,
4. message in listing-specific chat rooms,
5. file moderation reports.

### Key features and use cases

- JWT authentication with revocation on logout.
- Role-aware authorization (`SellerOnly`, `Admin` routes).
- Validation-rich input rules with predictable `ProblemDetails` errors.
- Consistent pagination response model.
- Email-based verification/reset workflows.

### Architecture overview

- **Protocol:** REST (JSON over HTTP)
- **Style:** Resource-based controllers (`/api/v1/auth`, `/api/v1/products`, `/api/v1/chats`, etc.)
- **Storage:** SQL Server for persistent data, Redis for token revocation checks.

### Base URL, versioning, environments

- **Local (Docker default):** `http://localhost:8080`
- **Base API prefix:** `/api`
- **Current versioning strategy:** URI versioning via `/api/v1/...`.

| Environment | Base URL | Purpose | Data policy |
|---|---|---|---|
| Local development | `http://localhost:8080` | Developer testing | Ephemeral/local |
| Sandbox (recommended) | `https://sandbox.api.second.example` | Pre-production integration testing | Non-production test data |
| Production | `https://api.second.example` | Live traffic | Real customer data |

> **Note:** Current stable base path is `/api/v1/...`. Introduce `/api/v2/...` for future breaking changes and keep a migration window.

> **Warning:** Never point automated tests at production unless they are explicitly read-only tests with production-safe credentials.

Suggested environments:

- `sandbox` (test data + safe keys)
- `production` (real users/data)

---

## 2) Authentication & Security

### Auth methods used

- **JWT Bearer** in `Authorization` header.

```http
Authorization: Bearer <accessToken>
```

### Step-by-step setup

1. Register via `POST /api/v1/auth/register`.
2. Verify email (`request-email-verification` -> `verify-email`).
3. Log in via `POST /api/v1/auth/login`.
4. Store `accessToken` securely in memory/session state.
5. Send token in `Authorization` for protected routes.

### Token behavior

- JWT includes `sub`, `jti`, `email`, `nameidentifier`, and `role` claims.
- Expiry is controlled by backend configuration (`Jwt:ExpiresInMinutes`, default 60).
- `POST /api/v1/auth/logout` revokes token `jti` (Redis-backed).

> **Warning:** No refresh-token endpoint currently exists. Re-authenticate when token expires.

### Scopes / permissions model

This API enforces **role-based permissions** rather than OAuth scopes:

| Permission boundary | Enforced by |
|---|---|
| Authenticated-only actions | Valid bearer token |
| Seller actions (create/edit products) | `Seller` role |
| Admin actions (seller listing limits) | `Admin` role |

### Token expiry and re-authentication flow

1. Call protected endpoint with access token.
2. If response is `401`, treat token as invalid/expired.
3. Clear local auth state.
4. Re-run login (`POST /api/v1/auth/login`) to obtain a new token.
5. Retry the original protected request once with the new token.

> **Note:** Keep login retry count low (recommended max 1) to avoid lockout-like behavior caused by repeatedly retrying invalid credentials.

### Roles and permissions

- `User`: basic authenticated account.
- `Seller`: can create/update listings and images.
- `Admin`: can update seller listing limits.

### Security best practices for consumers

- Always use HTTPS outside local development.
- Never store JWT long-term in insecure storage if avoidable.
- Clear app auth state on `401`.
- Implement client-side request throttling and exponential backoff for transient failures.
- Restrict frontend origin via CORS in production.
- Use allow-listed backend domains in frontend configuration.

### Security hardening checklist

- Enforce HTTPS and reject mixed content on web clients.
- Rotate API credentials for service-to-service integrations on a fixed schedule.
- Use short-lived session storage for tokens and clear on browser close for high-risk apps.
- Implement client-side and edge rate limiting for login, reset-password, and chat send actions.
- If running in enterprise environments, allow traffic only from approved egress IP ranges.

### Platform-level security notes

- CORS currently allows `http://localhost:3000`.
- Revoked JWTs are checked on token validation.
- Email/password flows use generic responses where needed to reduce account enumeration risk.

---

## 3) Getting Started / Quickstart

### Prerequisites

- Running API instance (`docker compose up --build` from repo root).
- Ability to make HTTP requests (cURL/Postman/HTTP client).
- Optional: Node.js or Python for scripted examples.

### Hello World flow (working sequence)

This sequence registers, verifies (manual token retrieval from email/log), logs in, and fetches profile.

#### 3.1 cURL

```bash
BASE_URL="http://localhost:8080"
EMAIL="frontend.demo.$(date +%s)@example.com"
PASSWORD="StrongPass1"

# Register
REGISTER_RESPONSE=$(curl -sS -X POST "$BASE_URL/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")

echo "$REGISTER_RESPONSE"

# Request verification email
curl -sS -X POST "$BASE_URL/api/v1/auth/request-email-verification" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\"}" | jq

# Manually read token from email/log and set:
VERIFY_TOKEN="${VERIFY_TOKEN:?Set VERIFY_TOKEN from verification email link}"

curl -sS -X POST "$BASE_URL/api/v1/auth/verify-email" \
  -H "Content-Type: application/json" \
  -d "{\"token\":\"$VERIFY_TOKEN\"}" | jq

# Login
LOGIN_RESPONSE=$(curl -sS -X POST "$BASE_URL/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")

ACCESS_TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.accessToken')

# Me
curl -sS "$BASE_URL/api/v1/auth/me" \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq
```

#### 3.2 JavaScript (Node 18+)

```js
const BASE_URL = 'http://localhost:8080';
const email = `frontend.demo.${Date.now()}@example.com`;
const password = 'StrongPass1';

async function post(path, body, token) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: JSON.stringify(body)
  });
  return { status: res.status, json: await res.json() };
}

async function get(path, token) {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {}
  });
  return { status: res.status, json: await res.json() };
}

(async () => {
  const register = await post('/api/v1/auth/register', { email, password });
  console.log('register', register.status, register.json);

  await post('/api/v1/auth/request-email-verification', { email });
  console.log('Requested verification email; now verify manually from email/log token.');

  // const verify = await post('/api/v1/auth/verify-email', { token: process.env.VERIFY_TOKEN });
  // console.log('verify', verify.status, verify.json);

  const login = await post('/api/v1/auth/login', { email, password });
  console.log('login', login.status, login.json);

  const me = await get('/api/v1/auth/me', login.json.accessToken);
  console.log('me', me.status, me.json);
})();
```

#### 3.3 Python (requests)

```python
import os
import time
import requests

BASE_URL = "http://localhost:8080"
email = f"frontend.demo.{int(time.time())}@example.com"
password = "StrongPass1"

register = requests.post(f"{BASE_URL}/api/v1/auth/register", json={"email": email, "password": password})
print("register", register.status_code, register.json())

requests.post(f"{BASE_URL}/api/v1/auth/request-email-verification", json={"email": email})
print("verification requested; verify email manually with token from email/log")

# verify = requests.post(f"{BASE_URL}/api/v1/auth/verify-email", json={"token": os.environ["VERIFY_TOKEN"]})
# print("verify", verify.status_code, verify.json())

login = requests.post(f"{BASE_URL}/api/v1/auth/login", json={"email": email, "password": password})
print("login", login.status_code, login.json())

token = login.json()["accessToken"]
me = requests.get(f"{BASE_URL}/api/v1/auth/me", headers={"Authorization": f"Bearer {token}"})
print("me", me.status_code, me.json())
```

---

## 4) Core Concepts & Glossary

### Core terms

- **User:** any registered account.
- **Seller:** user allowed to publish listings.
- **Admin:** elevated role with seller-management abilities.
- **Product:** listing created by seller.
- **ChatRoom:** conversation bound to a `(productId, buyerId, sellerId)` tuple.
- **Message:** chat entry in a room.
- **Report:** moderation signal against a product/seller/message.

### Object relationship summary

- `User (Seller)` 1..N `Product`
- `Product` 1..N `ProductImage`
- `Product + Buyer + Seller` -> unique-ish `ChatRoom` behavior (returns existing room)
- `ChatRoom` 1..N `Message`
- `User` 1..N `Report`

### Idempotency conventions

- `DELETE /api/v1/products/images/{imageId}` is idempotent: returns `204` even if already removed.
- `POST /api/v1/chats` behaves quasi-idempotent for same product/buyer/seller triple by returning existing room.

### Pagination conventions

- Query: `pageNumber` (default `1`), `pageSize` (default `20`, max `100`)
- Response: `items`, `pageNumber`, `pageSize`, `totalCount`, `totalPages`

### Filtering and sorting conventions

- Filtering examples currently exposed:
  - products by active state (`/api/v1/products/active`)
  - products by seller
  - reports by target
- Explicit client-provided sorting parameters are not currently exposed.

---

## 5) Endpoints / API Reference

> **Format per endpoint:** method/path, purpose, auth, parameters, examples (cURL + JavaScript), response schema, success/error samples, status codes.

### Endpoint catalog

| Domain | Endpoint count | Base path |
|---|---:|---|
| Authentication | 9 | `/api/v1/auth` |
| Products | 10 | `/api/v1/products` |
| Chats | 4 | `/api/v1/chats` |
| Reports | 2 | `/api/v1/reports` |
| Admin seller controls | 1 | `/api/v1/admin/sellers` |

### Shared response schemas

#### AuthResponseDto

```json
{
  "userId": "b7a6b87a-8f53-4ac7-bda6-a2c6ebf07f96",
  "email": "user@example.com",
  "role": "User",
  "accessToken": "<jwt>",
  "expiresAtUtc": "2026-02-18T15:00:00Z"
}
```

#### PagedResult<T>

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 0,
  "totalPages": 0
}
```

---

## 5.1 Auth (`/api/v1/auth`)

### 1) POST `/api/v1/auth/register`

Creates a new user account.

- **Auth:** none
- **Headers:** `Content-Type: application/json`
- **Body:**
  - `email` (string, required, valid email, max 320)
  - `password` (string, required, min 8, uppercase+lowercase+number)

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"alice.register@example.com","password":"StrongPass1"}'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'alice.register@example.com', password: 'StrongPass1' })
});
```

**Success (`200`)**
```json
{
  "userId": "5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60",
  "email": "alice.register@example.com",
  "role": "User",
  "accessToken": "<jwt>",
  "expiresAtUtc": "2026-02-18T15:00:00Z"
}
```

**Error example (`409 email_already_exists`)**
```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "An account with this email already exists.",
  "errorCode": "email_already_exists"
}
```

**Status codes:** `200`, `400`, `409`.

---

### 2) POST `/api/v1/auth/login`

Authenticates a verified user.

- **Auth:** none
- **Body:** `email`, `password`

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"alice.register@example.com","password":"StrongPass1"}'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'alice.register@example.com', password: 'StrongPass1' })
});
```

**Success (`200`)**: `AuthResponseDto`

**Error examples**

`401 invalid_credentials`
```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid email or password.",
  "errorCode": "invalid_credentials"
}
```

`401 email_not_verified`
```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Please verify your email before logging in.",
  "errorCode": "email_not_verified"
}
```

**Status codes:** `200`, `400`, `401`.

---

### 3) POST `/api/v1/auth/logout`

Revokes current JWT (`jti`) and clears auth cookies.

- **Auth:** required
- **Headers:** `Authorization: Bearer <token>`

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/auth/logout" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/auth/logout', {
  method: 'POST',
  headers: { Authorization: `Bearer ${accessToken}` }
});
```

**Success (`200`)**
```json
{ "message": "Logged out successfully." }
```

**Status codes:** `200`, `401`.

---

### 4) POST `/api/v1/auth/request-email-verification`

Requests verification email (generic response for enumeration safety).

- **Auth:** none
- **Body:** `email`

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/auth/request-email-verification" \
  -H "Content-Type: application/json" \
  -d '{"email":"alice.register@example.com"}'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/auth/request-email-verification', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'alice.register@example.com' })
});
```

**Success (`200`)**
```json
{ "message": "If the account exists and is unverified, a verification email has been sent." }
```

**Status codes:** `200`, `400`.

---

### 5) POST `/api/v1/auth/verify-email`

Verifies account with emailed token.

- **Auth:** none
- **Body:** `token` (string, required, max 512)

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/auth/verify-email" \
  -H "Content-Type: application/json" \
  -d '{"token":"${VERIFY_TOKEN}"}'
```

**JavaScript**
```js
const verifyToken = process.env.VERIFY_TOKEN;
await fetch('http://localhost:8080/api/v1/auth/verify-email', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ token: verifyToken })
});
```

**Success (`200`)**
```json
{ "message": "Email verified successfully." }
```

**Error (`400 invalid_email_verification_token`)**
```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "The token is invalid or expired.",
  "errorCode": "invalid_email_verification_token"
}
```

**Status codes:** `200`, `400`.

---

### 6) POST `/api/v1/auth/forgot-password`

Starts reset flow with generic response.

- **Auth:** none
- **Body:** `email`

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/auth/forgot-password" \
  -H "Content-Type: application/json" \
  -d '{"email":"alice.register@example.com"}'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/auth/forgot-password', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'alice.register@example.com' })
});
```

**Success (`200`)**
```json
{ "message": "If an account exists, a reset link has been sent." }
```

**Status codes:** `200`, `400`.

---

### 7) POST `/api/v1/auth/reset-password`

Completes password reset with token.

- **Auth:** none
- **Body:**
  - `token` (required)
  - `newPassword` (required, min 8, uppercase+lowercase+number)

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/auth/reset-password" \
  -H "Content-Type: application/json" \
  -d '{"token":"'"$RESET_TOKEN"'","newPassword":"NewStrong1"}'
```

**JavaScript**
```js
const resetToken = process.env.RESET_TOKEN;
await fetch('http://localhost:8080/api/v1/auth/reset-password', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ token: resetToken, newPassword: 'NewStrong1' })
});
```

**Success (`200`)**
```json
{ "message": "Password reset successfully." }
```

**Error (`400 invalid_reset_token`)**
```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "The reset token is invalid or expired.",
  "errorCode": "invalid_reset_token"
}
```

**Status codes:** `200`, `400`.

---

### 8) POST `/api/v1/auth/become-seller`

Upgrades current authenticated `User` role to `Seller` and returns fresh JWT.

- **Auth:** required

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/auth/become-seller" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/auth/become-seller', {
  method: 'POST',
  headers: { Authorization: `Bearer ${accessToken}` }
});
```

**Success (`200`)**: `AuthResponseDto`

**Status codes:** `200`, `401`, `404`.

---

### 9) GET `/api/v1/auth/me`

Returns current user profile.

- **Auth:** required

**cURL**
```bash
curl -sS "http://localhost:8080/api/v1/auth/me" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/auth/me', {
  headers: { Authorization: `Bearer ${accessToken}` }
});
```

**Success (`200`)**
```json
{
  "id": "5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60",
  "email": "alice.register@example.com",
  "role": "Seller",
  "emailVerified": true,
  "sellerRating": 0,
  "listingLimit": 10,
  "createdAt": "2026-02-18T13:00:00Z"
}
```

**Status codes:** `200`, `401`, `404`.

---

## 5.2 Products (`/api/v1/products`)

### 10) POST `/api/v1/products`

Creates a product (seller/admin only).

- **Auth:** required (`Seller` or `Admin`)
- **Body:**
  - `sellerUserId` (guid, required)
  - `title` (string, required, <=200)
  - `description` (string, required, <=2000)
  - `priceText` (string, optional, <=100)
  - `price` (integer cents, required, >=0)
  - `condition` (`New|LikeNew|Used|HeavilyUsed|Vintage`)
  - `imageUrls` (string[], optional, each <=500)

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/products" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "sellerUserId":"5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60",
    "title":"iPhone 14",
    "description":"Used, excellent condition",
    "priceText":"$550",
    "price":55000,
    "condition":"Used",
    "imageUrls":["https://picsum.photos/seed/iphone14a/800/600"]
  }'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/products', {
  method: 'POST',
  headers: {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    sellerUserId: '5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60',
    title: 'iPhone 14',
    description: 'Used, excellent condition',
    priceText: '$550',
    price: 55000,
    condition: 'Used',
    imageUrls: ['https://picsum.photos/seed/iphone14a/800/600']
  })
});
```

**Success (`201`)**: `ProductDto`

**Error (`409 listing_limit_reached`)**
```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "Seller reached active listing limit (10).",
  "errorCode": "listing_limit_reached"
}
```

**Status codes:** `201`, `400`, `401`, `403`, `404`, `409`.

---

### 11) GET `/api/v1/products/{productId}`

Returns one product.

- **Auth:** none
- **Path:** `productId` (guid, required)

**cURL**
```bash
curl -sS "http://localhost:8080/api/v1/products/11111111-1111-1111-1111-111111111111"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/products/11111111-1111-1111-1111-111111111111');
```

**Success (`200`)**: `ProductDto`

**Error (`404 product_not_found`)**
```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "No product found with id 11111111-1111-1111-1111-111111111111.",
  "errorCode": "product_not_found"
}
```

**Status codes:** `200`, `404`.

---

### 12) GET `/api/v1/products/active`

Returns paged active products.

- **Auth:** none
- **Query:** `pageNumber` (int >=1), `pageSize` (1..100), `q`, `condition`, `minPrice`, `maxPrice`, `sortBy` (`newest|price_asc|price_desc`)

**cURL**
```bash
curl -sS "http://localhost:8080/api/v1/products/active?pageNumber=1&pageSize=20&q=iphone&condition=Used&minPrice=30000&maxPrice=70000&sortBy=price_asc"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/products/active?pageNumber=1&pageSize=20&q=iphone&condition=Used&minPrice=30000&maxPrice=70000&sortBy=price_asc');
```

**Success (`200`)**: `PagedResult<ProductDto>`

**Status codes:** `200`, `400`.

---

### 13) GET `/api/v1/products/by-seller/{sellerUserId}`

Returns paged products for one seller.

- **Auth:** currently public
- **Path:** `sellerUserId` (guid)
- **Query:** pagination

**cURL**
```bash
curl -sS "http://localhost:8080/api/v1/products/by-seller/5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60?pageNumber=1&pageSize=20"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/products/by-seller/5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60?pageNumber=1&pageSize=20');
```

**Success (`200`)**: `PagedResult<ProductDto>`

**Status codes:** `200`, `400`.

---

### 14) PUT `/api/v1/products/{productId}`

Updates product metadata and active state.

- **Auth:** required (`Seller` or `Admin`)
- **Path:** `productId` (guid)
- **Body:** `title`, `description`, `priceText`, `price`, `condition`, `status` (`Active|Sold|Paused|Archived`)

**cURL**
```bash
curl -sS -X PUT "http://localhost:8080/api/v1/products/11111111-1111-1111-1111-111111111111" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title":"iPhone 14 - price drop",
    "description":"Used, excellent condition, includes case",
    "priceText":"$520",
    "price":52000,
    "condition":"Used",
    "status":"Active"
  }'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/products/11111111-1111-1111-1111-111111111111', {
  method: 'PUT',
  headers: {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    title: 'iPhone 14 - price drop',
    description: 'Used, excellent condition, includes case',
    priceText: '$520',
    price: 52000,
    condition: 'Used',
    status: "Active"
  })
});
```

**Success (`200`)**: `ProductDto`

**Status codes:** `200`, `400`, `401`, `403`, `404`, `409`.

---

### 15) POST `/api/v1/products/{productId}/images`

Adds product image.

- **Auth:** required (`Seller` or `Admin`)
- **Path:** `productId` (guid)
- **Body:**
  - `imageUrl` (required, <=500)
  - `order` (int, >=0)

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/products/11111111-1111-1111-1111-111111111111/images" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"imageUrl":"https://picsum.photos/seed/iphone14b/800/600","order":1}'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/products/11111111-1111-1111-1111-111111111111/images', {
  method: 'POST',
  headers: {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ imageUrl: 'https://picsum.photos/seed/iphone14b/800/600', order: 1 })
});
```

**Success (`200`)**
```json
{
  "id": "d69b55c6-d805-4ec5-95d6-3a72f2a94f72",
  "productId": "11111111-1111-1111-1111-111111111111",
  "imageUrl": "https://picsum.photos/seed/iphone14b/800/600",
  "order": 1
}
```

**Status codes:** `200`, `400`, `401`, `403`, `404`.

---

### 16) DELETE `/api/v1/products/images/{imageId}`

Removes product image.

- **Auth:** required (`Seller` or `Admin`)
- **Path:** `imageId` (guid)

**cURL**
```bash
curl -i -X DELETE "http://localhost:8080/api/v1/products/images/d69b55c6-d805-4ec5-95d6-3a72f2a94f72" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/products/images/d69b55c6-d805-4ec5-95d6-3a72f2a94f72', {
  method: 'DELETE',
  headers: { Authorization: `Bearer ${accessToken}` }
});
```

**Success (`204`)** no body.

> **Note:** This endpoint is idempotent and still returns `204` if image does not exist.

**Status codes:** `204`, `401`, `403`.

---

### 17) DELETE `/api/v1/products/{productId}`

Archives a product (soft delete) by setting `status` to `Archived`.

- **Auth:** required (`Seller` or `Admin`)
- **Path:** `productId` (guid)
- **Authorization:** owner seller or admin.

**cURL**
```bash
curl -i -X DELETE "http://localhost:8080/api/v1/products/11111111-1111-1111-1111-111111111111" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

**Success (`204`)** no body.

**Status codes:** `204`, `401`, `403`.

---

## 5.3 Chats (`/api/v1/chats`)

> **Warning:** Controller currently has no `[Authorize]`. Protect through gateway or backend update before production.

### 17) POST `/api/v1/chats`

Starts chat room or returns existing room for same product/buyer/seller.

- **Auth:** currently public
- **Body:** `productId`, `buyerId`, `sellerId` (all guid required)

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/chats" \
  -H "Content-Type: application/json" \
  -d '{
    "productId":"11111111-1111-1111-1111-111111111111",
    "buyerId":"22222222-2222-2222-2222-222222222222",
    "sellerId":"5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60"
  }'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/chats', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    productId: '11111111-1111-1111-1111-111111111111',
    buyerId: '22222222-2222-2222-2222-222222222222',
    sellerId: '5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60'
  })
});
```

**Success (`201`)**: `ChatRoomDto`

**Status codes:** `201`, `400`.

---

### 18) GET `/api/v1/chats/{chatRoomId}`

Returns chat room by id.

- **Auth:** currently public
- **Path:** `chatRoomId` (guid)

**cURL**
```bash
curl -sS "http://localhost:8080/api/v1/chats/33333333-3333-3333-3333-333333333333"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/chats/33333333-3333-3333-3333-333333333333');
```

**Success (`200`)**: `ChatRoomDto`

**Error (`404 chat_room_not_found`)**
```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "No chat room found with id 33333333-3333-3333-3333-333333333333.",
  "errorCode": "chat_room_not_found"
}
```

**Status codes:** `200`, `404`.

---

### 19) GET `/api/v1/chats/by-user/{userId}`

Returns paged chat rooms where user is participant.

- **Auth:** currently public
- **Path:** `userId` (guid)
- **Query:** pagination

**cURL**
```bash
curl -sS "http://localhost:8080/api/v1/chats/by-user/22222222-2222-2222-2222-222222222222?pageNumber=1&pageSize=20"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/chats/by-user/22222222-2222-2222-2222-222222222222?pageNumber=1&pageSize=20');
```

**Success (`200`)**: `PagedResult<ChatRoomDto>`

**Status codes:** `200`, `400`.

---

### 20) POST `/api/v1/chats/{chatRoomId}/messages`

Sends a message to chat room.

- **Auth:** currently public
- **Path:** `chatRoomId` (guid)
- **Body:**
  - `senderId` (guid, required)
  - `content` (string, required, <=2000)

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/chats/33333333-3333-3333-3333-333333333333/messages" \
  -H "Content-Type: application/json" \
  -d '{
    "senderId":"22222222-2222-2222-2222-222222222222",
    "content":"Hi, is this product still available?"
  }'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/chats/33333333-3333-3333-3333-333333333333/messages', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    senderId: '22222222-2222-2222-2222-222222222222',
    content: 'Hi, is this product still available?'
  })
});
```

**Success (`200`)**: `MessageDto`

**Status codes:** `200`, `400`, `404`.

---

### 21) GET `/api/v1/chats/{chatRoomId}/messages`

Returns paged messages for chat room.

- **Auth:** currently public
- **Path:** `chatRoomId` (guid)
- **Query:** pagination

**cURL**
```bash
curl -sS "http://localhost:8080/api/v1/chats/33333333-3333-3333-3333-333333333333/messages?pageNumber=1&pageSize=20"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/chats/33333333-3333-3333-3333-333333333333/messages?pageNumber=1&pageSize=20');
```

**Success (`200`)**: `PagedResult<MessageDto>`

**Status codes:** `200`, `400`.

---

## 5.4 Reports (`/api/v1/reports`)

> **Warning:** Controller currently has no `[Authorize]`. Add server-side auth if required by policy.

### 22) POST `/api/v1/reports`

Creates moderation report.

- **Auth:** currently public
- **Body:**
  - `reporterId` (guid, required)
  - `targetType` (`Product|Seller|Message`, required)
  - `targetId` (guid, required)
  - `reason` (string, required, <=1000)

**cURL**
```bash
curl -sS -X POST "http://localhost:8080/api/v1/reports" \
  -H "Content-Type: application/json" \
  -d '{
    "reporterId":"22222222-2222-2222-2222-222222222222",
    "targetType":"Product",
    "targetId":"11111111-1111-1111-1111-111111111111",
    "reason":"Suspicious listing details"
  }'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/reports', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    reporterId: '22222222-2222-2222-2222-222222222222',
    targetType: 'Product',
    targetId: '11111111-1111-1111-1111-111111111111',
    reason: 'Suspicious listing details'
  })
});
```

**Success (`200`)**: `ReportDto`

**Status codes:** `200`, `400`.

---

### 23) GET `/api/v1/reports/by-target`

Returns paged reports for a target entity.

- **Auth:** currently public
- **Query:**
  - `targetType` (enum, required)
  - `targetId` (guid, required)
  - pagination query

**cURL**
```bash
curl -sS "http://localhost:8080/api/v1/reports/by-target?targetType=Product&targetId=11111111-1111-1111-1111-111111111111&pageNumber=1&pageSize=20"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/reports/by-target?targetType=Product&targetId=11111111-1111-1111-1111-111111111111&pageNumber=1&pageSize=20');
```

**Success (`200`)**: `PagedResult<ReportDto>`

**Status codes:** `200`, `400`.

---

### 24) GET `/api/v1/reports/by-reporter/{reporterId}`

Returns paged reports by reporter id.

- **Auth:** currently public
- **Path:** `reporterId` (guid)
- **Query:** pagination

**cURL**
```bash
curl -sS "http://localhost:8080/api/v1/reports/by-reporter/22222222-2222-2222-2222-222222222222?pageNumber=1&pageSize=20"
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/reports/by-reporter/22222222-2222-2222-2222-222222222222?pageNumber=1&pageSize=20');
```

**Success (`200`)**: `PagedResult<ReportDto>`

**Status codes:** `200`, `400`.

---

## 5.5 Admin Sellers (`/api/v1/admin/sellers`)

### 25) PATCH `/api/v1/admin/sellers/{sellerUserId}/listing-limit`

Updates seller listing limit.

- **Auth:** required role `Admin`
- **Path:** `sellerUserId` (guid)
- **Body:** `listingLimit` (int, required, >=0)

**cURL**
```bash
curl -sS -X PATCH "http://localhost:8080/api/v1/admin/sellers/5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60/listing-limit" \
  -H "Authorization: Bearer $ADMIN_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"listingLimit":25}'
```

**JavaScript**
```js
await fetch('http://localhost:8080/api/v1/admin/sellers/5ef5cb3d-e1e8-4d9d-95f1-6d5f2fca1a60/listing-limit', {
  method: 'PATCH',
  headers: {
    Authorization: `Bearer ${adminAccessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ listingLimit: 25 })
});
```

**Success (`200`)**: `UserDto`

**Error (`400 user_not_seller`)**
```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "The specified user is not a seller.",
  "errorCode": "user_not_seller"
}
```

**Status codes:** `200`, `400`, `401`, `403`, `404`.

---

## 6) Error Handling

All failures are returned as `ProblemDetails`-style JSON.

### Canonical error shape

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/auth/register",
  "traceId": "00-...",
  "timestampUtc": "2026-02-18T14:21:22.1234567Z",
  "errorCode": "validation_failed",
  "errors": {
    "Email": ["'Email' is not a valid email address."]
  }
}
```

### Error reference table

| errorCode | HTTP | Meaning | Suggested fix |
|---|---:|---|---|
| `validation_failed` | 400 | DTO/FluentValidation rules failed | Correct field values using `errors` map |
| `invalid_json` | 400 | Malformed JSON payload | Validate JSON before send |
| `invalid_credentials` | 401 | Email/password invalid | Re-check credentials |
| `email_not_verified` | 401 | Email not verified | Complete verify-email workflow |
| `invalid_user_token` | 401 | Missing/invalid user claim in JWT | Re-login and send valid bearer token |
| `token_metadata_missing` | 401 | Missing token claims (logout path) | Use a valid JWT generated by this API |
| `user_not_found` | 404 | User does not exist | Refresh local user state, handle deleted users |
| `seller_not_found` | 404 | Seller not found | Use an existing seller id |
| `product_not_found` | 404 | Product missing | Verify product id |
| `chat_room_not_found` | 404 | Chat room missing | Verify room id |
| `listing_limit_reached` | 409 | Seller exceeded active listing capacity | Increase listing limit or deactivate another listing |
| `invalid_email_verification_token` | 400 | Verification token invalid/expired | Request new verification token |
| `invalid_reset_token` | 400 | Reset token invalid/expired | Request password reset again |
| `user_not_seller` | 400 | Admin tried to set limit for non-seller | Promote user to seller first |
| `invalid_pagination_parameters` | 400 | pageNumber/pageSize out of range | Use pageNumber>=1 and 1<=pageSize<=100 |

### Common failure scenarios

1. **401 after logout with same token**: expected due to revocation.
2. **409 on product create**: seller hit listing cap.
3. **400 on enum fields**: must send enum **string values** exactly.
4. **400 validation**: parse and display `errors` object per field.

### Retry guidance

- **Do retry** for transient `500`/network timeouts using exponential backoff.
- **Do not retry unchanged** for `400/401/403/404/409` until request data/auth state is fixed.
- Recommended backoff: 0.5s, 1s, 2s, 4s (max 4 tries).

---

## 7) Rate Limits & Quotas

Current backend implementation does **not** expose explicit per-plan rate limits or rate-limit headers.

> **Warning:** Until server-side rate limiting is added, clients should implement local throttling to prevent accidental bursts.

Suggested client behavior:

- Cap concurrent writes (e.g., max 3 at once).
- Debounce rapid user actions (search/filter typing, repeated submits).
- Add retry/backoff only for transient errors.

### Planned plan-tier limits (documentation target)

Until server-enforced limits are shipped, use the following defaults in clients and gateways:

| Tier | Suggested sustained limit | Burst limit | Notes |
|---|---:|---:|---|
| Sandbox | 30 requests/minute | 10 requests/5 seconds | Keep CI noise low |
| Production Standard | 120 requests/minute | 30 requests/10 seconds | Suitable for most web/mobile clients |
| Production Enterprise | 600 requests/minute | 120 requests/10 seconds | High-volume integrations |

If/when backend rate limits are enabled, expect common headers such as:

- `X-RateLimit-Limit`
- `X-RateLimit-Remaining`
- `X-RateLimit-Reset`
- `Retry-After` (for `429` responses)

and HTTP `429 Too Many Requests` when exceeded.

### Recommended `429` handling

1. Read `Retry-After` header if present.
2. Pause new requests for that API key/session until reset.
3. Retry with exponential backoff + jitter.
4. Surface a user-friendly message if retries continue to fail.

---

## 8) Webhooks & Events

There are currently **no webhook endpoints/events** in this API.

- No registration API for webhook URLs.
- No signed delivery mechanism.
- No delivery retry/dead-letter behavior.

> **Note:** If webhooks are introduced, publish signature verification docs and replay protection guidance.

### Forward-looking webhook contract (for planning)

When webhooks are added, document each event with:

- event name (for example `product.created`, `chat.message.created`),
- delivery guarantees (at-least-once vs exactly-once),
- retry schedule and max attempts,
- JSON schema including optional fields,
- signature algorithm and verification code samples.

---

## 9) SDKs & Libraries

### Official SDKs

No official SDK package is currently published.

### Installation quick commands (community-standard clients)

```bash
# JavaScript / TypeScript
npm install axios

# Python
python -m pip install requests

# .NET (included in runtime)
dotnet add package Microsoft.Extensions.Http
```

### Recommended client libraries

- **JavaScript/TypeScript:** native `fetch`, Axios
- **Python:** `requests`
- **C#/.NET:** `HttpClient`

### Community libraries

No curated community SDK list is maintained yet.

---

## 10) Changelog & Versioning

### Versioning state

- Current API exposes versioned paths (`/api/v1/...`).
- No formal semantic API version number is embedded in routes.

### Change classification rules

- **Breaking:** existing client code must change (field removal/rename, enum rename, stricter validation, response shape removal).
- **Non-breaking:** additive only (new optional fields, new endpoints, new optional query parameters).

### Migration guide checklist for breaking changes

1. Publish impacted endpoints and payload diffs.
2. Provide before/after request and response examples.
3. Announce overlap period where `v1` and `v2` both run.
4. Publish cutoff date and rollback strategy.
5. Provide SDK/client update notes.

### Deprecation policy (recommended)

Because routes are not versioned yet, adopt:

1. announce deprecation in docs,
2. keep old behavior for at least one release window,
3. include migration examples,
4. remove only after communicated cutoff date.

### Breaking vs non-breaking examples

- **Breaking:** removing field, renaming enum value, changing status code.
- **Non-breaking:** adding optional response fields, adding new endpoints.

### Recent documentation changes

- Added comprehensive frontend integration guide with endpoint examples and operational guidance.

---

## 11) FAQs & Troubleshooting

> **Note:** For quick scanning, each answer includes the first action to try before escalating.

1. **Why do I get `email_not_verified` on login?**  
   Verify email first using verification token flow.

2. **Why does `/logout` succeed but old token fails later?**  
   That is expected: token `jti` is revoked.

3. **Why does product creation return `listing_limit_reached`?**  
   Seller hit active listing cap. Reduce active listings or raise cap via admin endpoint.

4. **Why do enum requests fail?**  
   Send enum names as strings exactly (`Used`, not `used` or numeric).

5. **Why does delete image return `204` even for missing ID?**  
   Endpoint is intentionally idempotent.

6. **Why can I call chat/report without auth?**  
   Current controllers do not enforce `[Authorize]`; secure upstream or update backend.

7. **Why no refresh token endpoint?**  
   Not currently implemented; user re-authenticates when access token expires.

8. **How do I debug validation quickly?**  
   Read `errorCode=validation_failed` and show field messages from `errors` map.

9. **What should I do on `401` globally?**  
   Clear local session and redirect to login.

10. **How do I contact support/status page?**  
   No dedicated support/status URLs are currently defined in repository docs.

---

## Appendix A: Full DTO contracts (TypeScript)

```ts
export type UserRole = 'User' | 'Seller' | 'Admin';
export type ProductCondition = 'New' | 'LikeNew' | 'Used' | 'HeavilyUsed' | 'Vintage';
export type ProductStatus = 'Active' | 'Sold' | 'Paused' | 'Archived';
export type ReportTargetType = 'Product' | 'Seller' | 'Message';

export interface AuthResponseDto {
  userId: string;
  email: string;
  role: UserRole;
  accessToken: string;
  expiresAtUtc: string;
}

export interface UserDto {
  id: string;
  email: string;
  role: UserRole;
  emailVerified: boolean;
  sellerRating: number;
  listingLimit: number;
  createdAt: string;
}

export interface ProductImageDto {
  id: string;
  productId: string;
  imageUrl: string;
  order: number;
}

export interface ProductDto {
  id: string;
  sellerUserId: string;
  title: string;
  description: string;
  priceText: string | null;
  price: number;
  condition: ProductCondition;
  status: ProductStatus;
  createdAt: string;
  updatedAt: string | null;
  images: ProductImageDto[];
}

export interface ChatRoomDto {
  id: string;
  productId: string;
  buyerId: string;
  sellerId: string;
  createdAt: string;
}

export interface MessageDto {
  id: string;
  chatRoomId: string;
  senderId: string;
  content: string;
  sentAt: string;
  createdAt: string;
}

export interface ReportDto {
  id: string;
  reporterId: string;
  targetType: ReportTargetType;
  targetId: string;
  reason: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
```
