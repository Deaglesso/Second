# Second API — Complete Frontend Integration Documentation

This document is a contract-style API reference for the current backend implementation so a frontend can be built without reading backend code.

---

## 1) API runtime and base URL

- **Base URL (local Docker default):** `http://localhost:8080`
- **API prefix:** `/api`
- **Swagger (Development environment only):** `/swagger`
- **Static frontend demo files are hosted from API root** (not required for your own frontend).

Reference routes in this doc are written as full relative paths (example: `/api/auth/login`).

---

## 2) Authentication and authorization model

### 2.1 Auth type

- The API uses **JWT Bearer tokens** in the `Authorization` header.
- Header format:

```http
Authorization: Bearer <accessToken>
```

### 2.2 Token claims included

Access tokens include:

- `sub` = user id
- `jti` = token id (used for revocation)
- `email`
- `nameidentifier` (user id)
- `email` (ClaimTypes.Email)
- `role` (`User`, `Seller`, `Admin`)

### 2.3 Expiration

- Token expiry is controlled by `Jwt:ExpiresInMinutes` (default 60 minutes).
- Login/register/become-seller responses include `expiresAtUtc`.

### 2.4 Revocation behavior

- `POST /api/auth/logout` revokes the current token `jti` server-side (Redis-backed revocation).
- A revoked token fails auth on subsequent requests.

### 2.5 Role policy and role checks

- Policy `SellerOnly` allows roles: `Seller` or `Admin`.
- Admin-only routes are protected via `[Authorize(Roles = "Admin")]`.

---

## 3) CORS

Current CORS policy allows frontend origin:

- `http://localhost:3000`

with any method and any header.

---

## 4) Content type, serialization, and enum format

### 4.1 JSON conventions

- Send request body as JSON.
- Responses are JSON.
- Use header:

```http
Content-Type: application/json
```

### 4.2 Enum serialization

Enums are serialized and accepted as **strings** (because `JsonStringEnumConverter` is enabled).

Allowed enum values:

- `UserRole`: `User`, `Seller`, `Admin`
- `ProductCondition`: `New`, `LikeNew`, `Used`, `HeavilyUsed`, `Vintage`
- `ReportTargetType`: `Product`, `Seller`, `Message`

---

## 5) Standard error response contract

Errors are returned as RFC7807-like `ProblemDetails` JSON from global exception middleware.

### 5.1 Error payload shape

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/auth/register",
  "traceId": "00-...",
  "timestampUtc": "2026-02-18T14:21:22.1234567Z",
  "errorCode": "validation_failed",
  "errors": {
    "Email": ["'Email' is not a valid email address."],
    "Password": ["Password must contain at least one uppercase letter."]
  }
}
```

### 5.2 Common status codes

- `200` success
- `201` created
- `204` no content
- `400` bad request / validation / malformed JSON
- `401` unauthorized
- `403` forbidden
- `404` not found
- `409` conflict
- `499` request canceled by client
- `500` server error

### 5.3 Common error codes used by business logic

- `validation_failed`
- `invalid_json`
- `invalid_credentials`
- `email_not_verified`
- `token_metadata_missing`
- `invalid_user_token`
- `user_not_found`
- `seller_not_found`
- `listing_limit_reached`
- `product_not_found`
- `chat_room_not_found`
- `invalid_email_verification_token`
- `invalid_reset_token`
- `user_not_seller`
- `invalid_pagination_parameters`

---

## 6) Pagination contract

Endpoints that return `PagedResult<T>` accept query parameters:

- `pageNumber` (default `1`, must be `>= 1`)
- `pageSize` (default `20`, allowed range `1..100`)

Response shape:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 0,
  "totalPages": 0
}
```

If invalid pagination values are sent, API returns `400` with `errorCode: "invalid_pagination_parameters"`.

---

## 7) Data models (request + response)

> Use these models exactly in your frontend API client typing.

### 7.1 AuthResponseDto

```ts
interface AuthResponseDto {
  userId: string;        // guid
  email: string;
  role: 'User' | 'Seller' | 'Admin';
  accessToken: string;
  expiresAtUtc: string;  // ISO datetime
}
```

### 7.2 UserDto

```ts
interface UserDto {
  id: string;            // guid
  email: string;
  role: 'User' | 'Seller' | 'Admin';
  emailVerified: boolean;
  sellerRating: number;  // decimal
  listingLimit: number;
  createdAt: string;     // ISO datetime
}
```

### 7.3 ProductImageDto

```ts
interface ProductImageDto {
  id: string;            // guid
  productId: string;     // guid
  imageUrl: string;
  order: number;
}
```

### 7.4 ProductDto

