# Loom — Improvement Plan

Recommendations from code review of the Web Forms multitenancy prototype. Ordered by impact within each category.

## Bugs

### 1. Response filter attaches for `empty` slug

**File:** `src/Global.asax.cs:23-32`

`isMissingSlug` is `false` when the resolved slug equals `OrganizationCache.EmptySlug` (`"empty"`). The filter then rewrites links on error pages — for example, `href="/about"` becomes `href="/empty/about"`. Error pages should not be rewritten.

**Fix:** Skip the filter when `slug == OrganizationCache.EmptySlug`.

### 2. `OrganizationResolver.RemoteDomain` throws on missing config

**File:** `src/web/OrganizationResolver.cs:18`

`ConfigurationManager.AppSettings["Loom.RemoteDomain"]` returns `null` when the key is missing. `.ToLower()` then throws `NullReferenceException` during static initialization, which crashes the app pool on every request.

**Fix:** Validate the setting on startup with a clear error, or fall back safely. Use `ToLowerInvariant()` for culture safety.

### 3. `IsValidOrganization` inconsistent with cache

**File:** `src/state/OrganizationCache.cs:71-74`

`IsValidOrganization` scans the `Slugs` array, but `Organizations` dictionary also holds `empty`. Two sources of truth. The check excludes `empty` correctly today but only by accident of how `Slugs` was initialized.

**Fix:** `return Organizations.ContainsKey(slug) && slug != EmptySlug;`

### 4. `Response.Redirect(url, true)` throws ThreadAbortException

**File:** `src/web/OrganizationResolver.cs:64, 71, 81`

`endResponse=true` raises `ThreadAbortException`. Costly and noisy in logs.

**Fix:** `Response.Redirect(url, false); HttpContext.Current.ApplicationInstance.CompleteRequest();`

### 5. Hardcoded UTF-8 in response filter

**File:** `src/state/OrganizationUrlResponseFilter.cs:56, 61`

Ignores `Response.ContentEncoding`. Breaks non-UTF-8 pages.

**Fix:** Capture `Response.ContentEncoding` in the constructor; use it for both decode and encode.

## Security

### 6. XSS pattern in code-behind

**Files:** `src/about.aspx.cs:16`, `src/default.aspx.cs:17`

`InnerHtml = "About the " + span` concatenates un-encoded cache values into HTML. Today's cache values are safe, but the pattern invites injection when the cache loads from a database.

**Fix:** `HttpUtility.HtmlEncode(name)` and validate `color` against a whitelist (or use a CSS class instead of inline style).

### 7. No HTTPS enforcement

**File:** `src/Web.config`

No HSTS header, no HTTP-to-HTTPS redirect. Acceptable for the prototype, mandatory for production.

**Fix:** Add a URL Rewrite rule to redirect HTTP to HTTPS and an `<add name="Strict-Transport-Security" ...>` custom header.

### 8. Hardcoded demo data in `Default.aspx.cs`

**File:** `src/default.aspx.cs:23`

The string `"indigo."` is baked into production code. Demo data should not live in the request path.

**Fix:** Move to app settings or remove. Treat as a demo-only artifact.

## Design

### 9. `OrganizationResolver` is not marked `static`

**File:** `src/web/OrganizationResolver.cs:10`

All members are static. Mark the class `static` to communicate intent and prevent instantiation.

### 10. `OrganizationSettings` is mutable

**File:** `src/state/OrganizationSettings.cs`

Cached entries can be mutated by any caller. Make immutable via init-only setters or a constructor.

### 11. `SlugVariable` const reused for two distinct keys

**File:** `src/web/OrganizationResolver.cs:12`

Same string identifies both the IIS server variable and the `HttpContext.Items` key. Split into `SlugServerVariable` and `SlugItemKey`.

### 12. Filter rebuilds slug list and regex per request

**File:** `src/state/OrganizationUrlResponseFilter.cs:21-34`

`OrganizationCache.GetAll()`, `Concat`, and `new Regex(...)` run for every response. Move to `static readonly` fields (with a one-time build).

### 13. `OrganizationCache` has no refresh path

**File:** `src/state/OrganizationCache.cs`

README notes that production will load from a database. The current static initialization requires a process restart to pick up new tenants. Plan a `Reload()` method or a `Lazy<ConcurrentDictionary>` with a TTL.

### 14. `HttpContext.Current` accessed directly in services

**Files:** `src/web/WebOrganizationContext.cs`, `src/web/OrganizationUrl.cs`

Couples the helpers to the runtime, blocks unit tests. Inject `HttpContextBase` or pass context explicitly at the call site.

## Performance / correctness

### 15. Response filter buffers the entire HTML body

**File:** `src/state/OrganizationUrlResponseFilter.cs`

Fine for small pages, broken for streaming or large responses. Either document the limit or implement chunked rewrite that handles attribute splits across buffer boundaries.

### 16. `Stream.Close` override is deprecated; unused members on a write-only stream

**File:** `src/state/OrganizationUrlResponseFilter.cs:47`

Override `Dispose(bool)` instead. Drop `Length`, `Position`, `Seek`, `SetLength` — the filter is write-only.

### 17. `Debug.WriteLine` on every request

**File:** `src/Global.asax.cs:16`

Noise. Remove or wrap in `#if DEBUG` only when actively debugging.

### 18. `Slugs.Contains(slug)` linear scan

**File:** `src/state/OrganizationCache.cs:73`

Trivial at seven slugs, O(n) at production scale. Use `HashSet<string>` or check the dictionary directly (see #3).

## Build / project

### 19. Old-style `packages.config` + non-SDK csproj

**Files:** `src/Loom.csproj`, `src/packages.config`

Migrate to SDK-style project with `PackageReference`. Still targets `net48`, less ceremony.

### 20. `bin/` and `obj/` historically tracked

**Files:** `src/bin/`, `src/obj/`

DLLs and compiler caches are in the repo despite being gitignored now. Remove with `git rm -r --cached src/bin src/obj` and commit.

### 21. No test project

`IOrganizationContext` exists for testability but has no tests against it. Add an xUnit project covering:

- `OrganizationResolver` subdomain redirect logic
- `OrganizationUrlResponseFilter` regex (positive and negative cases, including `empty` slug)
- `OrganizationUrl.Resolve` edge cases

## Execution order

1. Fix #1 — silent rewrite bug on error pages
2. Fix #2 — config validation
3. Fix #4 — redirect mechanics
4. #20 — strip tracked `bin/`/`obj/`
5. #10, #9 — immutability and `static` markers
6. Remaining items as scope allows
