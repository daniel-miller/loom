# Building with this design system

These are the shared primitives behind four admin apps (training records, tickets, courses,
companies). Pages are dense, quiet, and token-coloured. Use the library components for every
control, and Tailwind utility classes with the semantic tokens below for your own layout.

## Setup

Link `styles.css` and load `_ds_bundle.js`. Everything is on `window.Loom`.

- **Wrap the tree in `TooltipProvider`.** `Tooltip` throws outside it. The apps use
  `<TooltipProvider delayDuration={200}>` at the root.
- **Mount `Toaster` once, then call `toast()`.** Both come from `window.Loom`.
  `toast("Course renamed")`, or `toast(title, { description, action: { label: "Undo", onClick } })`.
- **Dark mode is the `dark` class on `<html>`.** Every token flips under it, so components need
  no props for it. Pass `theme="dark"` to `Toaster` to match.

## Styling idiom

Tailwind v4 utilities that reference semantic tokens, never raw colours (`bg-primary`, not
`bg-indigo-600`). The stylesheet is precompiled: **arbitrary values such as `w-[300px]` do not
exist**, so use the scale (`w-72`, `min-h-80`, `max-w-md`). Spacing, sizing, grid and flex,
typography, borders, radius, shadow and position utilities, with `sm:`, `md:` and `lg:` variants,
are all available.

| Role | Classes |
|---|---|
| Surfaces | `bg-card` (content area and panels), `bg-background` (sidebar chrome), `bg-muted` (wells, hover) |
| Text | `text-foreground`, `text-foreground-strong`, `text-muted-foreground` (helpers, captions), `text-link` |
| Lines | `border-border`, `border-input-border`, `divide-y` |
| Action | `bg-primary text-primary-foreground`, `hover:bg-primary-hover` |
| Status | `text-success`, `text-warning`, `text-danger`, with tints like `bg-danger/10` |
| Shape | `rounded-card` (panels), `rounded-badge`, `rounded-lg`, `shadow-sm` |
| Type | `text-sm` is the body size (15px); `text-xs` for captions; `font-semibold` for titles |

For status, prefer `Badge variant="success" | "warning" | "destructive"` over hand-coloured text.

## House rules

- **Machine ids in mono.** Handles, numeric keys, UUIDs and dates go in `font-mono`; names and
  prose stay proportional. `CopyableId` is the standard way to show a copyable id.
- **Dates are ISO.** Render with `formatDate(iso)` (`2026-11-05`) or `formatAbsolute(iso)`
  (`2026-11-05 14:32 MST`), never `toLocaleDateString()`.
- **Sentence case** for titles, labels and buttons. Fragments (subtitles, helper text, badges)
  take no period.
- **Icons are Font Awesome Pro, sharp style:**
  `<i className="fa-sharp fa-regular fa-plus" aria-hidden="true" />` (`fa-solid` for filled).
  Put the icon before the label inside `Button`; an icon-only button uses `size="icon"` and an
  `aria-label`.
- **App shell:** `AppShell` > `Sidebar` (tinted) beside `ShellBody` > `Topbar` and `ShellMain`
  (white). The active `SidebarNavItem` is a soft primary fill with a left rule, never a solid
  pill. Give every nav item at one level an icon, or none of them. Below md the `Sidebar` hides
  and a second copy sits in `SidebarSheet`, opened by `SidebarTrigger` first in the `Topbar`.
- **Page shape:** `PageContainer` > `PageHeader` (title, subtitle, `actions`) > `Card`s, inside
  `ShellMain`. Cards sit on white, so their hairline border is what separates them. Destructive
  settings go last, in a `DangerZone`.
- **Confirm before destroying.** Use `ConfirmDialog` (danger tone by default), not a plain
  `Dialog`, for delete and archive.
- **Dialog footers** put Cancel (`variant="outline"`) before the primary action.

## Where the truth lives

Read `styles.css` and `_ds_bundle.css` before inventing a class. Each component's
`components/<group>/<Name>/<Name>.prompt.md` and `.d.ts` hold its API; compound components
(`Dialog`, `Select`, `DropdownMenu`, `Sheet`, `Table`, `Tabs`, `Card`) export their parts as
siblings, such as `DialogContent` and `SelectItem`.

## Example

```jsx
const { PageContainer, PageHeader, Button, Card, CardHeader, CardTitle, CardContent,
  Badge, formatDate } = window.Loom;

<PageContainer>
  <PageHeader
    title="Learners"
    subtitle="214 learners across 6 sites"
    actions={<Button><i className="fa-sharp fa-regular fa-plus" aria-hidden="true" />Add learner</Button>}
  />
  <div className="grid gap-4 md:grid-cols-3">
    <Card>
      <CardHeader><CardTitle>H2S Alive</CardTitle></CardHeader>
      <CardContent className="flex items-center justify-between text-sm">
        <span>Expires <span className="font-mono">{formatDate("2028-11-05")}</span></span>
        <Badge variant="success">Valid</Badge>
      </CardContent>
    </Card>
  </div>
</PageContainer>
```
