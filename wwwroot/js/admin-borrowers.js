(function () {
    const endpoint = '/AdminBorrowers';
    const columns = [['studentId', 'Student ID'], ['name', 'Name'], ['email', 'Email']];
    const pageSize = 10;
    const state = { rows: [], edit: null, query: '', page: 1, sort: null };
    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const isNew = row => row && row.__new;
    const input = (key, value, type = 'text') => `<input data-field="${key}" type="${type}" value="${escapeHtml(value)}" required>`;

    async function refresh() {
        const response = await fetch(`${endpoint}/Data`);
        state.rows = await response.json();
        render();
    }
    function filtered() {
        const query = state.query.toLowerCase();
        const rows = state.rows.filter(row => Object.values(row).some(value => String(value).toLowerCase().includes(query)));
        if (state.sort) rows.sort((a, b) => String(a[state.sort.key] ?? '').localeCompare(String(b[state.sort.key] ?? ''), undefined, { numeric: true }) * (state.sort.direction === 'asc' ? 1 : -1));
        return rows;
    }
    function render() {
        const rows = filtered(); const pages = Math.max(1, Math.ceil(rows.length / pageSize)); state.page = Math.min(state.page, pages);
        const pageRows = rows.slice((state.page - 1) * pageSize, state.page * pageSize);
        const toolbar = `<div class="table-toolbar"><label>Search: <input id="borrower-search" value="${escapeHtml(state.query)}" aria-label="Search borrowers"></label></div>`;
        const head = columns.map(([key, label]) => `<th>${label}<button class="sort-button" data-sort="${key}" aria-label="Sort by ${label}"><i class="bi bi-caret-up"></i><i class="bi bi-caret-down"></i></button></th>`).join('');
        const renderedRows = pageRows.map(row => row.__new || state.edit === row.libraryId ? editRow(row) : normalRow(row)).join('');
        const hasSavedRows = state.rows.some(row => !row.__new);
        const emptyRow = (!pageRows.length || !hasSavedRows) ? `<tr><td colspan="${columns.length + 1}" class="empty-cell">No Entries Found</td></tr>` : '';
        const body = renderedRows + emptyRow;
        document.getElementById('borrowers-table').innerHTML = `${toolbar}<div class="admin-table-wrap"><table class="admin-data-table"><thead><tr>${head}<th>Actions</th></tr></thead><tbody>${body}</tbody></table></div><div class="table-footer"><span>Showing ${rows.length ? ((state.page - 1) * pageSize + 1) : 0} to ${Math.min(state.page * pageSize, rows.length)} of ${rows.length} entries</span><div class="pagination"><button data-page="prev" ${state.page === 1 ? 'disabled' : ''}>Previous</button>${Array.from({ length: pages }, (_, i) => `<button data-page="${i + 1}" class="${state.page === i + 1 ? 'active' : ''}">${i + 1}</button>`).join('')}<button data-page="next" ${state.page === pages ? 'disabled' : ''}>Next</button></div></div>`;
    }
    function editRow(row) {
        return `<tr data-id="${escapeHtml(row.libraryId)}"><td>${input('studentId', row.studentId)}</td><td>${input('name', row.name)}</td><td>${input('email', row.email, 'email')}</td><td class="actions"><button class="admin-action-button" data-action="save" ${isNew(row) ? 'disabled' : ''}>${isNew(row) ? 'Enter' : 'Save'}</button>${!isNew(row) ? '<button class="admin-action-button" data-action="cancel">Cancel</button>' : ''}</td></tr>`;
    }
    function normalRow(row) { return `<tr data-id="${escapeHtml(row.libraryId)}">${columns.map(([key]) => `<td>${escapeHtml(row[key])}</td>`).join('')}<td class="actions"><button class="admin-action-button" data-action="edit" data-id="${escapeHtml(row.libraryId)}">Edit</button><button class="admin-action-button" data-action="delete" data-id="${escapeHtml(row.libraryId)}">Delete</button></td></tr>`; }
    function values(row) { const result = { ...row }; document.querySelector(`tr[data-id="${CSS.escape(row.libraryId)}"]`).querySelectorAll('[data-field]').forEach(field => result[field.dataset.field] = field.value.trim()); return result; }
    function valid(row) { return row.studentId && row.name && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(row.email); }
    async function save(row) {
        const item = values(row); if (!valid(item)) { alert('Enter a name, student ID, and valid email.'); return; }
        const response = await fetch(`${endpoint}/${isNew(row) ? 'Add' : `Edit?id=${encodeURIComponent(row.libraryId)}`}`, { method: isNew(row) ? 'POST' : 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(item) });
        if (!response.ok) { alert('The borrower could not be saved.'); return; }
        state.edit = null; await refresh();
    }
    async function remove(id) {
        if (!confirm('Delete this borrower? This will also remove their account and cannot be undone.')) return;
        const response = await fetch(`${endpoint}/Delete?id=${encodeURIComponent(id)}`, { method: 'DELETE' });
        if (!response.ok) { alert('The borrower could not be deleted.'); return; }
        await refresh();
    }
    document.getElementById('add-borrower').addEventListener('click', () => { if (state.edit) return; const row = { libraryId: 'new-borrower', studentId: '', name: '', email: '', __new: true }; state.rows.unshift(row); state.edit = row.libraryId; state.page = 1; render(); });
    document.addEventListener('input', event => {
        if (event.target.id === 'borrower-search') { const cursor = event.target.selectionStart; state.query = event.target.value; state.page = 1; render(); const next = document.getElementById('borrower-search'); next.focus(); next.setSelectionRange(cursor, cursor); return; }
        if (!event.target.dataset.field) return;
        const row = event.target.closest('tr'); const fields = Object.fromEntries([...row.querySelectorAll('[data-field]')].map(field => [field.dataset.field, field.value.trim()]));
        row.querySelector('[data-action="save"]').disabled = !valid(fields);
    });
    document.addEventListener('click', async event => { const button = event.target.closest('button'); if (!button) return;
        if (button.dataset.sort) { const old = state.sort; state.sort = { key: button.dataset.sort, direction: old?.key === button.dataset.sort && old.direction === 'asc' ? 'desc' : 'asc' }; render(); return; }
        if (button.dataset.page) { const pages = Math.max(1, Math.ceil(filtered().length / pageSize)); state.page = button.dataset.page === 'prev' ? Math.max(1, state.page - 1) : button.dataset.page === 'next' ? Math.min(pages, state.page + 1) : Number(button.dataset.page); render(); return; }
        if (button.dataset.action === 'edit') { state.edit = button.dataset.id; render(); return; }
        if (button.dataset.action === 'cancel') { state.edit = null; state.rows = state.rows.filter(row => !row.__new); render(); return; }
        if (button.dataset.action === 'delete') { await remove(button.dataset.id); return; }
        if (button.dataset.action === 'save') { const row = state.rows.find(item => item.libraryId === state.edit); if (row) await save(row); }
    });
    refresh();
})();
