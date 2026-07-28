(function () {
    const pageSize = 12;
    const state = { books: [], query: '', category: '', availability: '', course: '', sort: 'newest', page: 1 };
    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

    function filtered() {
        const q = state.query.toLowerCase();
        let rows = state.books.filter(b => {
            if (q) {
                const haystack = `${b.title} ${b.author} ${b.category} ${(b.tags || []).join(' ')}`.toLowerCase();
                if (!haystack.includes(q)) return false;
            }
            if (state.category && b.category !== state.category) return false;
            if (state.course && b.category !== state.course) return false;
            if (state.availability === 'available' && !b.isAvailable) return false;
            if (state.availability === 'unavailable' && b.isAvailable) return false;
            return true;
        });
        rows = rows.slice().sort((a, b) => {
            switch (state.sort) {
                case 'oldest': return a.dateAdded.localeCompare(b.dateAdded);
                case 'title-asc': return a.title.localeCompare(b.title);
                case 'title-desc': return b.title.localeCompare(a.title);
                default: return b.dateAdded.localeCompare(a.dateAdded);
            }
        });
        return rows;
    }

    function bookCard(book) {
        const cover = book.coverImageUrl
            ? `<img class="book-cover-img" src="${escapeHtml(book.coverImageUrl)}" alt="Book cover of ${escapeHtml(book.title)}" loading="lazy" />`
            : `<div class="book-cover-placeholder"></div>`;
        return `<div class="col">
            <a class="book-card text-decoration-none text-reset" href="/Book/Details/${encodeURIComponent(book.id)}">
                ${cover}
                <div class="book-title">${escapeHtml(book.title)}</div>
                <div class="book-author">${escapeHtml(book.author)}</div>
                <div class="book-status d-flex align-items-center gap-2">
                    <span class="status-dot ${book.isAvailable ? 'status-available' : 'status-unavailable'}"></span>
                    <span class="status-text ${book.isAvailable ? 'text-available' : 'text-unavailable'}">${book.isAvailable ? 'Available' : 'Unavailable'}</span>
                </div>
            </a>
        </div>`;
    }

    function render() {
        const rows = filtered();
        const pages = Math.max(1, Math.ceil(rows.length / pageSize));
        state.page = Math.min(state.page, pages);
        const pageRows = rows.slice((state.page - 1) * pageSize, state.page * pageSize);

        const grid = document.getElementById('libraryGrid');
        grid.innerHTML = pageRows.length
            ? pageRows.map(bookCard).join('')
            : '<p class="text-center w-100 py-5">No books found.</p>';

        const pagination = document.getElementById('libraryPagination');
        if (rows.length <= pageSize) { pagination.innerHTML = ''; return; }

        const items = [`<li class="page-item ${state.page === 1 ? 'disabled' : ''}"><a class="page-link" href="#" data-page="prev" aria-label="Previous">&lsaquo;</a></li>`];
        for (let i = 1; i <= pages; i++) {
            items.push(`<li class="page-item ${state.page === i ? 'active' : ''}"><a class="page-link" href="#" data-page="${i}">${i}</a></li>`);
        }
        items.push(`<li class="page-item ${state.page === pages ? 'disabled' : ''}"><a class="page-link" href="#" data-page="next" aria-label="Next">&rsaquo;</a></li>`);
        pagination.innerHTML = items.join('');
    }

    async function load() {
        const response = await fetch('/Library/Data');
        state.books = response.ok ? await response.json() : [];
        render();
    }

    document.getElementById('librarySearch').addEventListener('input', e => { state.query = e.target.value.trim(); state.page = 1; render(); });
    document.getElementById('categoryFilter').addEventListener('change', e => { state.category = e.target.value; state.page = 1; render(); });
    document.getElementById('availabilityFilter').addEventListener('change', e => { state.availability = e.target.value; state.page = 1; render(); });
    document.getElementById('courseFilter').addEventListener('change', e => { state.course = e.target.value; state.page = 1; render(); });
    document.getElementById('sortFilter').addEventListener('change', e => { state.sort = e.target.value; state.page = 1; render(); });
    document.querySelector('.search-btn').addEventListener('click', () => { state.query = document.getElementById('librarySearch').value.trim(); state.page = 1; render(); });
    document.getElementById('libraryPagination').addEventListener('click', e => {
        const link = e.target.closest('a[data-page]');
        if (!link) return;
        e.preventDefault();
        const pages = Math.max(1, Math.ceil(filtered().length / pageSize));
        state.page = link.dataset.page === 'prev' ? Math.max(1, state.page - 1)
            : link.dataset.page === 'next' ? Math.min(pages, state.page + 1)
            : Number(link.dataset.page);
        render();
    });

    load();
})();
