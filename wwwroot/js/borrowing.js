(function () {
    const state = { overdue: [], dueThisWeek: [], returned: [], query: '', status: '', sort: 'due-soonest' };
    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const formatDate = value => {
        if (!value) return '—';
        const date = new Date(`${value}T00:00:00`);
        return Number.isNaN(date.valueOf()) ? value : date.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
    };
    const cover = (entry, extraClass) => entry.coverImageUrl
        ? `<img src="${escapeHtml(entry.coverImageUrl)}" alt="Book cover of ${escapeHtml(entry.title)}" class="${extraClass}" loading="lazy" />`
        : `<div class="borrowed-card__cover-placeholder" aria-hidden="true"></div>`;

    function matchesSearch(entry) {
        if (!state.query) return true;
        const haystack = `${entry.title} ${entry.author}`.toLowerCase();
        return haystack.includes(state.query.toLowerCase());
    }

    function sortEntries(entries, byDue) {
        const sorted = entries.slice();
        if (state.sort === 'title') {
            sorted.sort((a, b) => a.title.localeCompare(b.title));
        } else if (byDue) {
            sorted.sort((a, b) => (a.dueDate || '').localeCompare(b.dueDate || '') * (state.sort === 'due-latest' ? -1 : 1));
        } else {
            sorted.sort((a, b) => (b.dateReturned || '').localeCompare(a.dateReturned || ''));
        }
        return sorted;
    }

    function overdueCard(entry) {
        const days = Math.abs(entry.daysRemaining);
        return `<li class="borrowed-card borrowed-card--overdue" role="listitem" tabindex="0" aria-label="${escapeHtml(entry.title)} by ${escapeHtml(entry.author)}, Overdue">
            <div class="borrowed-card__cover-wrapper">${cover(entry, 'borrowed-card__cover')}</div>
            <div class="borrowed-card__info">
                <h3 class="borrowed-card__title">${escapeHtml(entry.title)}</h3>
                <p class="borrowed-card__author">${escapeHtml(entry.author)}</p>
                <span class="borrowed-badge borrowed-badge--overdue">Overdue</span>
            </div>
            <div class="borrowed-card__dates">
                <span class="borrowed-card__date-block"><span class="borrowed-card__date-label">Borrowed</span><span class="borrowed-card__date-value"><i class="bi bi-calendar3" aria-hidden="true"></i> ${formatDate(entry.dateBorrowed)}</span></span>
                <span class="borrowed-card__date-block"><span class="borrowed-card__date-label">Due Date</span><span class="borrowed-card__date-value"><i class="bi bi-calendar3" aria-hidden="true"></i> ${formatDate(entry.dueDate)}</span></span>
            </div>
            <div class="borrowed-card__chip borrowed-card__chip--overdue">
                <span class="borrowed-card__chip-main">${days} day${days === 1 ? '' : 's'} overdue</span>
                <span class="borrowed-card__chip-sub">Due on ${formatDate(entry.dueDate)}</span>
            </div>
        </li>`;
    }

    function dueSoonCard(entry) {
        const days = entry.daysRemaining;
        return `<li class="borrowed-card borrowed-card--due-soon" role="listitem" tabindex="0" aria-label="${escapeHtml(entry.title)} by ${escapeHtml(entry.author)}, Due Soon">
            <div class="borrowed-card__cover-wrapper">${cover(entry, 'borrowed-card__cover')}</div>
            <div class="borrowed-card__info">
                <h3 class="borrowed-card__title">${escapeHtml(entry.title)}</h3>
                <p class="borrowed-card__author">${escapeHtml(entry.author)}</p>
                <span class="borrowed-badge borrowed-badge--due-soon">Due Soon</span>
            </div>
            <div class="borrowed-card__dates">
                <span class="borrowed-card__date-block"><span class="borrowed-card__date-label">Borrowed</span><span class="borrowed-card__date-value"><i class="bi bi-calendar3" aria-hidden="true"></i> ${formatDate(entry.dateBorrowed)}</span></span>
                <span class="borrowed-card__date-block"><span class="borrowed-card__date-label">Due Date</span><span class="borrowed-card__date-value"><i class="bi bi-calendar3" aria-hidden="true"></i> ${formatDate(entry.dueDate)}</span></span>
            </div>
            <div class="borrowed-card__chip borrowed-card__chip--due-soon">
                <span class="borrowed-card__chip-main">${days} day${days === 1 ? '' : 's'} left</span>
                <span class="borrowed-card__chip-sub">Due on ${formatDate(entry.dueDate)}</span>
            </div>
        </li>`;
    }

    function returnedCard(entry) {
        return `<li class="returned-item" role="listitem" tabindex="0" aria-label="${escapeHtml(entry.title)} by ${escapeHtml(entry.author)}, Returned">
            <div class="returned-item__cover-wrapper">${cover(entry, 'borrowed-card__cover')}</div>
            <div class="returned-item__info">
                <h3 class="returned-item__title">${escapeHtml(entry.title)}</h3>
                <p class="returned-item__author">${escapeHtml(entry.author)}</p>
            </div>
            <div class="borrowed-card__dates">
                <span class="borrowed-card__date-block"><span class="borrowed-card__date-label">Borrowed</span><span class="borrowed-card__date-value"><i class="bi bi-calendar3" aria-hidden="true"></i> ${formatDate(entry.dateBorrowed)}</span></span>
                <span class="borrowed-card__date-block"><span class="borrowed-card__date-label">Returned</span><span class="borrowed-card__date-value"><i class="bi bi-calendar3" aria-hidden="true"></i> ${formatDate(entry.dateReturned)}</span></span>
            </div>
            <span class="borrowed-badge borrowed-badge--returned">Returned</span>
        </li>`;
    }

    function render() {
        const showOverdue = !state.status || state.status === 'overdue';
        const showDueWeek = !state.status || state.status === 'due-soon';
        const showReturned = !state.status || state.status === 'returned';

        const overdue = showOverdue ? sortEntries(state.overdue.filter(matchesSearch), true) : [];
        const dueWeek = showDueWeek ? sortEntries(state.dueThisWeek.filter(matchesSearch), true) : [];
        const returned = showReturned ? sortEntries(state.returned.filter(matchesSearch), false) : [];

        document.getElementById('overdue-items').innerHTML = overdue.map(overdueCard).join('') || '<p class="text-muted">No overdue books.</p>';
        document.getElementById('due-week-items').innerHTML = dueWeek.map(dueSoonCard).join('') || '<p class="text-muted">No books due this week.</p>';
        document.getElementById('returned-items').innerHTML = returned.map(returnedCard).join('') || '<p class="text-muted">No returned books in the past 3 months.</p>';

        document.getElementById('overdue-count').textContent = `(${overdue.length})`;
        document.getElementById('due-week-count').textContent = `(${dueWeek.length})`;

        document.querySelector('[aria-labelledby="overdue-heading"]').hidden = !showOverdue;
        document.querySelector('[aria-labelledby="due-week-heading"]').hidden = !showDueWeek;
        document.getElementById('returned-section').hidden = !showReturned;
    }

    async function load() {
        const response = await fetch('/Borrowing/Data');
        if (!response.ok) return;
        const data = await response.json();

        state.overdue = data.overdue || [];
        state.dueThisWeek = data.dueThisWeek || [];
        state.returned = data.returned || [];

        document.getElementById('stat-active').textContent = data.activeCount ?? 0;
        document.getElementById('stat-overdue').textContent = data.overdueCount ?? 0;
        document.getElementById('stat-due').textContent = data.dueThisWeekCount ?? 0;
        document.getElementById('stat-returned').textContent = data.returnedCount ?? 0;

        render();
    }

    document.getElementById('borrowSearch').addEventListener('input', e => { state.query = e.target.value.trim(); render(); });
    document.getElementById('statusSelect').addEventListener('change', e => { state.status = e.target.value; render(); });
    document.getElementById('borrowedSortSelect').addEventListener('change', e => { state.sort = e.target.value; render(); });

    load();
})();
