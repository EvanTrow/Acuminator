import * as vscode from 'vscode';

const legend = new vscode.SemanticTokensLegend([
  'acumaticaDac',
  'acumaticaDacExtension',
  'acumaticaDacField',
  'acumaticaBqlParameter',
  'acumaticaBqlOperator',
  'acumaticaConstantPrefix',
  'acumaticaConstantEnding',
  'acumaticaGraph',
  'acumaticaAction',
  'acuminatorBraceLevel1', 'acuminatorBraceLevel2', 'acuminatorBraceLevel3', 'acuminatorBraceLevel4',
  'acuminatorBraceLevel5', 'acuminatorBraceLevel6', 'acuminatorBraceLevel7', 'acuminatorBraceLevel8',
  'acuminatorBraceLevel9', 'acuminatorBraceLevel10', 'acuminatorBraceLevel11', 'acuminatorBraceLevel12',
  'acuminatorBraceLevel13', 'acuminatorBraceLevel14',
  'acuminatorAngleLevel1', 'acuminatorAngleLevel2', 'acuminatorAngleLevel3', 'acuminatorAngleLevel4',
  'acuminatorAngleLevel5', 'acuminatorAngleLevel6', 'acuminatorAngleLevel7', 'acuminatorAngleLevel8',
  'acuminatorAngleLevel9', 'acuminatorAngleLevel10', 'acuminatorAngleLevel11', 'acuminatorAngleLevel12',
  'acuminatorAngleLevel13', 'acuminatorAngleLevel14'
]);

