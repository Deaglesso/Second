# Authentication workflows and security design

## 1) Logout: secure session/token invalidation

A secure logout flow is **two-sided**:

- **Client-side logout**
  - Immediately remove in-memory access tokens.
  - Delete refresh token cookie (if cookie-based) with `HttpOnly`, `Secure`, `SameSite=Lax/Strict`.
  - Clear auth state and cached user profile.
- **Server-side logout**
  - Invalidate the current refresh token record in storage (revoke + timestamp).
  - Rotate a per-user security version (or token version) to invalidate outstanding tokens.
  - Persist audit metadata (IP, user-agent, revoked-at).

Recommended behavior:
- `POST /auth/logout` revokes only the current session.
- `POST /auth/logout-all` revokes all refresh tokens for user.

For JWT access tokens, do not rely on client deletion only. Pair short-lived access tokens with server-tracked refresh token revocation.

## 2) Email verification workflow

Secure flow:

1. User registers with `email_verified = false`.
2. Server generates a random token with CSPRNG (at least 32 bytes entropy).
3. Store only a **hash** of token + expiry in DB (`verification_token_hash`, `verification_expires_at`).
4. Send verification link containing raw token once: `/auth/verify-email?token=...`.
5. On callback:
   - hash provided token,
   - constant-time compare with stored hash,
   - verify not expired and not already used,
   - set `email_verified = true`, clear token fields.

Security considerations:
- single-use token,
- short expiry (15–60 mins typical),
- rate-limit verification attempts,
- log verification events.

## 3) Forgot password flow (enumeration safe)

Use a request → reset → update sequence:

1. **Request reset** (`POST /auth/forgot-password`)
   - Always return the same generic response: `If an account exists, a reset link has been sent.`
   - If account exists: create CSPRNG token, store token hash + expiry + not-used, send email.
2. **Validate and reset** (`POST /auth/reset-password`)
   - Validate token hash, expiry, and one-time use.
   - Enforce strong new password policy.
   - Update password hash using secure hasher.
   - Mark token used/revoked.
   - Revoke all active sessions (security-version bump or refresh-token revocation).

Anti-enumeration controls:
- identical response body and status for existing/non-existing emails,
- consistent response timing (or bounded jitter),
- aggressive rate limits by IP + email key.


## 4) Frontend integration contract (current API)

Base auth route in this project is `/api/auth`.

Public endpoints:
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/request-email-verification`
- `POST /api/auth/verify-email`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`

Authorized endpoints (Bearer token required):
- `GET /api/auth/me`
- `POST /api/auth/logout`
- `POST /api/auth/become-seller`

Important login behavior:
- Login now requires verified email. If credentials are valid but email is not verified, API returns 401 with code `email_not_verified`.

## 5) Frontend test sections for all auth functions

Use these sections in your client QA page, Postman collection, or Cypress/Playwright test suite.

### Section A — Register

**Goal**: user can register and receive token payload.

1. Submit `POST /api/auth/register` with email + password.
2. Expect 200 and response containing:
   - `userId`
   - `email`
   - `role`
   - `accessToken`
   - `expiresAtUtc`
3. Save `accessToken` for temporary authenticated calls.

### Section B — Request email verification

**Goal**: verification email flow is triggerable.

1. Submit `POST /api/auth/request-email-verification` with registered email.
2. Expect generic success message regardless of account state.
3. Verify your email inbox/log sink received the verification link.

### Section C — Verify email

**Goal**: verification token enables account verification.

1. Extract token from verification link.
2. Submit `POST /api/auth/verify-email` with token.
3. Expect 200 with success message.
4. Re-submit same token and expect failure (token should be invalid/expired/single use).

### Section D — Login (verified + unverified cases)

**Goal**: enforce policy and validate token issuance.

1. Attempt `POST /api/auth/login` for an **unverified** account.
   - Expect 401 with error code `email_not_verified`.
2. Attempt login for a **verified** account.
   - Expect 200 and token payload.
3. Decode JWT in client test utilities and assert claims exist (`sub`, `jti`, email, role).

### Section E — Me (current user profile)

**Goal**: authenticated identity works end-to-end.

1. Call `GET /api/auth/me` with header:
   - `Authorization: Bearer <accessToken>`
2. Expect 200 and user profile with matching email/id/role.
3. Call without header and expect 401.

### Section F — Logout and token revocation

**Goal**: logged-out token is no longer valid.

1. Call `POST /api/auth/logout` with valid Bearer token.
2. Expect 200 `Logged out successfully`.
3. Reuse same token on `GET /api/auth/me`.
   - Expect 401 (revoked token).

### Section G — Forgot password

**Goal**: password reset request is enumeration-safe.

1. Call `POST /api/auth/forgot-password` with existing email.
2. Call again with non-existing email.
3. Ensure both responses are the same generic success message.
4. Verify reset email appears for existing account only.

### Section H — Reset password

**Goal**: reset token updates password and invalidates old secret.

1. Submit `POST /api/auth/reset-password` with token + new password.
2. Expect 200.
3. Login with old password should fail.
4. Login with new password should pass.

### Section I — Become seller (role upgrade)

**Goal**: role change endpoint and role claim refresh.

1. Login as normal user and call `POST /api/auth/become-seller` with token.
2. Expect 200 and new token payload.
3. Call `GET /api/auth/me` with new token and verify role changed to seller.

## 6) Deployment readiness checklist (frontend)

Mark deploy-ready only when all checks pass in the target environment:

- [ ] Frontend stores token securely (prefer in-memory; avoid long-lived localStorage if possible).
- [ ] Every protected request includes `Authorization: Bearer <token>`.
- [ ] `401` handler clears auth state and redirects to login.
- [ ] Verified-account login passes; unverified-account login fails with `email_not_verified`.
- [ ] Logout immediately blocks old token usage.
- [ ] Forgot/reset password flows complete using real email provider configuration.
- [ ] Role change (`become-seller`) updates both backend authorization result and frontend UI permissions.
- [ ] Production CORS origin is configured correctly for your frontend domain.
