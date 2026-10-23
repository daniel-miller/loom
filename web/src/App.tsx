import { useState } from "react";
import { Button } from "@/components/ui/button";
import { PageContainer } from "@/components/ui/page-container";
import { PageHeader } from "@/components/ui/page-header";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Toaster } from "@/components/ui/toaster";
import {
  applyPalette,
  applyTheme,
  isPalette,
  palettes,
  storedPalette,
  type Palette,
} from "./palettes";
import { BadgeSection } from "./sections/badge";
import { ButtonSection } from "./sections/button";
import { CardSection } from "./sections/card";
import { SkeletonSection, TableSection, TabsSection } from "./sections/content";
import {
  ConfirmDialogSection,
  CopyableIdSection,
  DangerZoneSection,
  ToastSection,
} from "./sections/feedback";
import { CheckboxSection, InputSection } from "./sections/forms";
import { AppShellSection } from "./sections/layout";
import {
  DialogSheetSection,
  DropdownMenuSection,
  PopoverTooltipSection,
  SelectSection,
} from "./sections/overlays";
import { CalendarSection, FileDropzoneSection, SliderSection } from "./sections/pickers";

const sections = [
  { id: "button", title: "Button", Component: ButtonSection },
  { id: "badge", title: "Badge", Component: BadgeSection },
  { id: "card", title: "Card", Component: CardSection },
  { id: "app-shell", title: "App shell", Component: AppShellSection },
  { id: "input", title: "Input", Component: InputSection },
  { id: "checkbox", title: "Checkbox", Component: CheckboxSection },
  { id: "select", title: "Select", Component: SelectSection },
  { id: "dropdown-menu", title: "Dropdown menu", Component: DropdownMenuSection },
  { id: "popover", title: "Popover", Component: PopoverTooltipSection },
  { id: "dialog", title: "Dialog", Component: DialogSheetSection },
  { id: "tabs", title: "Tabs", Component: TabsSection },
  { id: "table", title: "Table", Component: TableSection },
  { id: "skeleton", title: "Skeleton", Component: SkeletonSection },
  { id: "slider", title: "Slider", Component: SliderSection },
  { id: "calendar", title: "Calendar", Component: CalendarSection },
  { id: "file-dropzone", title: "File dropzone", Component: FileDropzoneSection },
  { id: "toast", title: "Toast", Component: ToastSection },
  { id: "confirm-dialog", title: "Confirm dialog", Component: ConfirmDialogSection },
  { id: "danger-zone", title: "Danger zone", Component: DangerZoneSection },
  { id: "copyable-id", title: "Copyable id", Component: CopyableIdSection },
];

export function App() {
  const [palette, setPalette] = useState<Palette>(storedPalette);
  const [dark, setDark] = useState(() => document.documentElement.classList.contains("dark"));

  return (
    <div className="min-h-full">
      <header className="border-border bg-background/90 sticky top-0 z-10 border-b backdrop-blur">
        {/* Left-aligned like PageContainer, which expects an app shell to place it. */}
        <div className="flex max-w-5xl items-center gap-3 px-6 py-3">
          <span className="font-semibold">Loom specimen</span>
          <div className="ml-auto flex items-center gap-2">
            <Select
              value={palette}
              onValueChange={(value) => {
                if (!isPalette(value)) return;
                setPalette(value);
                applyPalette(value);
              }}
            >
              <SelectTrigger className="w-36" aria-label="Palette">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {Object.keys(palettes).map((name) => (
                  <SelectItem key={name} value={name}>
                    {name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button
              variant="outline"
              size="icon"
              aria-label={dark ? "Switch to light" : "Switch to dark"}
              onClick={() => {
                setDark(!dark);
                applyTheme(!dark);
              }}
            >
              <i
                className={dark ? "fa-sharp fa-regular fa-sun" : "fa-sharp fa-regular fa-moon"}
                aria-hidden="true"
              />
            </Button>
          </div>
        </div>
      </header>

      <PageContainer>
        <PageHeader
          title="Components"
          subtitle="Every registry component, rendered from the registry source"
        />

        <nav aria-label="Components" className="flex flex-wrap gap-x-4 gap-y-1 text-sm">
          {sections.map(({ id, title }) => (
            <a key={id} href={`#${id}`} className="text-link hover:underline">
              {title}
            </a>
          ))}
        </nav>

        {sections.map(({ id, Component }) => (
          <Component key={id} />
        ))}
      </PageContainer>

      <Toaster theme={dark ? "dark" : "light"} />
    </div>
  );
}
