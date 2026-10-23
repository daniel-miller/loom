# Claude Design sync notes

The project is "Loom" at https://claude.ai/design/p/eae8e665-84b7-4e4d-a6b2-fe1676d82b62
(first synced 2026-10-09). Package shape, no Storybook: previews are ported from the `web/`
specimen sections.

## How the build runs

- **Build the library first.** `npm run build:lib` writes `lib/dist/`, which the converter reads
  with `--entry ./lib/dist/index.js --node-modules ./node_modules`. `cssEntry` is not set: the
  converter finds `lib/dist/styles.css` on its own, and a `cssEntry` path resolves against `lib/`,
  not the repo root.
- **Playwright 1.64.0** is the release that pins the cached Chromium build 1248. It is installed
  in `.ds-sync/` beside the converter deps, not in the repo.
- **Bash heredocs with an apostrophe** in a long multi-file command failed to parse in the Bash
  tool on Windows. Write preview files with the file tool instead.

## Decisions

- **Families only.** 69 sub-parts (`DialogContent`, `SelectItem`, `CardHeader`...) are
  `componentSrcMap: null`. They stay in the bundle; each family's preview composes them.
- **Groups follow the specimen sections**, through one-line category stubs in
  `.design-sync/groups/` mapped by `docsMap`.
- **`TooltipProvider` is the global `provider`**, as in `web/src/main.tsx`. Radix `Tooltip`
  throws without it. Changing the provider clears every grade.
- **`toast` is re-exported from `lib/index.ts`.** Apps import it from sonner directly; a design
  has only the bundle.
- **`lib/styles.css` safelists utility families** with `@source inline(...)`. Without it the
  stylesheet held only classes found in `registry/` and `web/src`, so common layout classes
  (`w-16`, `grid-cols-3`, `gap-8`, `sm:grid-cols-2`) rendered unstyled. Arbitrary values
  (`min-h-[300px]`) are still absent by design; previews and designs must use the scale.
- **Library internals are tagged `/* @kind other */`.** Claude Design registers every custom
  property in `styles.css` as a token. It warned on 219 `--tw-*` declarations under variant and
  `[data-*]` selectors, and on 3,837 with no kind, 3,791 of which are Font Awesome's per-icon
  `--fa`. `tagInternalVars` in `lib/vite.config.ts` tags every `--tw-*`, `--fa*`, `--_fa*` and
  `--rdp-*` declaration on both sides, after minification. No rule mixes them with real tokens.
- **Overlay previews open by default** (`defaultOpen` or `open`) with `cardMode: single` and a
  viewport per component. `DropdownMenu` sets `modal={false}` so the open menu does not lock the
  card.

## Known render warns

None at the end of the first sync.

## Re-sync risks

- **Previews duplicate specimen content.** If a `web/src/sections/*.tsx` example changes, the
  matching `.design-sync/previews/<Name>.tsx` does not follow. Port the change by hand.
- **The safelist is hand-maintained.** A new semantic colour token in `tokens/base.css` or a
  palette needs adding to the colour lines in `lib/styles.css`, or designs cannot use it.
- **`conventions.md` names classes and exports.** Re-validate it against the fresh build on every
  sync (the skill does this); a renamed token or block breaks it silently.
- **Only the cmds palette ships.** `lib/styles.css` imports `palettes/cmds.css`. The other three
  palettes are not in Claude Design.
- **Extracted `.d.ts` files drop inherited DOM props** (`onClick`, `placeholder`, `disabled`) by
  converter design, and print `React_2.ReactNode` where the rolled-up types alias React. Both are
  cosmetic for the agent; override with `dtsPropsFor` if a component's API reads wrong.
- **`_ds_bundle.css` is about 1.65 MB** because the fonts and Font Awesome are inlined, and the
  `@kind` tags add 144 KB (5 KB gzipped). Fine today; watch it if more font families are added.
- **The `@kind` placement is unconfirmed.** No published doc says which side of a declaration
  Claude Design reads the tag from, hence both. If the warnings persist after a sync, ask it.