```ts
interface ProductDto {
  id: string;            // guid
  sellerUserId: string;  // guid
  title: string;
  description: string;
  priceText?: string | null;
  condition: 'New' | 'LikeNew' | 'Used' | 'HeavilyUsed' | 'Vintage';
  isActive: boolean;
  createdAt: string;     // ISO datetime
  images: ProductImageDto[];
}
```

### 7.5 ChatRoomDto

```ts
interface ChatRoomDto {
  id: string;            // guid
  productId: string;     // guid
  buyerId: string;       // guid
  sellerId: string;      // guid
  createdAt: string;     // ISO datetime
}
```

### 7.6 MessageDto

```ts
interface MessageDto {
  id: string;            // guid
  chatRoomId: string;    // guid
  senderId: string;      // guid
  content: string;
  sentAt: string;        // ISO datetime
  createdAt: string;     // ISO datetime
}
```

### 7.7 ReportDto

```ts
interface ReportDto {
  id: string;            // guid
  reporterId: string;    // guid
  targetType: 'Product' | 'Seller' | 'Message';
  targetId: string;      // guid
  reason: string;
  createdAt: string;     // ISO datetime
}
```

---

## 8) Validation rules for request DTOs

### 8.1 RegisterUserRequest

```json
{ "email": "user@example.com", "password": "StrongPass1" }
```

Rules:
- `email`: required, valid email format, max length 320
- `password`: required, min length 8, must contain uppercase, lowercase, and number

### 8.2 LoginRequest

```json
{ "email": "user@example.com", "password": "StrongPass1" }
```

Rules:
- `email`: required, valid email
- `password`: required

### 8.3 RequestEmailVerificationRequest

```json
{ "email": "user@example.com" }
```

Rules:
- `email`: required, valid email, max length 320

### 8.4 VerifyEmailRequest

```json
{ "token": "opaque-token" }
```

Rules:
- `token`: required, max length 512

### 8.5 ForgotPasswordRequest

```json
{ "email": "user@example.com" }
```

Rules:
- `email`: required, valid email, max length 320

### 8.6 ResetPasswordRequest

```json
{ "token": "opaque-token", "newPassword": "NewStrong1" }
```

Rules:
- `token`: required, max length 512
- `newPassword`: required, min length 8, uppercase + lowercase + number required

### 8.7 CreateProductRequest

```json
{
  "sellerUserId": "00000000-0000-0000-0000-000000000000",
  "title": "MacBook Pro 14",
  "description": "Like new, 16GB RAM",
  "priceText": "$1500",
  "condition": "LikeNew",
  "imageUrls": ["https://.../1.jpg", "https://.../2.jpg"]
}
```

Rules:
- `sellerUserId`: required GUID
- `title`: required, max length 200
- `description`: required, max length 2000
- `priceText`: optional, max length 100
- `condition`: must be valid enum
- each `imageUrls[]`: required, max length 500

### 8.8 UpdateProductRequest

```json
{
  "title": "Updated title",
  "description": "Updated description",
  "priceText": "$1400",
  "condition": "Used",
  "isActive": true
}
```

Rules:
- path provides `productId`; body has same model property set internally by API
- `title`: required, max length 200
- `description`: required, max length 2000
- `priceText`: optional, max length 100
- `condition`: valid enum

### 8.9 AddProductImageRequest

```json
{ "imageUrl": "https://.../3.jpg", "order": 2 }
```

Rules:
- path provides `productId`; body model property set internally by API
- `imageUrl`: required, max length 500
- `order`: must be >= 0

### 8.10 StartChatRequest

```json
{
  "productId": "00000000-0000-0000-0000-000000000000",
  "buyerId": "00000000-0000-0000-0000-000000000000",
  "sellerId": "00000000-0000-0000-0000-000000000000"
}
```

Rules:
- all three GUIDs required

### 8.11 SendMessageRequest

```json
{
  "senderId": "00000000-0000-0000-0000-000000000000",
  "content": "Is this still available?"
}
```

Rules:
- path provides `chatRoomId`; body model property set internally by API
- `senderId`: required GUID
- `content`: required, max length 2000

### 8.12 CreateReportRequest

```json
{
  "reporterId": "00000000-0000-0000-0000-000000000000",
  "targetType": "Product",
  "targetId": "00000000-0000-0000-0000-000000000000",
  "reason": "Fraudulent listing"
}
```

Rules:
- `reporterId`: required GUID
- `targetType`: valid enum
- `targetId`: required GUID
- `reason`: required, max length 1000

### 8.13 UpdateSellerListingLimitRequest

```json
{ "listingLimit": 25 }
```

