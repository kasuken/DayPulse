# DayPulse custom-domain configuration

Status: Discovering existing hosting; awaiting target confirmation.

## Goal

Use `https://www.daypulse.me` as the production address and provide exact GoDaddy DNS records.

## Scope

- Inspect existing DNS and accessible Azure Static Web Apps.
- Identify and confirm the intended Azure resource and subscription.
- Configure domain validation and binding on the confirmed resource.
- Provide GoDaddy records without changing unrelated DNS or mail records.
- Verify domain validation and HTTPS after DNS propagation.

## Pending decisions

- Existing Azure resource, or approval to provision one if none is available.
- Subscription and region if provisioning is required.

## Safety

No paid resources, DNS changes, secret rotation, source push or deployment without the applicable approval. Do not guess a hostname or DNS validation token.
