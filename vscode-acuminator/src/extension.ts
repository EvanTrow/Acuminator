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
  'acumaticaAttribute',
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
  'where','where2','and','and2','or','orderby','aggregate','aggregateto','groupby','on','leftjoin','innerjoin','select','select2','select5','selectfrom','search','search2',
  'isequal','equal','isnotequal','notequal','isnull','isnotnull','islike','isless','islessequal','isgreater','isgreaterequal','between','in',
  'fromcurrent','currentvalue','asc','desc','current','current2',
  'isworkgroupofcontact','isworkgrouporsubgroupofcontact'
]);


const bqlSelectCommandRegex = /\b((PX)?Select(GroupBy)?(OrderBy)?|Search\d?|PXSetup|PXUpdate|PXSelectReadonly\d?|PXSelectGroupJoin|PXSelectJoin(OrderBy|GroupBy)?|PX(Filtered)?Processing(Join)?)\b/g;
const bqlParameterRegex = /\b(Current2?|Optional2?|Required)\b/g;


const dacTypeofWithFieldRegex = /typeof\s*\(\s*(?:[A-Za-z_][A-Za-z0-9_]*\.)*([A-Z][A-Za-z0-9_]*)\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)\s*\)/g;
const dacTypeofRegex = /typeof\s*\(\s*(?:[A-Za-z_][A-Za-z0-9_]*\.)*([A-Z][A-Za-z0-9_]*)\s*\)/g;
const attributeRegex = /\[(PX[A-Za-z0-9_]+)(?:Attribute)?\b/g;
const bqlFieldGenericRegex = /Bql(?:String|Int|Bool|Guid|DateTime|Decimal|ByteArray|Long|Short)\.Field\s*<\s*([A-Za-z_][A-Za-z0-9_]*)\s*>/g;
const typeofKeywordRegex = /\btypeof\b/g;
const propertyDeclarationRegex = /\b(public)\s+(virtual\s+)?([A-Za-z_][A-Za-z0-9_<>\[\]\.?]+)\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?=\{|=>)/gm;

const bqlMemberOperatorRegex = /\b(where|where2|and|and2|or|orderby|aggregate|aggregateto|groupby|on|leftjoin|innerjoin|select\d*|selectfrom|search\d*|isequal|equal|isnotequal|notequal|isnull|isnotnull|islike|isless|islessequal|isgreater|isgreaterequal|between|in|fromcurrent|currentvalue|asc|desc|isworkgroupofcontact|isworkgrouporsubgroupofcontact)\b(?=\s*(<|\.|,|>))/gim;


function pushToken(builder: vscode.SemanticTokensBuilder, document: vscode.TextDocument, absoluteIndex: number, value: string, tokenType: number): void {
  if (!value) return;
  const start = document.positionAt(absoluteIndex);
  builder.push(start.line, start.character, value.length, tokenType, 0);
}

class AcumaticaSemanticTokensProvider implements vscode.DocumentSemanticTokensProvider {
  provideDocumentSemanticTokens(document: vscode.TextDocument): vscode.ProviderResult<vscode.SemanticTokens> {
    const text = document.getText();

    const isCSharpFile = document.fileName.toLowerCase().endsWith('.cs');
    const hasPxDataUsing = /(^|\n)\s*using\s+PX\.Data\s*;/m.test(text);

    if (!isCSharpFile || !hasPxDataUsing) {
      return new vscode.SemanticTokensBuilder(legend).build();
    }

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
      if (bqlOperatorNames.has(match[2].toLowerCase())) continue;
      const idx = match[0].indexOf(match[2]);
      pushToken(builder, document, match.index + idx, match[2], 0);
    }

    for (const match of text.matchAll(dacOperandRegex)) {
      if (match.index === undefined || !match[2]) continue;
      if (bqlOperatorNames.has(match[2].toLowerCase())) continue;
      const idx = match[0].indexOf(match[2]);
      pushToken(builder, document, match.index + idx, match[2], 0);
    }


    for (const match of text.matchAll(dacTypeofWithFieldRegex)) {
      if (match.index === undefined) continue;
      const full = match[0];
      const dac = match[1];
      const field = match[2];
      if (dac) pushToken(builder, document, match.index + full.indexOf(dac), dac, 0);
      if (field) pushToken(builder, document, match.index + full.lastIndexOf(field), field, 2);
    }

    for (const match of text.matchAll(dacTypeofRegex)) {
      if (match.index === undefined || !match[1]) continue;
      const idx = match[0].indexOf(match[1]);
      pushToken(builder, document, match.index + idx, match[1], 0);
    }

    for (const match of text.matchAll(propertyDeclarationRegex)) {
      if (match.index === undefined) continue;
      const full = match[0];
      const access = match[1];
      const virt = match[2]?.trim();
      const type = match[3];
      if (access) pushToken(builder, document, match.index + full.indexOf(access), access, 7);
      if (virt) pushToken(builder, document, match.index + full.indexOf(virt), virt, 7);
      if (type) pushToken(builder, document, match.index + full.indexOf(type), type, 7);
    }

    for (const match of text.matchAll(bqlFieldGenericRegex)) {
      if (match.index === undefined || !match[1]) continue;
      const idx = match[0].indexOf(match[1]);
      pushToken(builder, document, match.index + idx, match[1], 2);
    }

    for (const match of text.matchAll(attributeRegex)) {
      if (match.index === undefined || !match[1]) continue;
      pushToken(builder, document, match.index + 1, match[1], 9);
    }

    for (const match of text.matchAll(typeofKeywordRegex)) {
      if (match.index === undefined) continue;
      pushToken(builder, document, match.index, match[0], 7);
    }

    // BracesFormats parity: apply 14-level cyclic brace coloring
    let braceLevel = 0;
    for (let i = 0; i < text.length; i++) {
      const ch = text[i];
      if (ch === '{') {
        braceLevel++;
        const tokenType = 10 + Math.min(((braceLevel - 1) % 14) + 1, 14);
        pushToken(builder, document, i, ch, tokenType);
      } else if (ch === '}') {
        const tokenType = 10 + Math.min(((Math.max(braceLevel,1) - 1) % 14) + 1, 14);
        pushToken(builder, document, i, ch, tokenType);
        braceLevel = Math.max(0, braceLevel - 1);
      }
    }



    // Square bracket coloring for attribute containers
    for (let i = 0; i < text.length; i++) {
      const ch = text[i];
      if (ch === '[' || ch === ']') {
        pushToken(builder, document, i, ch, 4);
      }
    }

    // Angle bracket pair coloring for generic/BQL nesting: 14-level cyclic coloring for < and >
    let angleLevel = 0;
    for (let i = 0; i < text.length; i++) {
      const ch = text[i];
      if (ch === '<') {
        angleLevel++;
        const tokenType = 24 + Math.min(((angleLevel - 1) % 14) + 1, 14);
        pushToken(builder, document, i, ch, tokenType);
      } else if (ch === '>') {
        const tokenType = 24 + Math.min(((Math.max(angleLevel, 1) - 1) % 14) + 1, 14);
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
