# Acuminator for Visual Studio Code (Preview)

This extension now includes a **semantic colorizer layer** to better match the richer coloring style of the Visual Studio extension.

## What was added

In addition to TextMate regex highlighting, the extension now registers a C# semantic tokens provider that colorizes Acumatica-specific symbols:

- DAC class names
- DAC nested BQL field class names
- BQL parameter keywords (`Current`, `Current2`, `Optional`, `Optional2`, `Required`)
- Fluent BQL operators (`Where`, `And`, `Or`, `OrderBy`, joins, etc.)
- BQL constant-style prefixes/endings
- `PXGraph`/`PXGraphExtension` class names
- `PXAction<T>` member names

This mirrors the original Visual Studio colorizer intent more closely than TextMate-only rules.

## Local development

```bash
cd vscode-acuminator
npm install
npm run build
```

Press `F5` to launch an Extension Development Host.
