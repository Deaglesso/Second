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


## 4) Anonymous visitors (no account, no login)

When someone opens the site without signing in, treat them as an **anonymous visitor** (no DB user record).

- They can access public read-only endpoints (for example product browsing).
- They cannot perform any authenticated actions (chat as a user, create listings, report as a user).
- If they choose to continue, prompt to register/login and then proceed as a normal `User`.

Implementation guidance:
- Keep public browsing endpoints explicitly marked as `[AllowAnonymous]` for clarity.
- Keep write/privileged endpoints protected via authorization policies.

## 5) First-time guests without profile

With role-based users, a first-time guest is simply a registered user with `role = User` and no seller-specific record.

- Registered guests can browse listings and start buyer flows immediately.
- Seller-only actions stay protected by the `SellerOnly` policy.
- When a guest wants to sell, call `POST /auth/become-seller` (authorized) to switch role to `Seller` and issue a fresh token containing updated role claims.

This keeps onboarding simple and avoids forcing profile creation before basic usage.


## Implemented API endpoints

- `POST /api/auth/logout` revokes the current JWT `jti` in Redis and clears `access_token` + `refresh_token` cookies.
- `POST /api/auth/request-email-verification` generates/sends a verification link when applicable.
- `POST /api/auth/verify-email` validates token and marks email verified.
- `POST /api/auth/forgot-password` sends reset link with enumeration-safe response.
- `POST /api/auth/reset-password` validates token and updates password hash securely.


## Operational note: Redis outages and deployment consistency

If logs still show `ConnectionMultiplexer.Connect(...)` failures from `DependencyInjection` after this fix,
that usually indicates an older deployment artifact is still running.

- Rebuild and redeploy the latest commit.
- Ensure old containers/processes are fully replaced.
- Confirm runtime logs reference lazy connect inside `RedisTokenRevocationService`.
