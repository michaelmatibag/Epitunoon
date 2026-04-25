// ── State ──────────────────────────────────────────────────────────────────────
let allPrompts = [];
let editingId = null; // null = new prompt, string = editing existing

// ── DOM refs ───────────────────────────────────────────────────────────────────
const promptList  = document.getElementById('promptList');
const emptyState  = document.getElementById('emptyState');
const searchInput = document.getElementById('searchInput');
const formPanel   = document.getElementById('formPanel');
const formTitle   = document.getElementById('formTitle');
const formError   = document.getElementById('formError');
const fTitle      = document.getElementById('fTitle');
const fCategory   = document.getElementById('fCategory');
const fText       = document.getElementById('fText');

// ── Boot ───────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', async () => {
  allPrompts = await Storage.getPrompts();
  renderList(allPrompts);
  wireButtons();
});

// ── Render ─────────────────────────────────────────────────────────────────────
function renderList(prompts) {
  const hasAny = prompts.length > 0;
  emptyState.style.display  = hasAny ? 'none'  : 'flex';
  promptList.style.display  = hasAny ? 'block' : 'none';

  if (!hasAny) return;

  promptList.innerHTML = prompts.map((p) => `
    <div class="prompt-card" data-id="${esc(p.id)}">
      <div class="prompt-card-main">
        <div class="prompt-card-top">
          <span class="prompt-card-title">${esc(p.title)}</span>
          ${p.category ? `<span class="prompt-badge">${esc(p.category)}</span>` : ''}
        </div>
        <p class="prompt-card-preview">${esc(p.text.slice(0, 80))}${p.text.length > 80 ? '…' : ''}</p>
      </div>
      <div class="prompt-card-actions">
        <button class="btn-icon" data-action="edit"   data-id="${esc(p.id)}" title="Edit">✎</button>
        <button class="btn-icon btn-icon--danger" data-action="delete" data-id="${esc(p.id)}" title="Delete">✕</button>
      </div>
    </div>
  `).join('');

  // Delegate click handling for edit/delete buttons
  promptList.querySelectorAll('[data-action]').forEach((btn) => {
    btn.addEventListener('click', (e) => {
      const { action, id } = e.currentTarget.dataset;
      if (action === 'edit')   openForm(id);
      if (action === 'delete') handleDelete(id);
    });
  });
}

// ── Search ─────────────────────────────────────────────────────────────────────
function applySearch(query) {
  const q = query.toLowerCase().trim();
  const filtered = q
    ? allPrompts.filter(
        (p) =>
          p.title.toLowerCase().includes(q) ||
          p.text.toLowerCase().includes(q) ||
          (p.category || '').toLowerCase().includes(q)
      )
    : allPrompts;
  renderList(filtered);
}

// ── Form ───────────────────────────────────────────────────────────────────────
function openForm(id = null) {
  editingId = id;
  formError.style.display = 'none';

  if (id) {
    const p = allPrompts.find((x) => x.id === id);
    formTitle.textContent = 'Edit Prompt';
    fTitle.value    = p.title;
    fCategory.value = p.category || '';
    fText.value     = p.text;
  } else {
    formTitle.textContent = 'New Prompt';
    fTitle.value    = '';
    fCategory.value = '';
    fText.value     = '';
  }

  formPanel.style.display = 'flex';
  fTitle.focus();
}

function closeForm() {
  formPanel.style.display = 'none';
  editingId = null;
}

async function handleSave() {
  const title = fTitle.value.trim();
  const text  = fText.value.trim();

  if (!title) { showError('Title is required.'); return; }
  if (!text)  { showError('Prompt text is required.'); return; }

  const data = { title, text, category: fCategory.value.trim() || undefined };

  if (editingId) {
    allPrompts = await Storage.updatePrompt(editingId, data);
  } else {
    allPrompts = await Storage.addPrompt(data);
  }

  notifyTabs();
  closeForm();
  renderList(allPrompts);
}

async function handleDelete(id) {
  allPrompts = await Storage.deletePrompt(id);
  notifyTabs();
  applySearch(searchInput.value);
}

function showError(msg) {
  formError.textContent = msg;
  formError.style.display = 'block';
}

// ── Notify open tabs ───────────────────────────────────────────────────────────
// The background service_worker relays this to every Claude/ChatGPT tab.
function notifyTabs() {
  chrome.runtime.sendMessage({ type: 'PROMPTS_UPDATED', prompts: allPrompts });
}

// ── Wire up static buttons ─────────────────────────────────────────────────────
function wireButtons() {
  document.getElementById('btnNew').addEventListener('click', () => openForm());
  document.getElementById('btnNewEmpty').addEventListener('click', () => openForm());
  document.getElementById('btnCloseForm').addEventListener('click', closeForm);
  document.getElementById('btnCancel').addEventListener('click', closeForm);
  document.getElementById('btnSave').addEventListener('click', handleSave);
  searchInput.addEventListener('input', (e) => applySearch(e.target.value));
}

// ── Utility ────────────────────────────────────────────────────────────────────
function esc(str) {
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}
