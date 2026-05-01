# Acuminator for Visual Studio Code (Preview)

This folder contains a new VS Code extension scaffold inspired by the Visual Studio Acuminator extension, focused on **Acumatica-specific syntax highlighting** for C# source files.

## Current features

- Injection grammar that highlights common Acumatica framework elements in `.cs` files:
  - Graph and graph extension classes
  - DAC classes and DAC nested BQL field declarations
  - Common BQL types and fluent-BQL surface area
  - Acumatica attributes (`[PX...]`)
  - Common Acumatica event handler names

## Scope and parity notes

The original Visual Studio extension includes rich Roslyn-based diagnostics, code fixes, and tool windows. Those are not part of this initial VS Code port yet.

This first version prioritizes syntax highlighting as requested, and provides a base structure where additional VS Code features can be layered in incrementally.

## Local development

```bash
cd vscode-acuminator
npm install
npm run build
```

Then press `F5` in VS Code to launch an Extension Development Host.
