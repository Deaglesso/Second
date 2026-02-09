# Email service configuration

The API now supports real SMTP delivery for verification and password-reset flows.

## Configuration

Configure the `Email` section in `appsettings.json` (or environment variables):

- `Email:Enabled`: set to `true` to send real emails through SMTP.
- `Email:FromAddress`: sender email address.
- `Email:FromName`: sender display name.
- `Email:SmtpHost`: SMTP host name.
- `Email:SmtpPort`: SMTP port (usually 587 for STARTTLS or 465 for SSL).
- `Email:UseSsl`: whether SSL/TLS is enabled.
- `Email:UseDefaultCredentials`: use machine credentials when `true`.
- `Email:Username` / `Email:Password`: required when enabled and not using default credentials.
- `Email:TimeoutMilliseconds`: SMTP timeout.

## Fallback behavior

When `Email:Enabled` is `false`, the app uses a logging email sender and does not attempt SMTP delivery.

## Production guidance

- Store `Email:Password` in a secret manager (never in source control).
- Use a dedicated transactional mailbox (for example `no-reply@yourdomain.com`).
- Ensure SPF, DKIM, and DMARC are configured for your sender domain.
