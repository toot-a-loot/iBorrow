(function () {
    const state = { rows: [], query: '', overdueShown: 6, borrowersShown: 6 };
    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const formatDate = value => {
        if (!value) return '—';
        const date = new Date(`${value}T00:00:00`);
        return Number.isNaN(date.valueOf()) ? value : date.toLocaleDateString('en-US', { month: '2-digit', day: '2-digit', year: '2-digit' });
    };
    const searchable = row => [row.libraryId, row.name, row.studentId, row.course, ...(row.bookTitles || [])].join(' ').toLowerCase();
    const matches = row => !state.query || searchable(row).includes(state.query.toLowerCase());
    const card = row => `<a class="borrower-card borrower-card--${row.status.toLowerCase().replaceAll(' ', '-')}" href="/AdminBorrowers/Profile?studentId=${encodeURIComponent(row.studentId)}" aria-label="Open profile for ${escapeHtml(row.name)}">
        <div class="borrower-card__details"><div class="borrower-card__library">${escapeHtml(row.libraryId)}</div><strong>${escapeHtml(row.name || 'Unknown student')}</strong><div class="borrower-card__muted">${escapeHtml(row.studentId)}</div><div class="borrower-card__muted">${escapeHtml(row.course || 'Course not provided')}</div><div class="borrower-card__books">Books Borrowed: ${row.borrowedBooks}<br><span>${(row.bookTitles || []).map(escapeHtml).join(', ') || 'No titles provided'}</span></div></div>
        <div class="borrower-card__meta"><span class="status-badge">${escapeHtml(row.status)}</span><div>Due: <b>${formatDate(row.dueDate)}</b></div><div>Borrowed: ${formatDate(row.dateBorrowed)}</div></div>
    </a>`;
    function sectionRows(kind) {
        const rows = state.rows.filter(matches);
        return kind === 'overdue' ? rows.filter(row => row.status === 'Overdue' || row.status === 'Nearly Due') : rows;
    }
    function render(kind) {
        const rows = sectionRows(kind), shown = state[`${kind}Shown`], container = document.getElementById(kind === 'overdue' ? 'overdue-cards' : 'borrower-cards');
        container.innerHTML = rows.slice(0, shown).map(card).join('') || '<p class="overview-empty">No borrowers found.</p>';
        const more = document.getElementById(`${kind}-more`), allLoaded = shown >= rows.length;
        more.hidden = allLoaded || rows.length === 0;
        more.disabled = false;
        more.closest('.overview-section').classList.toggle('is-all-loaded', allLoaded && rows.length > 0);
    }
    function renderAll() { render('overdue'); render('borrowers'); }
    async function load() {
        const response = await fetch('/AdminBorrowers/Overview');
        if (!response.ok) return;
        state.rows = await response.json();
        renderAll();
    }
    document.getElementById('borrower-overview-search').addEventListener('input', event => { state.query = event.target.value.trim(); state.overdueShown = 6; state.borrowersShown = 6; renderAll(); });
    document.getElementById('overdue-more').addEventListener('click', () => { state.overdueShown += 9; render('overdue'); });
    document.getElementById('borrowers-more').addEventListener('click', () => { state.borrowersShown += 9; render('borrowers'); });
    load();
})();
