// PromptPicker handles everything in the content script:
//   - Watching for the // trigger in Claude / ChatGPT input boxes
//   - Rendering and positioning the floating picker UI
//   - Keyboard navigation (↑↓ Enter Esc)
//   - Inserting the chosen prompt text into the editor

class PromptPicker {
  constructor() {
    this.picker = null;       // The floating DOM element
    this.prompts = [];        // All saved prompts
    this.filteredPrompts = [];// Subset shown after search filtering
    this.selectedIndex = 0;   // Which item is highlighted
    this.activeInput = null;  // The textarea/contenteditable that triggered us
    this.searchQuery = '';    // Text typed after //
    this.isVisible = false;

    // Bind so we can add AND remove the same listener reference
    this._onInput = this._onInput.bind(this);
    this._onKeydown = this._onKeydown.bind(this);
    this._onMousedown = this._onMousedown.bind(this);
  }

  // Called once from injector.js after the DOM is ready
  async init() {
    this.prompts = await Storage.getPrompts();

    document.addEventListener('input', this._onInput, true);
    document.addEventListener('keydown', this._onKeydown, true);
    document.addEventListener('mousedown', this._onMousedown, true);

    // Keep prompt list in sync when the popup saves changes
    chrome.runtime.onMessage.addListener((msg) => {
      if (msg.type === 'PROMPTS_UPDATED') {
        this.prompts = msg.prompts;
      }
    });
  }

  // ─── Event Handlers ────────────────────────────────────────────────────────

  _onInput(e) {
    const el = e.target;
    if (!this._isEditable(el)) return;

    const text = this._getText(el);

    // Regex: // followed by any non-newline chars at end of string
    const match = text.match(/\/\/([^\n]*)$/);
    if (match) {
      this.activeInput = el;
      this.searchQuery = match[1];
      this._show(el);
      this._filter(this.searchQuery);
    } else if (this.isVisible) {
      this._hide();
    }
  }

  _onKeydown(e) {
    if (!this.isVisible) return;

    if (e.key === 'ArrowDown') {
      e.preventDefault();
      e.stopPropagation();
      this.selectedIndex = Math.min(this.selectedIndex + 1, this.filteredPrompts.length - 1);
      this._renderItems();
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      e.stopPropagation();
      this.selectedIndex = Math.max(this.selectedIndex - 1, 0);
      this._renderItems();
    } else if (e.key === 'Enter' || e.key === 'Tab') {
      if (this.filteredPrompts.length > 0) {
        e.preventDefault();
        e.stopPropagation();
        this._insert(this.filteredPrompts[this.selectedIndex]);
      }
    } else if (e.key === 'Escape') {
      e.preventDefault();
      this._hide();
    }
  }

  _onMousedown(e) {
    // Close if click is outside the picker
    if (this.picker && !this.picker.contains(e.target)) {
      this._hide();
    }
  }

  // ─── Visibility ─────────────────────────────────────────────────────────────

  _show(inputEl) {
    if (!this.picker) this._buildDOM();

    const rect = inputEl.getBoundingClientRect();
    const width = Math.min(Math.max(rect.width, 360), 520);

    this.picker.style.display = 'flex';
    this.picker.style.left = `${Math.max(8, rect.left)}px`;
    this.picker.style.width = `${width}px`;

    // Prefer showing above the input; fall back to below
    const spaceAbove = rect.top - 16;
    if (spaceAbove > 280) {
      this.picker.style.bottom = `${window.innerHeight - rect.top + 8}px`;
      this.picker.style.top = 'auto';
    } else {
      this.picker.style.top = `${rect.bottom + 8}px`;
      this.picker.style.bottom = 'auto';
    }

    this.isVisible = true;
  }

  _hide() {
    if (this.picker) this.picker.style.display = 'none';
    this.isVisible = false;
    this.activeInput = null;
    this.searchQuery = '';
    this.selectedIndex = 0;
  }

  // ─── Filtering ──────────────────────────────────────────────────────────────

