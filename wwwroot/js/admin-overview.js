(function () {
    const state = { rows: [], query: '', overdueShown: 6, borrowersShown: 6 };
    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const formatDate = value => {
        if (!value) return '—';
        const date = new Date(`${value}T00:00:00`);
        return Number.isNaN(date.valueOf()) ? value : date.toLocaleDateString('en-US', { month: '2-digit', day: '2-digit', year: '2-digit' });
    };
    const searchable = row => [row.name, row.studentId, ...(row.bookTitles || [])].join(' ').toLowerCase();
    const matches = row => !state.query || searchable(row).includes(state.query.toLowerCase());
    const card = row => `<div class="borrower-card borrower-card--${row.status.toLowerCase().replaceAll(' ', '-')}" role="button" tabindex="0" data-student-id="${escapeHtml(row.studentId)}" aria-label="View details for ${escapeHtml(row.name)}">
        <div class="borrower-card__details"><strong>${escapeHtml(row.name || 'Unknown student')}</strong><div class="borrower-card__muted">${escapeHtml(row.studentId)}</div><div class="borrower-card__books">Books Borrowed: ${row.borrowedBooks}<br><span>${(row.bookTitles || []).map(escapeHtml).join(', ') || 'No titles provided'}</span></div></div>
        <div class="borrower-card__meta"><span class="status-badge">${escapeHtml(row.status)}</span><div>Due: <b>${formatDate(row.dueDate)}</b></div><div>Borrowed: ${formatDate(row.dateBorrowed)}</div></div>
    </div>`;
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

    const detailModalEl = document.getElementById('borrowerDetailModal');
    const detailModal = detailModalEl ? bootstrap.Modal.getOrCreateInstance(detailModalEl) : null;

    async function openBorrowerDetail(studentId) {
        const response = await fetch(`/AdminBorrowers/Detail?studentId=${encodeURIComponent(studentId)}`);
        if (!response.ok) { alert('Could not load borrower details.'); return; }
        const detail = await response.json();

        document.getElementById('bd-name').textContent = detail.name;
        document.getElementById('bd-studentid').textContent = detail.studentId;
        document.getElementById('bd-email').textContent = detail.email;

        document.getElementById('bd-loans').innerHTML = detail.loans.length
            ? `<div class="admin-table-wrap"><table class="admin-data-table"><thead><tr><th>Book</th><th>Borrowed</th><th>Due</th><th>Status</th></tr></thead><tbody>${detail.loans.map(l => `<tr><td>${escapeHtml(l.book)}</td><td>${formatDate(l.dateBorrowed)}</td><td>${formatDate(l.dueDate)}</td><td>${escapeHtml(l.status)}</td></tr>`).join('')}</tbody></table></div>`
            : '<p class="overview-empty">No borrowing history.</p>';

        detailModal?.show();
    }

    document.addEventListener('click', event => {
        const card = event.target.closest('.borrower-card');
        if (card) openBorrowerDetail(card.dataset.studentId);
    });
    document.addEventListener('keydown', event => {
        if (event.key !== 'Enter' && event.key !== ' ') return;
        const card = event.target.closest('.borrower-card');
        if (!card) return;
        event.preventDefault();
        openBorrowerDetail(card.dataset.studentId);
    });

    load();
})();
