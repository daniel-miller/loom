# Prompt template: a new app on the Loom registry

Copy the block below into the first session of a new app. Replace the `<...>` placeholders
first. For an existing app moving onto the registry, use one of the adoption prompts as a model
instead (`bump/tmp/adopt-design-registry.md` is the simplest); those start with a diff of what
the app already has.

## Placeholders

- `<app>`: the app's short name, lowercase, used for the palette (`palette-<app>`).
- `<repo>`: the new repo's path, such as `C:\base\repo\<org>\<name>`.
- `<accent>`: the brand accent colour, as hex or oklch.
- `<spelling>`: `Canadian`, or `US` for an `openscorm/*` repo.

## The prompt

````markdown
Set up the web UI for <app> in `<repo>` on the shared Loom registry in
`C:\base\repo\daniel-miller\loom`, so it starts from the same primitives, tokens and icons as
the other apps instead of copying them.

## Background

Loom is the single source of the shadcn-style primitives behind every app I build. Read its
`README.md` first. It publishes a shadcn registry namespaced `@loom` (`registry.json`, built
to `public/r/` by `npm run build:registry`): primitives in `registry/ui/`, page blocks in
`registry/blocks/`, `cn` and `dates` helpers in `registry/lib/`, shared tokens in
`tokens/base.css`, and one palette per app in `palettes/<app>.css`. Components reference
semantic tokens only (`bg-primary`, `text-muted-foreground`); icons are Font Awesome Pro sharp.

## Before you start

1. **Palette.** If `loom/palettes/<app>.css` doesn't exist, add it in the Loom repo first:
   copy the closest existing palette, set the accent to `<accent>` in light and dark, and add a
   `palette-<app>` item to `registry.json` modelled on the others. That's a separate commit in
   the Loom repo; show me the palette before committing it. Once it's merged, publish the
   registry with `./tools/publish-registry.ps1` so the CLI can install the new palette.

## Steps

1. **Plan first.** Propose the scaffold and the list of registry items this app needs on day
   one, and wait for my approval. Start small: add primitives when a screen needs them, not all
   of them up front.
2. **Scaffold** `web/` at the repo root with my default stack (Vite, React, TypeScript strict,
   Tailwind v4, TanStack Query, ESLint, Prettier, Vitest, a `typecheck` script), following
   `cmds-app/platform/web` as the live pattern.
3. **Font Awesome.** Add an `.npmrc` like the Loom repo's (it reads
   `FONTAWESOME_NPM_AUTH_TOKEN`), install `@fortawesome/fontawesome-pro`, and import
   `fontawesome.min.css`, `sharp-regular.min.css` and `sharp-solid.min.css`. Wire the token into
   `web-ci.yml`. No `lucide-react`.
4. **components.json.** Run `npx shadcn init`, then set the aliases to `@/components/ui` and
   `@/lib/cn`, remove `iconLibrary` (the CLI has no Font Awesome option, and the registry
   sources carry their own icons), and add the registry:
   `"registries": { "@loom": "https://loom.danielmiller.ca/r/{name}.json" }`.
5. **Install** `@loom/palette-<app>` plus the approved primitives and blocks with
   `npx shadcn add`.
6. **Styles.** `src/styles/globals.css` imports Tailwind, then `./loom/base.css`, then
   `./loom/palettes/<app>.css`, as the Loom README shows. App-level rules go after them;
   no inline token block.
7. **Root providers.** Wrap the app in `TooltipProvider` (`delayDuration={200}`) and mount
   `Toaster` once, passing the resolved theme. Dark mode is the `dark` class on `<html>`.
8. **Verify.** Run typecheck, lint, test and build in `web/`. Then run the app and screenshot
   the first screens in light and dark mode.

## Conventions to hold from day one

- `components/ui/` holds registry files only; app components live elsewhere in `components/`.
- Semantic tokens only, never raw colours. Machine ids and dates in `font-mono`; dates from
  `formatDate` / `formatAbsolute` in ISO form.
- Page shape: `PageContainer` > `PageHeader` > `Card`s, with `DangerZone` last and
  `ConfirmDialog` in front of anything destructive.
- If a component needs a change that every app would want, make it in the Loom repo and
  re-add it here, rather than forking it locally.

## Constraints

- Follow my global CLAUDE.md: Conventional Commits, <spelling> spelling, PowerShell for any
  command you hand me, and no Claude attribution in commits or PRs.
- One logical change per commit.
````
