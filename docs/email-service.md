# Email service configuration

The API uses **MailKit** for production-ready SMTP delivery in verification and password-reset flows.

## Configuration

Configure the `Email` section in `appsettings.json` (or environment variables):

- `Email:Enabled`: set to `true` to send real emails through SMTP.
- `Email:FromAddress`: sender email address (must be a valid email format).
- `Email:FromName`: sender display name.
- `Email:SmtpHost`: SMTP host name.
- `Email:SmtpPort`: SMTP port (usually 587 for STARTTLS or 465 for SSL/TLS).
- `Email:UseSsl`: when `true`, the sender uses STARTTLS by default (or SSL-on-connect for port 465).
- `Email:UseDefaultCredentials`: use machine credentials when `true`.
- `Email:Username` / `Email:Password`: required when enabled and not using default credentials.
- `Email:TimeoutMilliseconds`: SMTP timeout (1,000-120,000 ms).

## Fallback behavior

When `Email:Enabled` is `false`, the app uses a logging email sender and does not attempt SMTP delivery.

## Production guidance

- Store `Email:Password` in a secret manager (never in source control).
- Use a dedicated transactional mailbox (for example `no-reply@yourdomain.com`).
- Ensure SPF, DKIM, and DMARC are configured for your sender domain.
- Prefer port 587 + STARTTLS unless your provider explicitly requires 465.
- Keep SMTP timeouts realistic (10-30 seconds is a common baseline).
