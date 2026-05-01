import * as vscode from 'vscode';

const legend = new vscode.SemanticTokensLegend([
  'acumaticaDac',
  'acumaticaDacField',
  'acumaticaBqlParameter',
  'acumaticaBqlOperator',
  'acumaticaConstantPrefix',
  'acumaticaConstantEnding',
  'acumaticaGraph',
  'acumaticaAction'
]);

const declarationPatterns: Array<{ regex: RegExp; tokenType: number; group?: number }> = [
  { regex: /\b(?:public\s+|protected\s+|internal\s+|private\s+|partial\s+|abstract\s+|sealed\s+)*class\s+([A-Za-z_][A-Za-z0-9_]*)\s*:\s*[^\n{;]*\b(?:IBqlTable|PXBqlTable)\b/gm, tokenType: 0, group: 1 },
  { regex: /\b(?:public\s+|protected\s+|internal\s+|private\s+|partial\s+|abstract\s+|sealed\s+)*class\s+([A-Za-z_][A-Za-z0-9_]*)\s*:\s*[^\n{;]*\bBql(?:String|Int|Bool|Guid|DateTime|Decimal|ByteArray|Long|Short)\.Field\s*</gm, tokenType: 1, group: 1 },
  { regex: /\b(?:Current2?|Optional2?|Required)\b/gm, tokenType: 2 },
  { regex: /\b(?:Where|Where2|And|And2|Or|OrderBy|AggregateTo|InnerJoin|LeftJoin|On|Set|Values|View|Select|SelectSingle|SelectFrom|Update|Delete|Insert|Search\d?|PXSelect(?:Readonly\d?|GroupJoin|Join(?:OrderBy|GroupBy)?)?|PXSetup|PXUpdate|PX(?:Filtered)?Processing(?:Join)?)\b(?=\s*<|\s*\.)/gm, tokenType: 3 },
  { regex: /\bclass\s+([A-Z][A-Za-z0-9_]*)\s*:\s*PXGraph(?:Extension)?(?:<[^>]+>)?/gm, tokenType: 6, group: 1 },
  { regex: /\bPXAction\s*<[^>]+>\s+([A-Za-z_][A-Za-z0-9_]*)\b/gm, tokenType: 7, group: 1 },
  { regex: /\b([A-Z][A-Za-z0-9_]*)\.PK\b/gm, tokenType: 4, group: 1 },
  { regex: /\b([A-Z][A-Za-z0-9_]*)\.Constant\b/gm, tokenType: 5, group: 1 }
];

// Ports core regex intent from Acuminator.Vsix Coloriser/Regex/Utils/RegExpressions.cs for BQL usage highlighting.
const dacWithFieldRegex = /<\W*?(?:([A-Z]+\w*\.)?([A-Z]+\w*)+\d?\.\W*([a-z]+\w*\d*)([>|,])?)/g;
const dacOrConstantRegex = /<\W*?(?:([A-Z]+\w*\.)?([A-Z]+\w*\d?)\W*(>|,))/g;
const dacOperandRegex = /(,|<)?([A-Z]+\w*)\d?</g;

function pushToken(builder: vscode.SemanticTokensBuilder, document: vscode.TextDocument, absoluteIndex: number, value: string, tokenType: number): void {
  if (!value) return;
  const start = document.positionAt(absoluteIndex);
  builder.push(start.line, start.character, value.length, tokenType, 0);
}

function indexOfCaptureInMatch(full: string, capture: string, hintStart = 0): number {
  const idx = full.indexOf(capture, hintStart);
  return idx >= 0 ? idx : full.indexOf(capture);
}

class AcumaticaSemanticTokensProvider implements vscode.DocumentSemanticTokensProvider {
  provideDocumentSemanticTokens(document: vscode.TextDocument): vscode.ProviderResult<vscode.SemanticTokens> {
    const text = document.getText();
    const builder = new vscode.SemanticTokensBuilder(legend);

    for (const pattern of declarationPatterns) {
      for (const match of text.matchAll(pattern.regex)) {
        const value = pattern.group ? match[pattern.group] : match[0];
        if (!value || match.index === undefined) continue;

        const startIndex = pattern.group ? match.index + match[0].indexOf(value) : match.index;
        pushToken(builder, document, startIndex, value, pattern.tokenType);
      }
    }

    for (const match of text.matchAll(dacWithFieldRegex)) {
      if (match.index === undefined) continue;
      const full = match[0];
      const dac = match[2];
      const dacField = match[3];

      if (dac) {
        const localIdx = indexOfCaptureInMatch(full, dac);
        pushToken(builder, document, match.index + localIdx, dac, 0);
      }

      if (dacField) {
        const startFrom = dac ? indexOfCaptureInMatch(full, dac) + dac.length : 0;
        const localIdx = indexOfCaptureInMatch(full, dacField, startFrom);
        pushToken(builder, document, match.index + localIdx, dacField, 1);
      }
    }

    for (const match of text.matchAll(dacOrConstantRegex)) {
      if (match.index === undefined) continue;
      const full = match[0];
      const dacOrConst = match[2];
      if (!dacOrConst) continue;

      const localIdx = indexOfCaptureInMatch(full, dacOrConst);
      pushToken(builder, document, match.index + localIdx, dacOrConst, 0);
    }

    for (const match of text.matchAll(dacOperandRegex)) {
      if (match.index === undefined) continue;
      const full = match[0];
      const operand = match[2];
      if (!operand) continue;

      const localIdx = indexOfCaptureInMatch(full, operand);
      pushToken(builder, document, match.index + localIdx, operand, 0);
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
