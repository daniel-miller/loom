import bump from "./styles/bump.css?inline";
import cmds from "./styles/cmds.css?inline";
import festival from "./styles/festival.css?inline";
import openscorm from "./styles/openscorm.css?inline";

export const palettes = { cmds, bump, festival, openscorm } as const;

export type Palette = keyof typeof palettes;

export function isPalette(value: string | null): value is Palette {
  return value !== null && value in palettes;
}

const PALETTE_KEY = "loom.palette";
const THEME_KEY = "loom.theme";

// ?palette= and ?theme= take precedence over the stored choice, so a headless capture can ask for
// any combination.
export function storedPalette(): Palette {
  const requested = new URLSearchParams(location.search).get("palette");
  if (isPalette(requested)) return requested;
  try {
    const value = localStorage.getItem(PALETTE_KEY);
    if (isPalette(value)) return value;
  } catch {
    // Storage can be blocked; the default palette still renders.
  }
  return "cmds";
}

// One <style> element holds the active palette's whole stylesheet. Swapping its text is the
// switch; ?inline imports hand over the compiled CSS as a string in dev and in the build alike.
export function applyPalette(palette: Palette) {
  let el = document.getElementById("palette") as HTMLStyleElement | null;
  if (!el) {
    el = document.createElement("style");
    el.id = "palette";
    document.head.appendChild(el);
  }
  el.textContent = palettes[palette];
  try {
    localStorage.setItem(PALETTE_KEY, palette);
  } catch {
    // Not remembered across reloads, which is all that is lost.
  }
}

export function applyTheme(dark: boolean) {
  document.documentElement.classList.toggle("dark", dark);
  try {
    localStorage.setItem(THEME_KEY, dark ? "dark" : "light");
  } catch {
    // As above.
  }
}