Rules:
- `listingLimit`: must be >= 0

---

## 9) Endpoint reference by controller

## 9.1 Auth endpoints (`/api/auth`)

### POST `/api/auth/register`

- **Auth required:** No
- **Body:** `RegisterUserRequest`
- **Success:** `200 OK` + `AuthResponseDto`
- **Behavior details:**
  - Email is normalized to lowercase + trimmed.
  - If email already exists (including soft-deleted lookup path), returns `409 email_already_exists`.
  - User is created with role `User`, `EmailVerified=false`, `SellerRating=0`, `ListingLimit=10`.
  - Verification email flow is triggered.

### POST `/api/auth/login`

- **Auth required:** No
- **Body:** `LoginRequest`
- **Success:** `200 OK` + `AuthResponseDto`
- **Errors:**
  - `401 invalid_credentials` for unknown email / wrong password
  - `401 email_not_verified` if user has not verified email

### POST `/api/auth/logout`

- **Auth required:** Yes (`Bearer`)
- **Body:** none
- **Success:** `200 OK`

```json
{ "message": "Logged out successfully." }
```

- **Behavior:**
  - Reads `jti` and `exp` from current JWT.
  - Revokes `jti` until token expiry.
  - Clears cookies named `access_token` and `refresh_token` (even though auth is header-based).

### POST `/api/auth/request-email-verification`

- **Auth required:** No
- **Body:** `RequestEmailVerificationRequest`
- **Success:** always `200 OK`

```json
{ "message": "If the account exists and is unverified, a verification email has been sent." }
```

- **Behavior:** Anti-enumeration style response (same generic message).

### POST `/api/auth/verify-email`

- **Auth required:** No
- **Body:** `VerifyEmailRequest`
- **Success:** `200 OK`

```json
{ "message": "Email verified successfully." }
```

- **Error:** `400 invalid_email_verification_token` if token invalid/expired

### POST `/api/auth/forgot-password`

- **Auth required:** No
- **Body:** `ForgotPasswordRequest`
- **Success:** always `200 OK`

```json
{ "message": "If an account exists, a reset link has been sent." }
```

### POST `/api/auth/reset-password`

- **Auth required:** No
- **Body:** `ResetPasswordRequest`
- **Success:** `200 OK`

```json
{ "message": "Password reset successfully." }
```

- **Errors:** `400 invalid_reset_token` when token invalid/expired
- **Behavior:** also clears `access_token`/`refresh_token` cookies.

### POST `/api/auth/become-seller`

- **Auth required:** Yes (`Bearer`)
- **Body:** none
- **Success:** `200 OK` + `AuthResponseDto`
- **Behavior:**
  - If current role is `User`, it changes to `Seller`.
  - Returns a fresh JWT reflecting current role.

### GET `/api/auth/me`

- **Auth required:** Yes (`Bearer`)
- **Body:** none
- **Success:** `200 OK` + `UserDto`
- **Errors:**
  - `401 invalid_user_token` if auth claim parsing fails
  - `404 user_not_found` if user no longer exists

---

## 9.2 Product endpoints (`/api/products`)

### POST `/api/products`

- **Auth required:** Yes (`SellerOnly` => Seller/Admin)
- **Body:** `CreateProductRequest`
- **Success:** `201 Created` + `ProductDto`
- **Location header:** points to `GET /api/products/{productId}`
- **Business rules:**
  - `sellerUserId` must exist and be seller-capable.
  - Enforces active listing capacity against seller `ListingLimit`.
  - New product is always created with `isActive = true`.
  - Images are saved with order indices from array order (`0,1,2...`).

### GET `/api/products/{productId}`

- **Auth required:** No
- **Success:** `200 OK` + `ProductDto`
- **Error:** `404 product_not_found`

### GET `/api/products/active?pageNumber=1&pageSize=20`

- **Auth required:** No
- **Success:** `200 OK` + `PagedResult<ProductDto>`
- **Validation:** pagination rules apply

### GET `/api/products/by-seller/{sellerUserId}?pageNumber=1&pageSize=20`

- **Auth required:** No explicit `[Authorize]` (currently publicly accessible)
- **Success:** `200 OK` + `PagedResult<ProductDto>`

### PUT `/api/products/{productId}`

- **Auth required:** Yes (`SellerOnly`)
- **Body:** `UpdateProductRequest` (path id overrides body `productId`)
- **Success:** `200 OK` + `ProductDto`
- **Errors:**
  - `404 product_not_found`
  - `409 listing_limit_reached` when switching/keeping active exceeds seller capacity

### POST `/api/products/{productId}/images`

