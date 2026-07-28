/* admin-add-books.js — Add Books admin page
 * Pattern mirrors admin-overview.js / admin-borrowed-books.js (IIFE, no framework)
 */
(function () {
    'use strict';

    /* ── Constants ──────────────────────────────────────────────────────── */
    const CATEGORIES = ['Software Engineering', 'Game Development', 'Multimedia Arts', 'Real Estate', 'Filipiniana'];
    const INITIAL_ROWS = 1;   // rows shown before "See More"
    const EXTRA_ROWS   = 2;   // rows added per "See More" click
    const COLS         = computeCols(); // columns visible in the grid

    /* ── State ──────────────────────────────────────────────────────────── */
    const state = {
        allBooks: [],          // BookItemDto[]
        tags: [],              // string[]
        query: '',
        activeCategories: new Set(),
        activeTags: new Set(),
        // Per-category shown row count
        shownRows: Object.fromEntries(CATEGORIES.map(c => [c, INITIAL_ROWS])),
        // Search results shown count (rows)
        searchShownRows: INITIAL_ROWS,
        // Selected tags in modal
        selectedTags: [],
    };

    /* ── Helpers ────────────────────────────────────────────────────────── */
    const esc = v => String(v ?? '').replace(/[&<>"']/g, c =>
        ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

    function computeCols() {
        // rough estimate — grid uses auto-fill 95px; recalculate on render
        const contentWidth = document.getElementById('add-books-main')?.clientWidth ?? 900;
        const bodyPad = 48;
        return Math.max(1, Math.floor((contentWidth - bodyPad) / (95 + 14)));
    }

    function bookCard(book) {
        const avail = book.isAvailable;
        const coverHtml = book.coverImageUrl
            ? `<img class="ab-book-cover" src="${esc(book.coverImageUrl)}" alt="${esc(book.title)} cover" loading="lazy" />`
            : `<div class="ab-book-cover-placeholder" aria-hidden="true"></div>`;
        return `<div class="ab-book-card">
            ${coverHtml}
            <div class="ab-book-info">
                <div class="ab-book-title" title="${esc(book.title)}">${esc(book.title)}</div>
                <div class="ab-book-author" title="${esc(book.author)}">${esc(book.author)}</div>
                <div class="ab-book-status">
                    <span class="ab-status-dot ${avail ? 'available' : 'unavailable'}"></span>
                    <span class="ab-status-text ${avail ? 'available' : 'unavailable'}">${avail ? 'Available' : 'Not Available'}</span>
                </div>
            </div>
        </div>`;
    }

    function renderGrid(containerId, moreBtnId, books, shownRows) {
        const container = document.getElementById(containerId);
        const moreBtn   = document.getElementById(moreBtnId);
        if (!container) return;

        const cols  = Math.max(1, computeCols());
        const limit = shownRows * cols;

        if (!books.length) {
            container.innerHTML = '<p class="ab-empty">No books found.</p>';
            if (moreBtn) moreBtn.hidden = true;
            return;
        }

        container.innerHTML = books.slice(0, limit).map(bookCard).join('');

        if (!moreBtn) return;

        const hasMore = books.length > limit;
        const allVisible = shownRows > INITIAL_ROWS;

        if (hasMore) {
            moreBtn.hidden = false;
            moreBtn.innerHTML = 'See More <span aria-hidden="true">→</span>';
        } else if (allVisible) {
            moreBtn.hidden = false;
            moreBtn.textContent = 'See Less';
        } else {
            moreBtn.hidden = true;
        }

        // scrollable once we've hit 3 rows and there are still more
        const section = container.closest('.ab-category-section');
        if (section) {
            section.classList.toggle('is-scrollable', !hasMore && shownRows >= 3);
        }
    }

    /* ── Filtering ──────────────────────────────────────────────────────── */
    function filtered(books) {
        const q    = state.query.toLowerCase();
        const cats = state.activeCategories;
        const tags = state.activeTags;
        return books.filter(b => {
            if (q) {
                const haystack = `${b.title} ${b.author} ${b.category} ${(b.tags || []).join(' ')}`.toLowerCase();
                if (!haystack.includes(q)) return false;
            }
            if (cats.size > 0 && !cats.has(b.category)) return false;
            if (tags.size > 0 && !b.tags?.some(t => tags.has(t))) return false;
            return true;
        });
    }

    /* ── Rendering ──────────────────────────────────────────────────────── */
    function renderAll() {
        const q        = state.query;
        const hasFilters = state.activeCategories.size > 0 || state.activeTags.size > 0;
        const searching  = q.length > 0 || hasFilters;

        // Search Results section
        const srSection = document.getElementById('ab-search-results-section');
        if (srSection) srSection.hidden = !searching;

        if (searching) {
            const results = filtered(state.allBooks);
            renderGrid('ab-search-results', 'ab-search-more', results, state.searchShownRows);
        }

        // Category sections
        CATEGORIES.forEach(cat => {
            const slugBase = cat.replace(/\s+/g, '-').toLowerCase();
            const booksInCat = state.allBooks.filter(b => b.category === cat);
            const visible    = searching ? filtered(booksInCat) : booksInCat;
            renderGrid(`ab-grid-${slugBase}`, `ab-more-${slugBase}`, visible, state.shownRows[cat]);
        });
    }

    /* ── Data loading ───────────────────────────────────────────────────── */
    async function loadBooks() {
        try {
            const res = await fetch('/AdminAddBooks/Books');
            if (!res.ok) return;
            state.allBooks = await res.json();
            renderAll();
        } catch { /* network error */ }
    }

    async function loadTags() {
        try {
            const res = await fetch('/AdminAddBooks/Tags');
            if (!res.ok) return;
            state.tags = await res.json();
            renderFilterTags();
        } catch { /* network error */ }
    }

    /* ── Filter panel tags ──────────────────────────────────────────────── */
    function renderFilterTags() {
        const container = document.getElementById('ab-tag-filters');
        if (!container) return;
        container.innerHTML = state.tags.map(tag => {
            const id = `ftag-${esc(tag.replace(/\s+/g, '-').toLowerCase())}`;
            return `<label class="ab-check-label" for="${id}">
                <input type="checkbox" id="${id}" class="ab-tag-check" value="${esc(tag)}" />
                ${esc(tag)}
            </label>`;
        }).join('');
        // Re-attach listeners
        container.querySelectorAll('.ab-tag-check').forEach(cb => {
            cb.addEventListener('change', () => {
                if (cb.checked) state.activeTags.add(cb.value);
                else state.activeTags.delete(cb.value);
                state.searchShownRows = INITIAL_ROWS;
                CATEGORIES.forEach(c => state.shownRows[c] = INITIAL_ROWS);
                renderAll();
            });
        });
    }

    /* ── Search input ───────────────────────────────────────────────────── */
    document.getElementById('ab-search')?.addEventListener('input', e => {
        state.query = e.target.value.trim();
        state.searchShownRows = INITIAL_ROWS;
        CATEGORIES.forEach(c => state.shownRows[c] = INITIAL_ROWS);
        renderAll();
    });

    /* ── Filter checkbox — categories ───────────────────────────────────── */
    document.getElementById('ab-cat-filters')?.addEventListener('change', e => {
        const cb = e.target.closest('.ab-cat-check');
        if (!cb) return;
        if (cb.checked) state.activeCategories.add(cb.value);
        else state.activeCategories.delete(cb.value);
        state.searchShownRows = INITIAL_ROWS;
        CATEGORIES.forEach(c => state.shownRows[c] = INITIAL_ROWS);
        renderAll();
    });

    /* ── Filter toggle button ───────────────────────────────────────────── */
    const filterBtn   = document.getElementById('ab-filter-btn');
    const filterPanel = document.getElementById('ab-filter-panel');
    filterBtn?.addEventListener('click', () => {
        const isHidden = filterPanel.hidden;
        filterPanel.hidden = !isHidden;
        filterBtn.setAttribute('aria-expanded', String(isHidden));
    });

    /* ── See More / See Less ────────────────────────────────────────────── */
    document.addEventListener('click', e => {
        const btn = e.target.closest('.ab-see-more');
        if (!btn) return;

        const id = btn.id;

        if (id === 'ab-search-more') {
            const cols    = Math.max(1, computeCols());
            const results = filtered(state.allBooks);
            if (btn.textContent.trim().startsWith('See More')) {
                state.searchShownRows += EXTRA_ROWS;
            } else {
                state.searchShownRows = INITIAL_ROWS;
            }
            renderGrid('ab-search-results', 'ab-search-more', results, state.searchShownRows);
            return;
        }

        // Category see-more
        const catSlug = id.replace('ab-more-', '');
        const cat = CATEGORIES.find(c => c.replace(/\s+/g, '-').toLowerCase() === catSlug);
        if (!cat) return;

        if (btn.textContent.trim().startsWith('See More')) {
            state.shownRows[cat] += EXTRA_ROWS;
        } else {
            state.shownRows[cat] = INITIAL_ROWS;
        }
        const booksInCat = state.allBooks.filter(b => b.category === cat);
        const visible = (state.query || state.activeCategories.size || state.activeTags.size)
            ? filtered(booksInCat) : booksInCat;
        const slugBase = cat.replace(/\s+/g, '-').toLowerCase();
        renderGrid(`ab-grid-${slugBase}`, `ab-more-${slugBase}`, visible, state.shownRows[cat]);
    });

    /* ══════════════════════════════════════════════════════════════════════
     *  MODAL
     * ════════════════════════════════════════════════════════════════════ */
    const overlay   = document.getElementById('ab-modal-overlay');
    const form      = document.getElementById('ab-book-form');
    const submitBtn = document.getElementById('ab-submit-btn');

    function openModal() {
        overlay.hidden = false;
        document.body.style.overflow = 'hidden';
        resetModal();
        form.querySelector('#ab-f-title')?.focus();
    }

    function closeModal() {
        overlay.hidden = true;
        document.body.style.overflow = '';
    }

    function resetModal() {
        form.reset();
        state.selectedTags = [];
        renderTagChips();
        renderTagDropdown('');
        document.getElementById('ab-cover-preview').innerHTML =
            '<span class="ab-cover-placeholder-text">Image Preview</span>';
        document.getElementById('ab-cover-input').value = '';
        updateSubmitState();
    }

    // Open
    document.getElementById('ab-add-btn')?.addEventListener('click', openModal);
    // Back / Cancel
    document.getElementById('ab-modal-back')?.addEventListener('click', closeModal);
    document.getElementById('ab-cancel-btn')?.addEventListener('click', closeModal);
    // Do NOT close on overlay click (per spec)

    /* ── Submit validation ──────────────────────────────────────────────── */
    function updateSubmitState() {
        const title    = form.querySelector('#ab-f-title').value.trim();
        const author   = form.querySelector('#ab-f-author').value.trim();
        const category = form.querySelector('#ab-f-category').value;
        const synopsis = form.querySelector('#ab-f-synopsis').value.trim();
        const valid    = title && author && category && synopsis;
        submitBtn.disabled = !valid;
        submitBtn.classList.toggle('ab-submit-active', !!valid);
    }

    ['#ab-f-title','#ab-f-author','#ab-f-category','#ab-f-synopsis'].forEach(sel => {
        form.querySelector(sel)?.addEventListener('input', updateSubmitState);
        form.querySelector(sel)?.addEventListener('change', updateSubmitState);
    });

    /* ── Image upload & preview ─────────────────────────────────────────── */
    document.getElementById('ab-upload-btn')?.addEventListener('click', () => {
        document.getElementById('ab-cover-input').click();
    });

    document.getElementById('ab-cover-input')?.addEventListener('change', e => {
        const file = e.target.files?.[0];
        if (!file) return;

        const allowed = ['image/png','image/jpeg','image/webp'];
        if (!allowed.includes(file.type)) {
            alert('Please select a PNG, JPG, JPEG or WEBP image.');
            e.target.value = '';
            return;
        }
        if (file.size > 5 * 1024 * 1024) {
            alert('Image must be smaller than 5 MB.');
            e.target.value = '';
            return;
        }

        const reader = new FileReader();
        reader.onload = ev => {
            const preview = document.getElementById('ab-cover-preview');
            preview.innerHTML = `<img src="${ev.target.result}" alt="Cover preview" />`;
        };
        reader.readAsDataURL(file);
    });

    /* ── Tag multi-select ───────────────────────────────────────────────── */
    const tagInput    = document.getElementById('ab-f-tags-input');
    const tagDropdown = document.getElementById('ab-tag-dropdown');
    const tagChips    = document.getElementById('ab-tag-chips');

    function renderTagChips() {
        tagChips.innerHTML = state.selectedTags.map(t =>
            `<span class="ab-tag-chip">${esc(t)}<button type="button" data-tag="${esc(t)}" aria-label="Remove tag ${esc(t)}">×</button></span>`
        ).join('');
    }

    function renderTagDropdown(query) {
        const q = query.toLowerCase();
        const available = state.tags.filter(t =>
            !state.selectedTags.includes(t) &&
            (!q || t.toLowerCase().includes(q))
        );

        let html = available.map(t =>
            `<div class="ab-tag-option" data-tag="${esc(t)}">${esc(t)}</div>`
        ).join('');

        // Create new tag option
        const trimmed = query.trim();
        const exists  = state.tags.some(t => t.toLowerCase() === trimmed.toLowerCase());
        const already = state.selectedTags.some(t => t.toLowerCase() === trimmed.toLowerCase());
        if (trimmed && !exists && !already) {
            html += `<div class="ab-tag-option ab-tag-option-create" data-create="${esc(trimmed)}">Create "${esc(trimmed)}"</div>`;
        }

        tagDropdown.innerHTML = html || '<div class="ab-tag-option" style="color:#aaa;cursor:default">No tags</div>';
        tagDropdown.hidden = !available.length && !trimmed;
    }

    tagInput?.addEventListener('focus', () => { renderTagDropdown(tagInput.value); tagDropdown.hidden = false; });
    tagInput?.addEventListener('input', () => renderTagDropdown(tagInput.value));

    tagDropdown?.addEventListener('mousedown', async e => {
        e.preventDefault(); // keep focus on input
        const opt = e.target.closest('.ab-tag-option');
        if (!opt) return;

        let tag = opt.dataset.tag;

        if (opt.dataset.create) {
            tag = opt.dataset.create.trim();
            // Persist the new tag server-side
            try {
                await fetch('/AdminAddBooks/AddTag', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ tag })
                });
                if (!state.tags.includes(tag)) {
                    state.tags.push(tag);
                    renderFilterTags();
                }
            } catch { /* ignore */ }
        }

        if (tag && !state.selectedTags.includes(tag)) {
            state.selectedTags.push(tag);
            renderTagChips();
        }
        tagInput.value = '';
        renderTagDropdown('');
        tagDropdown.hidden = true;
    });

    // Remove chip
    tagChips?.addEventListener('click', e => {
        const btn = e.target.closest('button[data-tag]');
        if (!btn) return;
        state.selectedTags = state.selectedTags.filter(t => t !== btn.dataset.tag);
        renderTagChips();
    });

    // Close dropdown on outside click
    document.addEventListener('click', e => {
        if (!e.target.closest('.ab-tag-select-wrap')) {
            tagDropdown.hidden = true;
        }
    });

    /* ── Form submit ────────────────────────────────────────────────────── */
    form?.addEventListener('submit', async e => {
        e.preventDefault();
        submitBtn.disabled = true;
        submitBtn.textContent = 'Saving…';

        const coverFile = document.getElementById('ab-cover-input')?.files?.[0];
        const fd = new FormData();
        fd.append('title',       form.querySelector('#ab-f-title').value.trim());
        fd.append('author',      form.querySelector('#ab-f-author').value.trim());
        fd.append('category',    form.querySelector('#ab-f-category').value);
        fd.append('synopsis',    form.querySelector('#ab-f-synopsis').value.trim());
        fd.append('totalCopies', form.querySelector('#ab-f-copies').value || '1');
        fd.append('tags',        state.selectedTags.join(','));
        if (coverFile) fd.append('coverImage', coverFile);

        try {
            const res = await fetch('/AdminAddBooks/Add', { method: 'POST', body: fd });
            if (!res.ok) {
                const msg = await res.text();
                alert(`Could not save book: ${msg}`);
                updateSubmitState();
                return;
            }
            closeModal();
            await loadBooks();   // refresh all sections
            await loadTags();    // in case new tags were created
        } catch (err) {
            alert('A network error occurred. Please try again.');
            updateSubmitState();
        }
    });

    /* ── Init ───────────────────────────────────────────────────────────── */
    loadBooks();
    loadTags();

})();
