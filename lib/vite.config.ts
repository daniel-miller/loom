import { defineConfig, type Plugin } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import dts from "vite-plugin-dts";
import path from "path";
import postcss, { type ChildNode } from "postcss";
import { readFileSync } from "fs";
import { registryPaths } from "../registry-paths";

// Claude Design registers every custom property in styles.css as a token, and warns on the ones
// with no kind or declared under a component selector. Tailwind's --tw-*, Font Awesome's --fa and
// react-day-picker's --rdp-* are library internals, and --ease-*, --animate-*, --aspect-* and
// --default-transition-* are Tailwind theme defaults rather than tokens, so each declaration is
// tagged /* @kind other */. A tag attaches to the declaration whose ; it follows, so it goes after
// the declaration only: one before a rule's first declaration would follow the { and attach to
// nothing. It runs on the emitted asset because minification strips ordinary comments.
const internalVar = /^--(?:tw-|_?fa(?:-|$)|rdp-|ease-|animate-|aspect-|default-transition-)/;

function kindTag() {
  return postcss.comment({ text: "@kind other", raws: { left: " ", right: " " } });
}

function isKindTag(node: ChildNode | undefined) {
  return node?.type === "comment" && node.text === "@kind other";
}

function tagInternalVars(): Plugin {
  return {
    name: "tag-internal-vars",
    enforce: "post",
    generateBundle(_, bundle) {
      for (const file of Object.values(bundle)) {
        if (file.type !== "asset" || !file.fileName.endsWith(".css")) continue;
        const css =
          typeof file.source === "string" ? file.source : new TextDecoder().decode(file.source);
        const root = postcss.parse(css);
        root.walkDecls(internalVar, (decl) => {
          if (!isKindTag(decl.next())) decl.after(kindTag());
        });
        file.source = root.toString();
      }
    },
  };
}

const pkg = JSON.parse(readFileSync(path.resolve(__dirname, "package.json"), "utf-8")) as {
  dependencies: Record<string, string>;
  peerDependencies: Record<string, string>;
};

// Every dependency stays an import in dist/: the converter bundles them itself, and React must be
// the one copy it supplies. Stylesheets are the exception: a dependency's CSS (react-day-picker's)
// is compiled into dist/styles.css, the one stylesheet a design receives.
const external = [...Object.keys(pkg.dependencies), ...Object.keys(pkg.peerDependencies)];

export default defineConfig({
  plugins: [
    registryPaths(),
    react(),
    tailwindcss(),
    // One rolled-up index.d.ts, so no declaration file is left importing an @/ path the
    // converter cannot follow.
    dts({ tsconfigPath: "./tsconfig.json", entryRoot: "..", rollupTypes: true }),
    tagInternalVars(),
  ],
  build: {
    lib: {
      entry: path.resolve(__dirname, "index.ts"),
      formats: ["es"],
      fileName: "index",
      cssFileName: "styles",
    },
    rollupOptions: {
      external: (id) =>
        (!id.endsWith(".css") &&
          external.some((name) => id === name || id.startsWith(name + "/"))) ||
        id === "react/jsx-runtime",
    },
  },
});