- **Auth required:** Yes (`SellerOnly`)
- **Body:** `AddProductImageRequest` (path id overrides body `productId`)
- **Success:** `200 OK` + `ProductImageDto`
- **Error:** `404 product_not_found`

### DELETE `/api/products/images/{imageId}`

- **Auth required:** Yes (`SellerOnly`)
- **Body:** none
- **Success:** `204 No Content`
- **Behavior:** idempotent; if image does not exist, still returns `204`.

---

## 9.3 Chat endpoints (`/api/chats`)

> Note: Chat controller has no `[Authorize]` currently, so endpoints are publicly callable unless gateway/auth layer is added externally.

### POST `/api/chats`

- **Auth required:** No attribute-level auth
- **Body:** `StartChatRequest`
- **Success:** `201 Created` + `ChatRoomDto`
- **Behavior:**
  - If same `(productId,buyerId,sellerId)` room already exists, returns existing room.
  - Otherwise creates a new room.

### GET `/api/chats/{chatRoomId}`

- **Auth required:** No attribute-level auth
- **Success:** `200 OK` + `ChatRoomDto`
- **Error:** `404 chat_room_not_found`

### GET `/api/chats/by-user/{userId}?pageNumber=1&pageSize=20`

- **Auth required:** No attribute-level auth
- **Success:** `200 OK` + `PagedResult<ChatRoomDto>`

### POST `/api/chats/{chatRoomId}/messages`

- **Auth required:** No attribute-level auth
- **Body:** `SendMessageRequest` (path id overrides body `chatRoomId`)
- **Success:** `200 OK` + `MessageDto`
- **Error:** `404 chat_room_not_found`
- **Behavior:** `sentAt` set server-side (`UtcNow`).

### GET `/api/chats/{chatRoomId}/messages?pageNumber=1&pageSize=20`

- **Auth required:** No attribute-level auth
- **Success:** `200 OK` + `PagedResult<MessageDto>`

---

## 9.4 Report endpoints (`/api/reports`)

> Note: Report controller also has no `[Authorize]` in current implementation.

### POST `/api/reports`

- **Auth required:** No attribute-level auth
- **Body:** `CreateReportRequest`
- **Success:** `200 OK` + `ReportDto`

### GET `/api/reports/by-target?targetType=Product&targetId=<guid>&pageNumber=1&pageSize=20`

- **Auth required:** No attribute-level auth
- **Success:** `200 OK` + `PagedResult<ReportDto>`

### GET `/api/reports/by-reporter/{reporterId}?pageNumber=1&pageSize=20`

- **Auth required:** No attribute-level auth
- **Success:** `200 OK` + `PagedResult<ReportDto>`

---

## 9.5 Admin seller management endpoints (`/api/admin/sellers`)

### PATCH `/api/admin/sellers/{sellerUserId}/listing-limit`

- **Auth required:** Yes, role `Admin`
- **Body:** `UpdateSellerListingLimitRequest`
- **Success:** `200 OK` + `UserDto`
- **Errors:**
  - `404 seller_not_found`
  - `400 user_not_seller` (when target user is not Seller/Admin)

---

## 10) Frontend implementation checklist (practical)

1. Build a typed API layer using models in section 7.
2. Centralize error parsing:
   - read `status`, `title`, `detail`, `errorCode`, optional `errors`.
3. Inject bearer token on protected routes only.
4. Handle auth failures:
   - clear session on `401`.
5. For forms, surface per-field FluentValidation messages from `errors` dictionary.
6. Use enum strings exactly as documented.
7. For paged lists, always pass `pageNumber` + `pageSize`; keep UI page size <= 100.
8. For register/login/role upgrade, replace stored token with returned `accessToken`.
9. Expect generic success response for email verification and forgot-password request flows.
10. If you require strict privacy/ownership checks for chats/reports/seller data, add backend authorization rules before production use.

---

## 11) Minimal end-to-end call sequence examples

### 11.1 New user onboarding

1. `POST /api/auth/register`
2. `POST /api/auth/request-email-verification` (if needed)
3. `POST /api/auth/verify-email`
4. `POST /api/auth/login`
5. `GET /api/auth/me`

### 11.2 Seller flow

1. User login
2. `POST /api/auth/become-seller`
3. `POST /api/products`
4. `POST /api/products/{id}/images`
5. `GET /api/products/by-seller/{sellerId}`

### 11.3 Buyer + chat flow

1. `GET /api/products/active`
2. `POST /api/chats`
3. `POST /api/chats/{chatRoomId}/messages`
4. `GET /api/chats/{chatRoomId}/messages`

### 11.4 Report flow

1. `POST /api/reports`
2. `GET /api/reports/by-target`

