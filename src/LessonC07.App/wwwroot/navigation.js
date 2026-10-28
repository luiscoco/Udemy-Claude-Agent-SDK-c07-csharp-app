// Theme toggle and active-link marking. Uses event delegation and re-runs after Blazor
// enhanced navigation, because page content is swapped without a full reload.
(() => {
  const key = "c07-theme";
  function saved() { try { return localStorage.getItem(key) === "dark" ? "dark" : "light"; } catch { return "light"; } }
  function apply(theme) {
    document.documentElement.dataset.theme = theme;
    const button = document.getElementById("student-theme-toggle");
    if (button) {
      button.textContent = theme === "dark" ? "Switch to light mode" : "Switch to dark mode";
      button.setAttribute("aria-pressed", String(theme === "dark"));
    }
  }
  function active() {
    const selected = location.pathname === "/guide" ? "guide" + (location.hash || "#learning")
      : location.pathname === "/architecture" ? "architecture" : "#application-lab";
    for (const a of document.querySelectorAll(".student-nav a")) {
      if (a.getAttribute("href") === selected) a.setAttribute("aria-current", "location");
      else a.removeAttribute("aria-current");
    }
  }
  function refresh() { apply(saved()); active(); }
  document.addEventListener("click", (e) => {
    if (e.target.closest?.("#student-theme-toggle")) {
      const theme = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
      try { localStorage.setItem(key, theme); } catch { /* Theme still applies for this page. */ }
      apply(theme);
    }
    // The lab link is a hash on the home page; from other pages, go home first.
    const lab = e.target.closest?.("a[data-lab-link]");
    if (lab && location.pathname !== "/") { e.preventDefault(); location.href = "/#application-lab"; }
  });
  refresh();
  window.addEventListener("hashchange", active);
  globalThis.Blazor?.addEventListener?.("enhancedload", refresh);
})();
