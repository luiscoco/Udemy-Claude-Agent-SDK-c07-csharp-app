/* Local Prism integration: never evaluates source code or inserts unescaped source HTML. */
(() => {
  const Prism = globalThis.Prism;
  if (!Prism) return;
  // Small C# grammar on top of Prism's C-like base (the bundled build has no C# component).
  Prism.languages.csharp = Prism.languages.extend("clike", {
    string: [{ pattern: /\$?@?"(?:""|\\.|[^\\"])*"/, greedy: true }, { pattern: /'(?:\\.|[^\\'])'/, greedy: true }],
    "class-name": /\b[A-Z]\w*(?=\s*(?:[<({.]|\s+\w))/,
    keyword: /\b(?:abstract|as|async|await|base|bool|break|case|catch|class|const|continue|decimal|default|do|double|else|enum|false|finally|for|foreach|if|in|int|interface|internal|is|long|namespace|new|null|object|out|override|private|protected|public|readonly|record|return|sealed|static|string|switch|this|throw|true|try|using|var|void|when|while)\b/,
  });
  Prism.languages.dotenv = {
    comment: { pattern: /^\s*#.*$/m, greedy: true },
    property: { pattern: /(^|\n)\s*(?:export\s+)?[A-Z_][A-Z_0-9]*(?=\s*=)/, lookbehind: true },
    string: { pattern: /([=]\s*)[^\r\n]+/, lookbehind: true },
    operator: /=/,
  };
  function refresh() {
    for (const code of document.querySelectorAll("pre > code[class*='language-']")) {
      if (code.dataset.highlighted === code.textContent) continue;
      Prism.highlightElement(code);
      code.dataset.highlighted = code.textContent;
    }
  }
  refresh();
  globalThis.Blazor?.addEventListener?.("enhancedload", refresh);
})();
