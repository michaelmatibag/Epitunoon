// Relay PROMPTS_UPDATED messages from the popup to every open Claude/ChatGPT tab.
// This keeps the in-memory prompt list in each content script in sync
// immediately after the user saves changes in the popup.

chrome.runtime.onMessage.addListener((msg) => {
  if (msg.type !== 'PROMPTS_UPDATED') return;

  chrome.tabs.query(
    { url: ['https://claude.ai/*', 'https://chatgpt.com/*', 'https://chat.openai.com/*'] },
    (tabs) => {
      tabs.forEach((tab) => {
        // sendMessage can throw if the content script hasn't loaded yet — ignore those
        chrome.tabs.sendMessage(tab.id, msg).catch(() => {});
      });
    }
  );
});
