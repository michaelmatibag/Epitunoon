// Default prompts shipped with the extension on first install.
const DEFAULT_PROMPTS = [
  {
    id: 'default-1',
    title: 'Explain Simply',
    text: 'Explain the following in simple, clear language that anyone can understand:\n\n',
    category: 'Learning'
  },
  {
    id: 'default-2',
    title: 'Code Review',
    text: 'Review the following code for bugs, performance issues, and best practices. Give specific, actionable feedback:\n\n',
    category: 'Coding'
  },
  {
    id: 'default-3',
    title: 'Summarize',
    text: 'Summarize the following into 3-5 bullet points, capturing only the most important ideas:\n\n',
    category: 'Writing'
  },
  {
    id: 'default-4',
    title: "Devil's Advocate",
    text: "Argue the strongest possible counterargument to the following position. Be thorough and convincing:\n\n",
    category: 'Thinking'
  },
  {
    id: 'default-5',
    title: 'Action Items',
    text: 'Extract all action items, decisions, and next steps from the following. Format as a numbered checklist:\n\n',
    category: 'Productivity'
  },
  {
    id: 'default-6',
    title: 'Improve Writing',
    text: 'Rewrite the following to be clearer, more concise, and more engaging. Keep the original meaning:\n\n',
    category: 'Writing'
  }
];

// Storage is an IIFE so it doesn't pollute the global scope beyond the one name.
const Storage = (() => {
  async function getPrompts() {
    return new Promise((resolve) => {
      chrome.storage.local.get(['prompts', 'initialized'], (result) => {
        if (!result.initialized) {
          chrome.storage.local.set({ prompts: DEFAULT_PROMPTS, initialized: true });
          resolve(DEFAULT_PROMPTS);
        } else {
          resolve(result.prompts || []);
        }
      });
    });
  }

  async function savePrompts(prompts) {
    return new Promise((resolve) => {
      chrome.storage.local.set({ prompts }, resolve);
    });
  }

  async function addPrompt(prompt) {
    const prompts = await getPrompts();
    const created = { ...prompt, id: Date.now().toString() };
    const updated = [...prompts, created];
    await savePrompts(updated);
    return updated;
  }

  async function deletePrompt(id) {
    const prompts = await getPrompts();
    const updated = prompts.filter((p) => p.id !== id);
    await savePrompts(updated);
    return updated;
  }

  async function updatePrompt(id, changes) {
    const prompts = await getPrompts();
    const updated = prompts.map((p) => (p.id === id ? { ...p, ...changes } : p));
    await savePrompts(updated);
    return updated;
  }

  return { getPrompts, savePrompts, addPrompt, deletePrompt, updatePrompt };
})();