  _filter(query) {
    const q = query.toLowerCase().trim();
    this.filteredPrompts = q
      ? this.prompts.filter(
          (p) =>
            p.title.toLowerCase().includes(q) ||
            p.text.toLowerCase().includes(q) ||
            (p.category || '').toLowerCase().includes(q)
        )
      : [...this.prompts];

    this.selectedIndex = 0;
    this._renderItems();
  }

  // ─── DOM ────────────────────────────────────────────────────────────────────

  _buildDOM() {
    this.picker = document.createElement('div');
    this.picker.className = 'qp-picker';
    // data-qp marks our own elements so _onMousedown can identify them
    this.picker.setAttribute('data-qp', 'true');
    this.picker.innerHTML = `
      <div class="qp-header">
        <span class="qp-logo">⚡</span>
        <span class="qp-title">QuickPrompt</span>
        <span class="qp-hint">↑↓ navigate &nbsp;·&nbsp; Enter insert &nbsp;·&nbsp; Esc close</span>
      </div>
      <div class="qp-list" id="qp-list"></div>
    `;
    document.body.appendChild(this.picker);
  }

  _renderItems() {
    const list = this.picker.querySelector('#qp-list');
    if (!list) return;

    if (this.filteredPrompts.length === 0) {
      list.innerHTML = `<div class="qp-empty">No prompts match &ldquo;<strong>${this._esc(this.searchQuery)}</strong>&rdquo;</div>`;
      return;
    }

    list.innerHTML = this.filteredPrompts
      .map(
        (p, i) => `
        <div class="qp-item ${i === this.selectedIndex ? 'qp-selected' : ''}" data-qp-index="${i}">
          <div class="qp-item-body">
            <span class="qp-item-title">${this._esc(p.title)}</span>
            <span class="qp-item-preview">${this._esc(p.text.slice(0, 70))}${p.text.length > 70 ? '…' : ''}</span>
          </div>
          ${p.category ? `<span class="qp-badge">${this._esc(p.category)}</span>` : ''}
        </div>`
      )
      .join('');

    list.querySelectorAll('[data-qp-index]').forEach((el) => {
      // mousedown (not click) so we fire before the input loses focus
      el.addEventListener('mousedown', (e) => {
        e.preventDefault();
        this._insert(this.filteredPrompts[parseInt(el.dataset.qpIndex)]);
      });
      el.addEventListener('mousemove', () => {
        const idx = parseInt(el.dataset.qpIndex);
        if (idx !== this.selectedIndex) {
          this.selectedIndex = idx;
          this._renderItems();
        }
      });
    });

    const selected = list.querySelector('.qp-selected');
    if (selected) selected.scrollIntoView({ block: 'nearest' });
  }

  // ─── Insertion ──────────────────────────────────────────────────────────────

  _insert(prompt) {
    if (!this.activeInput) { this._hide(); return; }

    const el = this.activeInput;
    el.focus();

    // Build the new full text: everything before // + the prompt
    const triggerStr = '//' + this.searchQuery;
    const fullText = this._getText(el);
    const triggerIdx = fullText.lastIndexOf(triggerStr);
    const textBefore = triggerIdx >= 0 ? fullText.slice(0, triggerIdx) : fullText;
    const newText = textBefore + prompt.text;

    if (el.tagName === 'TEXTAREA') {
      // Simple case — plain textarea (some older ChatGPT views)
      el.value = newText;
      el.selectionStart = el.selectionEnd = newText.length;
      el.dispatchEvent(new Event('input', { bubbles: true }));
    } else {
      // contenteditable (Claude uses Tiptap, ChatGPT uses its own editor).
      // execCommand triggers React / ProseMirror's own change handlers correctly.
      document.execCommand('selectAll');
      document.execCommand('insertText', false, newText);
    }

    this._hide();
    el.focus();
  }

  // ─── Helpers ────────────────────────────────────────────────────────────────

  _isEditable(el) {
    return el && (el.getAttribute('contenteditable') === 'true' || el.tagName === 'TEXTAREA');
  }

  _getText(el) {
    return el.tagName === 'TEXTAREA' ? el.value : (el.textContent || '');
  }

  _esc(str) {
    return String(str)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }
}
