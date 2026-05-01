import * as vscode from 'vscode';

const legend = new vscode.SemanticTokensLegend([
  'class',        // DAC
  'type',         // DAC field
  'parameter',    // BQL parameters
  'keyword',      // BQL operators
  'enumMember',   // BQL constant prefix
  'property',     // BQL constant ending
  'class',        // PXGraph/PXGraphExtension
  'method'        // PXAction member
]);

const patterns: Array<{ regex: RegExp; tokenType: number; group?: number }> = [
  { regex: /\bclass\s+([A-Z][A-Za-z0-9_]*)\s*:\s*(?:IBqlTable|PXBqlTable)\b/gm, tokenType: 0, group: 1 },
  { regex: /\babstract\s+class\s+([a-z][A-Za-z0-9_]*)\s*:\s*Bql(?:String|Int|Bool|Guid|DateTime|Decimal|ByteArray|Long|Short)\.Field</gm, tokenType: 1, group: 1 },
  { regex: /\b(?:Current2?|Optional2?|Required)\b/gm, tokenType: 2 },
  { regex: /\b(?:Where|Where2|And|And2|Or|OrderBy|AggregateTo|InnerJoin|LeftJoin|On|Set|Values|View|Select|SelectSingle|SelectFrom|Update|Delete|Insert)\b(?=\s*<)/gm, tokenType: 3 },
  { regex: /\bclass\s+([A-Z][A-Za-z0-9_]*)\s*:\s*PXGraph(?:Extension)?(?:<[^>]+>)?/gm, tokenType: 6, group: 1 },
  { regex: /\bPXAction\s*<[^>]+>\s+([A-Za-z_][A-Za-z0-9_]*)\b/gm, tokenType: 7, group: 1 },
  { regex: /\b([A-Z][A-Za-z0-9_]*)\.PK\b/gm, tokenType: 4, group: 1 },
  { regex: /\b([A-Z][A-Za-z0-9_]*)\.Constant\b/gm, tokenType: 5, group: 1 }
];

class AcumaticaSemanticTokensProvider implements vscode.DocumentSemanticTokensProvider {
  provideDocumentSemanticTokens(document: vscode.TextDocument): vscode.ProviderResult<vscode.SemanticTokens> {
    const text = document.getText();
    const builder = new vscode.SemanticTokensBuilder(legend);

    for (const pattern of patterns) {
      for (const match of text.matchAll(pattern.regex)) {
        const value = pattern.group ? match[pattern.group] : match[0];

        if (!value || match.index === undefined) {
          continue;
        }

        const startIndex = pattern.group ? match.index + match[0].indexOf(value) : match.index;
        const start = document.positionAt(startIndex);
        builder.push(start.line, start.character, value.length, pattern.tokenType, 0);
      }
    }

    return builder.build();
  }
}

export function activate(context: vscode.ExtensionContext): void {
  const selector: vscode.DocumentSelector = { language: 'csharp', scheme: 'file' };

  context.subscriptions.push(
    vscode.languages.registerDocumentSemanticTokensProvider(selector, new AcumaticaSemanticTokensProvider(), legend)
  );
}

export function deactivate(): void {
  // no-op
}
