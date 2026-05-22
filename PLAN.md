# Loom — Reference-Readiness Plan

Second-pass review after initial improvements landed. Frame: code is correct for a prototype; gaps below matter when this becomes a template that other teams copy for a larger multitenancy initiative.

## Critical (blocks real deployment)

### 1. HTTPS redirect ignores `X-Forwarded-Proto`

**File:** `src/Web.config:33-41`

Behind any TLS-terminating proxy (Azure App Service, AWS ALB, nginx, CloudFront), `{HTTPS}=OFF` on the backend. The rule will permanent-redirect every request, producing a loop. Critical because every team copying this hits it the moment they deploy behind real infrastructure.

**Fix:** Add a condition that skips the redirect when `HTTP_X_FORWARDED_PROTO=https`. Document the requirement in `README.md`.

### 2. No `customErrors` configured

**File:** `src/Web.config`

Default ASP.NET shows yellow stack-trace pages on remote clients in some configurations. Information disclosure.

**Fix:** `<system.web><customErrors mode="RemoteOnly" defaultRedirect="/context-invalid" /></system.web>`, or wire a dedicated error page.

### 3. `Application_Error` is empty

**File:** `src/Global.asax.cs:43-47`

Unhandled exceptions vanish. For a reference codebase, the minimum is: capture, log, present a tenant-aware error page.

**Fix:** Log the exception via the chosen logger and transfer to an error page.

### 4. Reserved-path collision risk

**Files:** `src/Web.config` (rewrite rules), `src/state/OrganizationCache.cs`

IIS rewrite captures `^([^/]+)` as a tenant. Nothing prevents a tenant slug from being `about`, `default`, `css`, `img`, `organizations`. Onboarding `about` shadows the about page. No shared reserved-name list. The path-rewrite rule only excludes `context-missing|context-invalid`.

**Fix:** Define `OrganizationCache.ReservedSlugs` (set). Enforce at: cache build (reject seed clashes), `IsValidOrganization` (reject reserved at lookup), and the IIS exclusion pattern.

## High (shapes downstream code patterns)

### 5. No test project

Zero coverage. Reference codebases that ship without tests teach teams it is optional.

**Fix:** Add an xUnit project. Minimum surface:

- `OrganizationResolver` — subdomain redirect, missing/invalid slug paths, already-resolved short-circuit.
- `OrganizationUrlResponseFilter` — regex positive/negative cases, multi-chunked `Write`, encoding boundaries.
- `OrganizationUrl.Resolve` — slug from context, explicit slug override, query-string handling.
- `OrganizationCache.Reload` — atomic swap, `Reloaded` event firing.

### 6. Page boilerplate repeats context construction

**Files:** `src/about.aspx.cs`, `src/default.aspx.cs`, `src/organizations/search.aspx.cs`

Every page does `_orgContext = new WebOrganizationContext(new HttpContextWrapper(Context));`. Three repetitions today, dozens once scaled.

**Fix:** Introduce `LoomPage : Page` base class with `protected IOrganizationContext OrgContext { get; }` initialized in `OnInit`. Pages inherit; the boilerplate disappears.

### 7. Slug validation rules diverge across the codebase

- IIS rewrite: `[^/]+` (anything but `/`)
- `LegacySubdomainPattern`: `[a-z0-9-]+`
- `OrganizationCache.IsValidOrganization`: dictionary key membership

A tenant `My_Org` passes IIS rewrite, passes cache lookup, fails subdomain match.

**Fix:** Pick one canonical slug format (e.g., `^[a-z0-9](?:[a-z0-9-]{0,30}[a-z0-9])?$`). Enforce at onboarding. Use the same pattern everywhere.

### 8. Resolver does not pre-cache settings into Items

**Files:** `src/web/OrganizationResolver.cs:122`, `src/web/WebOrganizationContext.cs`

`Resolve` stores slug only. `WebOrganizationContext.GetSettings()` looks up the cache lazily on first read. Race: if `OrganizationCache.Reload()` runs between resolve and read, the slug may no longer be valid, and `GetBySlug` throws mid-render.

**Fix:** Resolver stores both slug **and** resolved `OrganizationSettings` into `HttpContext.Items` at resolve time. Page consumers read from Items; they never re-query the cache.

### 9. `OrganizationCache.Reloaded` has fragile multicast semantics

**File:** `src/state/OrganizationCache.cs:29, 42`

A throwing subscriber silently aborts later subscribers. With one subscriber today (the filter), invisible. With more subscribers, derived state desyncs.

**Fix:** Replace `Reloaded?.Invoke()` with an explicit foreach over `GetInvocationList()`, wrapping each call in try/catch and surfacing failures to a logger.

## Medium

### 10. `OrganizationCache.GetBySlug` throws the wrong exception

**File:** `src/state/OrganizationCache.cs:58`

`ArgumentOutOfRangeException` for a lookup miss. Use `KeyNotFoundException` — semantically correct, distinguishable in catch blocks.

### 11. `ToTitleCase` is culture-sensitive

**Files:** `src/state/OrganizationCache.cs:101`, `src/default.aspx.cs:38`

`char.ToUpper`/`.ToLower` use the current culture. Turkish locale produces `İ` for `i`.

**Fix:** Use `ToUpperInvariant` / `ToLowerInvariant`.

### 12. Demo code bleeds into the reference home page

**File:** `src/default.aspx.cs`

Even with the config-driven gate, `ConfigureSubdomainDemoLink` is reference-template noise. Teams will copy `Default.aspx` as their template and inherit the demo wiring.

**Fix:** Move the demo to a separate sample/demo area, or strip it from the template and document separately.

## Lower

### 13. No skip list for non-tenant paths

**File:** `src/web/OrganizationResolver.cs`

`Resolve` runs in `BeginRequest` for every managed request. Future `/health`, `/api/*`, `/metrics` will hit tenant resolution and 302 to context-missing.

**Fix:** Define a convention (e.g., paths starting with `/_`, or a configurable allowlist) and short-circuit at the top of `Resolve`.

### 14. Health endpoint convention not defined

Path-based multitenancy makes this ambiguous. `/healthz` global vs. `/{slug}/healthz` per-tenant.

**Fix:** Decide and document in the README so the larger initiative starts coherent.

### 15. `IOrganizationContext` is anemic

**File:** `src/state/IOrganizationContext.cs`

`Slug` + `Settings`. Real-world tenant context typically also needs `IsActive`, feature flags, connection-string resolver, audit identity.

**Fix:** Either expand the interface or document the extension seam.

### 16. Stack noise: empty event handlers in Global.asax

**File:** `src/Global.asax.cs:37-62`

`Application_End`, `Session_Start`, `Session_End` are empty templates with placeholder comments. Reference code with cargo-cult placeholders invites cargo-culting in copies.

**Fix:** Remove or fill with intentional behavior.

### 17. `EnsureConfigured` uses `var _ = RemoteDomain`

**File:** `src/web/OrganizationResolver.cs:52`

That `_` is a variable named underscore, not a discard. Works, but `_ = RemoteDomain;` reads cleaner.

## Top-3 to land before this becomes the template

1. **#1 X-Forwarded-Proto** — anyone deploying behind a proxy hits this immediately.
2. **#6 `LoomPage` base class** — sets the pattern future pages will copy.
3. **#5 tests + #8 settings pre-cache** — pair naturally; tests will surface the race.
