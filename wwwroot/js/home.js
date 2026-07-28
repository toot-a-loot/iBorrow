(function () {
    const maxBooks = 5;
    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

    function bookCard(book) {
        const cover = book.coverImageUrl
            ? `<img src="${escapeHtml(book.coverImageUrl)}" alt="Book cover of ${escapeHtml(book.title)}" class="book-card__cover" loading="lazy" />`
            : '';
        const badgeClass = book.isAvailable ? 'book-card__badge--available' : 'book-card__badge--unavailable';
        const statusText = book.isAvailable ? 'Available' : 'Unavailable';
        return `<a class="book-card text-decoration-none text-reset" role="listitem" tabindex="0" aria-label="${escapeHtml(book.title)} by ${escapeHtml(book.author)}, ${statusText}" href="/Book/Details/${encodeURIComponent(book.id)}">
            <div class="book-card__cover-wrapper">${cover}</div>
            <div class="book-card__body">
                <h3 class="book-card__title">${escapeHtml(book.title)}</h3>
                <p class="book-card__author">${escapeHtml(book.author)}</p>
                <span class="book-card__badge ${badgeClass}" aria-label="Status: ${statusText}">
                    <span class="book-card__badge-dot" aria-hidden="true"></span>
                    ${statusText}
                </span>
            </div>
        </a>`;
    }

    async function load() {
        const grid = document.getElementById('homeBookGrid');
        if (!grid) return;
        const response = await fetch('/Library/Data');
        const books = response.ok ? await response.json() : [];
        const featured = books.slice().sort((a, b) => b.dateAdded.localeCompare(a.dateAdded)).slice(0, maxBooks);
        grid.innerHTML = featured.length ? featured.map(bookCard).join('') : '<p>No books available yet.</p>';
    }

    load();
})();
