---
name: auth0-enterprise-sso
description: Auth0 enterprise SSO configuration for OpenID Connect and SAML connections. Use when setting up enterprise authentication, configuring identity providers, implementing SSO with external IdPs, or managing domain-based authentication flows.
---

# Auth0 Enterprise SSO

Configure enterprise identity providers for Single Sign-On using SAML and OpenID Connect protocols.

## When to Apply

- Setting up SAML or OIDC enterprise connections
- Configuring domain-based authentication routing
- Implementing IdP-initiated SSO flows
- Mapping IdP attributes to Auth0 user profiles

## Critical Rules

**Connection Strategy**: Use exact strategy names for connection types

```json
// WRONG - incorrect strategy
{
  "strategy": "saml",
  "name": "enterprise-connection"
}

// RIGHT - correct strategy names
{
  "strategy": "samlp",  // for SAML connections
  "name": "enterprise-saml"
}

{
  "strategy": "oidc",   // for OpenID Connect
  "name": "enterprise-oidc"
}
```

**Certificate Format**: SAML certificates must be Base64-encoded without headers

```json
// WRONG - includes PEM headers
{
  "signingCert": "-----BEGIN CERTIFICATE-----\nMIIC..."
}

// RIGHT - Base64 only
{
  "signingCert": "MIICXDCCAcWgAwIBAgIJAKS..."
}
```

## Key Patterns

### SAML Enterprise Connection

```json
{
  "strategy": "samlp",
  "name": "enterprise-saml",
  "options": {
    "signInEndpoint": "https://idp.company.com/saml/sso",
    "signOutEndpoint": "https://idp.company.com/saml/slo",
    "signingCert": "BASE64_CERTIFICATE",
    "signatureAlgorithm": "rsa-sha256",
    "digestAlgorithm": "sha256",
    "fieldsMap": {
      "email": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
      "user_id": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
      "name": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
    },
    "signSAMLRequest": true,
    "debug": true
  }
}
```

### OIDC Back-Channel Connection

```json
{
  "strategy": "oidc",
  "name": "enterprise-oidc",
  "options": {
    "type": "back_channel",
    "issuer": "https://idp.company.com",
    "authorization_endpoint": "https://idp.company.com/oauth/authorize",
    "client_id": "your_client_id",
    "client_secret": "your_client_secret",
    "scopes": "openid profile email"
  }
}
```

### OIDC Front-Channel Connection

```json
{
  "strategy": "oidc",
  "name": "enterprise-oidc-frontend",
  "options": {
    "type": "front_channel",
    "issuer": "https://idp.company.com",
    "authorization_endpoint": "https://idp.company.com/oauth/authorize",
    "token_endpoint": "https://idp.company.com/oauth/token",
    "client_id": "your_client_id",
    "scopes": "openid profile email"
  }
}
```

### Domain-Based Organization Discovery

```json
{
  "domain": "company.com",
  "use_for_organization_discovery": true,
  "verification_host": "_auth0-domain-verification.company.com",
  "verification_txt": "auth0-domain-verification=abcdef1234567890"
}
```

### Attribute Mapping Configuration

```json
{
  "fieldsMap": {
    "user_id": [
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn",
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
    ],
    "email": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
    "given_name": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
    "family_name": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
    "groups": "http://schemas.xmlsoap.org/claims/Group"
  }
}
```

## Common Mistakes

- **Missing debug flag**: Enable `"debug": true` in SAML connections to troubleshoot attribute mapping issues
- **Incorrect endpoint URLs**: Verify signInEndpoint and signOutEndpoint match IdP configuration exactly
- **Certificate format errors**: Remove PEM headers/footers from signing certificates
- **Scope mismatches**: Ensure OIDC scopes match what the IdP supports
- **Wrong connection type**: Use `back_channel` for server-side apps, `front_channel` for SPAs