# Acuminator for Visual Studio Code (Preview)

This extension includes a semantic colorizer layer modeled after the Visual Studio Acuminator colorizer behavior.

## Why colors were white

VS Code does not automatically style arbitrary custom semantic token types. If token types are custom and no active theme/user setting defines their colors, they often render with default text color.

## Fix implemented

The semantic provider now emits **built-in VS Code semantic token types** (such as `class`, `type`, `keyword`, `parameter`, `method`) so the active VS Code theme applies colors immediately.

## Acumatica elements currently colorized

- DAC class names
- DAC nested BQL field class names
- BQL parameter keywords (`Current`, `Current2`, `Optional`, `Optional2`, `Required`)
- Fluent BQL operators (`Where`, `And`, `Or`, `OrderBy`, joins, etc.)
- BQL constant-style prefixes/endings
- `PXGraph` / `PXGraphExtension` class names
- `PXAction<T>` member names

## Local development

```bash
cd vscode-acuminator
npm install
npm run build
```

Press `F5` to launch an Extension Development Host.
