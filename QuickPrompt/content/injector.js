// Entry point for the content script.
// Guards against double-initialisation (can happen if Chrome injects the
// script more than once into the same page during SPA navigation).
(function () {
  if (window.__quickPromptLoaded) return;
  window.__quickPromptLoaded = true;

  const picker = new PromptPicker();
  picker.init();
})();