const declarationPatterns: Array<{ regex: RegExp; tokenType: number; group?: number }> = [
  { regex: /\b(?:public\s+|protected\s+|internal\s+|private\s+|partial\s+|abstract\s+|sealed\s+)*class\s+([A-Za-z_][A-Za-z0-9_]*)\s*:\s*[^\n{;]*\b(?:IBqlTable|PXBqlTable)\b/gm, tokenType: 0, group: 1 },
  { regex: /\b(?:public\s+|protected\s+|internal\s+|private\s+|partial\s+|abstract\s+|sealed\s+)*class\s+([A-Za-z_][A-Za-z0-9_]*)\s*:\s*[^\n{;]*\bPXCacheExtension\s*</gm, tokenType: 1, group: 1 },
  { regex: /\b(?:public\s+|protected\s+|internal\s+|private\s+|partial\s+|abstract\s+|sealed\s+)*class\s+([A-Za-z_][A-Za-z0-9_]*)\s*:\s*[^\n{;]*\bBql(?:String|Int|Bool|Guid|DateTime|Decimal|ByteArray|Long|Short)\.Field\s*</gm, tokenType: 2, group: 1 },
  { regex: /\b(?:Current2?|Optional2?|Required)\b/gm, tokenType: 3 },
  { regex: /\b(?:Where|Where2|And|And2|Or|OrderBy|AggregateTo|InnerJoin|LeftJoin|On|Set|Values|View|Select|SelectSingle|SelectFrom|Update|Delete|Insert|Search\d?|PXSelect(?:Readonly\d?|GroupJoin|Join(?:OrderBy|GroupBy)?)?|PXSetup|PXUpdate|PX(?:Filtered)?Processing(?:Join)?)\b(?=\s*<|\s*\.)/gm, tokenType: 4 },
  { regex: /\bclass\s+([A-Z][A-Za-z0-9_]*)\s*:\s*PXGraph(?:Extension)?(?:<[^>]+>)?/gm, tokenType: 7, group: 1 },
  { regex: /\bPXAction\s*<[^>]+>\s+([A-Za-z_][A-Za-z0-9_]*)\b/gm, tokenType: 8, group: 1 },
  { regex: /\b([A-Z][A-Za-z0-9_]*)\.PK\b/gm, tokenType: 5, group: 1 },
  { regex: /\b([A-Z][A-Za-z0-9_]*)\.Constant\b/gm, tokenType: 6, group: 1 }
];

const dacWithFieldRegex = /<\W*?(?:([A-Z]+\w*\.)?([A-Z]+\w*)+\d?\.\W*([a-z]+\w*\d*)([>|,])?)/g;
const dacOrConstantRegex = /<\W*?(?:([A-Z]+\w*\.)?([A-Z]+\w*\d?)\W*(>|,))/g;
const dacOperandRegex = /(,|<)?([A-Z]+\w*)\d?</g;

const bqlOperatorNames = new Set([
  'Where','Where2','And','And2','Or','OrderBy','Aggregate','AggregateTo','GroupBy','On','LeftJoin','InnerJoin','Select','Select2','Select5','SelectFrom','Search','Search2',
  'IsEqual','Equal','IsNotEqual','NotEqual','IsNull','IsNotNull','IsLike','IsLess','IsLessEqual','IsGreater','IsGreaterEqual','Between','In',
  'FromCurrent','CurrentValue','Asc','Desc','Current','Current2',
  'IsWorkgroupOfContact','IsWorkgroupOrSubgroupOfContact'
]);


const bqlSelectCommandRegex = /(PX)?Select(GroupBy)?(OrderBy)?|Search\d?|PXSetup|PXUpdate|PXSelectReadonly\d?|PXSelectGroupJoin|PXSelectJoin(OrderBy|GroupBy)?|PX(Filtered)?Processing(Join)?/g;
const bqlParameterRegex = /\b(Current2?|Optional2?|Required)\b/g;

const bqlMemberOperatorRegex = /\b(Where|Where2|And|And2|Or|OrderBy|Aggregate|AggregateTo|GroupBy|On|LeftJoin|InnerJoin|Select\d*|SelectFrom|Search\d*|IsEqual|Equal|IsNotEqual|NotEqual|IsNull|IsNotNull|IsLike|IsLess|IsLessEqual|IsGreater|IsGreaterEqual|Between|In|FromCurrent|CurrentValue|Asc|Desc|IsWorkgroupOfContact|IsWorkgroupOrSubgroupOfContact)\b(?=\s*(<|\.|,|>))/gm;


function pushToken(builder: vscode.SemanticTokensBuilder, document: vscode.TextDocument, absoluteIndex: number, value: string, tokenType: number): void {
  if (!value) return;
  const start = document.positionAt(absoluteIndex);
  builder.push(start.line, start.character, value.length, tokenType, 0);
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

    
    // VS Regex colorizer parity: explicit BQL select command and parameter passes
    for (const match of text.matchAll(bqlSelectCommandRegex)) {
      if (match.index === undefined) continue;
      pushToken(builder, document, match.index, match[0], 4);
    }

    for (const match of text.matchAll(bqlParameterRegex)) {
      if (match.index === undefined) continue;
      pushToken(builder, document, match.index, match[0], 3);
    }

    for (const match of text.matchAll(bqlMemberOperatorRegex)) {
      if (match.index === undefined || !match[1]) continue;
      pushToken(builder, document, match.index, match[1], 4);
    }

    for (const match of text.matchAll(dacWithFieldRegex)) {
      if (match.index === undefined) continue;
      const full = match[0];
      const dac = match[2];
      const dacField = match[3];
      if (dac) pushToken(builder, document, match.index + full.indexOf(dac), dac, 0);
      if (dacField) {
        const hint = dac ? full.indexOf(dac) + dac.length : 0;
        const fieldIdx = full.indexOf(dacField, hint);
        pushToken(builder, document, match.index + (fieldIdx >= 0 ? fieldIdx : full.indexOf(dacField)), dacField, 2);
      }
    }

    for (const match of text.matchAll(dacOrConstantRegex)) {
      if (match.index === undefined || !match[2]) continue;
      if (bqlOperatorNames.has(match[2])) continue;
      const idx = match[0].indexOf(match[2]);
      pushToken(builder, document, match.index + idx, match[2], 0);
    }

    for (const match of text.matchAll(dacOperandRegex)) {
      if (match.index === undefined || !match[2]) continue;
      if (bqlOperatorNames.has(match[2])) continue;
      const idx = match[0].indexOf(match[2]);
      pushToken(builder, document, match.index + idx, match[2], 0);
    }

    // BracesFormats parity: apply 14-level cyclic brace coloring
    let braceLevel = 0;
    for (let i = 0; i < text.length; i++) {
      const ch = text[i];
      if (ch === '{') {
        braceLevel++;
        const tokenType = 8 + Math.min(((braceLevel - 1) % 14) + 1, 14);
        pushToken(builder, document, i, ch, tokenType);
      } else if (ch === '}') {
        const tokenType = 8 + Math.min(((Math.max(braceLevel,1) - 1) % 14) + 1, 14);
        pushToken(builder, document, i, ch, tokenType);
        braceLevel = Math.max(0, braceLevel - 1);
      }
    }


    // Angle bracket pair coloring for generic/BQL nesting: 14-level cyclic coloring for < and >
    let angleLevel = 0;
    for (let i = 0; i < text.length; i++) {
      const ch = text[i];
      if (ch === '<') {
        angleLevel++;
        const tokenType = 22 + Math.min(((angleLevel - 1) % 14) + 1, 14);
        pushToken(builder, document, i, ch, tokenType);
      } else if (ch === '>') {
        const tokenType = 22 + Math.min(((Math.max(angleLevel, 1) - 1) % 14) + 1, 14);
        pushToken(builder, document, i, ch, tokenType);
        angleLevel = Math.max(0, angleLevel - 1);
      }
    }

    return builder.build();
  }
}

export function activate(context: vscode.ExtensionContext): void {
  const selector: vscode.DocumentSelector = { language: 'csharp', scheme: 'file' };
  context.subscriptions.push(vscode.languages.registerDocumentSemanticTokensProvider(selector, new AcumaticaSemanticTokensProvider(), legend));
}

export function deactivate(): void {
  // no-op
}
