(function () {
    const endpoint = '/AdminBorrowedBooks';
    const borrowedColumns = [
        ['Id', 'ID'], ['Book', 'Book ID/Name'], ['BorrowerId', 'Borrower ID'], ['BorrowerName', 'Borrower Name'],
        ['DateBorrowed', 'Date Borrowed'], ['DueDate', 'Due Date'], ['Copies', 'No. of Copies'], ['Status', 'Status']
    ];
    const returnedColumns = [
        ['Id', 'ID'], ['Book', 'Book ID/Name'], ['BorrowerName', 'Borrower Name'], ['DateBorrowed', 'Date Borrowed'],
        ['DueDate', 'Due Date'], ['Copies', 'No. of Copies'], ['Status', 'Status'], ['ProcessedBy', 'Processed By'],
        ['DateReturned', 'Date Returned'], ['ReceivedBy', 'Received By']
    ];
    const state = { borrowed: [], returned: [], borrowedEdit: null, returnedEdit: null, borrowedQuery: '', returnedQuery: '', borrowedPage: 1, returnedPage: 1, borrowedSize: 10, returnedSize: 10, sort: { borrowed: null, returned: null } };

    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const valueFor = (item, key) => item[key] ?? '';
    const input = (key, value, type = 'text', disabled = false) => `<input data-field="${key}" type="${type}" value="${escapeHtml(value)}" ${disabled ? 'disabled' : ''} required>`;
    const isNew = item => item && item.__new;

    async function refresh() {
        const response = await fetch(`${endpoint}/Data`);
        const data = await response.json();
        state.borrowed = data.borrowed || [];
        state.returned = data.returned || [];
        render('borrowed'); render('returned');
    }

    function filtered(kind) {
        const query = state[`${kind}Query`].toLowerCase();
        let rows = state[kind].filter(row => Object.values(row).some(value => String(value).toLowerCase().includes(query)));
        const sort = state.sort[kind];
        if (sort) rows.sort((a, b) => String(a[sort.key] ?? '').localeCompare(String(b[sort.key] ?? ''), undefined, { numeric: true }) * (sort.direction === 'asc' ? 1 : -1));
        return rows;
    }

    function render(kind) {
        const columns = kind === 'borrowed' ? borrowedColumns : returnedColumns;
        const container = document.getElementById(`${kind}-table`);
        const rows = filtered(kind); const size = state[`${kind}Size`]; const pages = Math.max(1, Math.ceil(rows.length / size));
        state[`${kind}Page`] = Math.min(state[`${kind}Page`], pages);
        const pageRows = rows.slice((state[`${kind}Page`] - 1) * size, state[`${kind}Page`] * size);
        const editing = state[`${kind}Edit`];
        const toolbar = `<div class="table-toolbar"><label>Show <select data-size="${kind}">${[10, 25, 50, 100].map(n => `<option ${n === size ? 'selected' : ''}>${n}</option>`).join('')}</select> entries</label><label>Search: <input data-search="${kind}" value="${escapeHtml(state[`${kind}Query`])}" aria-label="Search ${kind} books"></label></div>`;
        const head = columns.map(([key, label]) => `<th>${label}<button class="sort-button" data-sort="${kind}" data-key="${key}" aria-label="Sort by ${label}"><i class="bi bi-caret-up"></i><i class="bi bi-caret-down"></i></button></th>`).join('');
        const body = pageRows.length ? pageRows.map(row => row.__new || editing === row.Id ? editRow(kind, row, columns) : normalRow(kind, row, columns)).join('') : `<tr><td colspan="${columns.length + 1}" class="empty-cell">No entries found</td></tr>`;
        container.innerHTML = `${toolbar}<div class="admin-table-wrap"><table class="admin-data-table"><thead><tr>${head}<th>Actions</th></tr></thead><tbody>${body}</tbody></table></div><div class="table-footer"><span>Showing ${rows.length ? ((state[`${kind}Page`] - 1) * size + 1) : 0} to ${Math.min(state[`${kind}Page`] * size, rows.length)} of ${rows.length} entries</span><div class="pagination"><button data-page="${kind}" data-target="prev" ${state[`${kind}Page`] === 1 ? 'disabled' : ''}>Previous</button>${Array.from({ length: pages }, (_, i) => `<button data-page="${kind}" data-target="${i + 1}" class="${state[`${kind}Page`] === i + 1 ? 'active' : ''}">${i + 1}</button>`).join('')}<button data-page="${kind}" data-target="next" ${state[`${kind}Page`] === pages ? 'disabled' : ''}>Next</button></div></div>`;
    }

    function editRow(kind, row, columns) {
        const returned = kind === 'returned';
        return `<tr data-id="${escapeHtml(row.Id)}">${columns.map(([key]) => {
            const locked = key === 'Id' || (returned && ['Book', 'BorrowerName', 'DateBorrowed', 'DueDate', 'Copies', 'Status'].includes(key));
            const type = ['DateBorrowed', 'DueDate', 'DateReturned'].includes(key) ? 'date' : key === 'Copies' ? 'number' : 'text';
            return `<td>${locked ? escapeHtml(valueFor(row, key)) : input(key, valueFor(row, key), type, false)}</td>`;
        }).join('')}<td class="actions"><button class="admin-action-button" data-action="save" data-kind="${kind}">Enter</button>${!isNew(row) ? `<button class="admin-action-button" data-action="cancel" data-kind="${kind}">Cancel</button>` : ''}</td></tr>`;
    }
    function normalRow(kind, row, columns) {
        const buttons = kind === 'borrowed' ? `<button class="admin-action-button" data-action="return" data-id="${row.Id}">Returned</button><button class="admin-action-button" data-action="edit" data-id="${row.Id}">Edit</button>` : '';
        return `<tr data-id="${escapeHtml(row.Id)}">${columns.map(([key]) => `<td>${escapeHtml(valueFor(row, key))}</td>`).join('')}<td class="actions">${buttons}</td></tr>`;
    }

    function getEditValues(row) { const result = { ...row }; row.__new = row.__new || false; document.querySelector(`tr[data-id="${CSS.escape(row.Id)}"]`).querySelectorAll('[data-field]').forEach(field => result[field.dataset.field] = field.value.trim()); return result; }
    async function save(kind, row) {
        const item = getEditValues(row); const required = kind === 'borrowed' ? ['Book', 'BorrowerId', 'BorrowerName', 'DateBorrowed', 'DueDate', 'Copies'] : ['ProcessedBy', 'DateReturned', 'ReceivedBy'];
        if (required.some(key => !item[key]) || Number(item.Copies) <= 0) { alert('Please complete every required field with a valid value.'); return; }
        const response = await fetch(`${endpoint}/${isNew(row) ? (kind === 'borrowed' ? 'AddBorrowed' : 'AddReturned') : `EditBorrowed?id=${encodeURIComponent(row.Id)}`}`, { method: isNew(row) ? 'POST' : 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ ...item, Copies: Number(item.Copies) }) });
        if (!response.ok) { alert('The record could not be saved.'); return; }
        state[`${kind}Edit`] = null; await refresh();
    }

    document.getElementById('add-borrowed').addEventListener('click', () => { if (state.borrowedEdit) return; const row = { Id: 'new-borrowed', __new: true, Status: 'Borrowed', Copies: '' }; state.borrowed.unshift(row); state.borrowedEdit = row.Id; state.borrowedPage = 1; render('borrowed'); });
    document.addEventListener('input', event => { const search = event.target.dataset.search; if (search) { const cursor = event.target.selectionStart; state[`${search}Query`] = event.target.value; state[`${search}Page`] = 1; render(search); const next = document.querySelector(`[data-search="${search}"]`); if (next) { next.focus(); next.setSelectionRange(cursor, cursor); } } });
    document.addEventListener('change', event => { const kind = event.target.dataset.size; if (kind) { state[`${kind}Size`] = Number(event.target.value); state[`${kind}Page`] = 1; render(kind); } });
    document.addEventListener('click', async event => {
        const button = event.target.closest('button'); if (!button) return;
        if (button.dataset.sort) { const old = state.sort[button.dataset.sort]; state.sort[button.dataset.sort] = { key: button.dataset.key, direction: old?.key === button.dataset.key && old.direction === 'asc' ? 'desc' : 'asc' }; render(button.dataset.sort); return; }
        if (button.dataset.page) { const kind = button.dataset.page; const pages = Math.max(1, Math.ceil(filtered(kind).length / state[`${kind}Size`])); state[`${kind}Page`] = button.dataset.target === 'prev' ? Math.max(1, state[`${kind}Page`] - 1) : button.dataset.target === 'next' ? Math.min(pages, state[`${kind}Page`] + 1) : Number(button.dataset.target); render(kind); return; }
        const kind = button.dataset.kind; const id = button.dataset.id;
        if (button.dataset.action === 'edit') { state.borrowedEdit = id; render('borrowed'); }
        if (button.dataset.action === 'cancel') { state[`${kind}Edit`] = null; state[kind] = state[kind].filter(row => !row.__new); render(kind); }
        if (button.dataset.action === 'save') { const row = state[kind].find(item => item.Id === (kind === 'borrowed' ? state.borrowedEdit : state.returnedEdit)); if (row) await save(kind, row); }
        if (button.dataset.action === 'return') { const response = await fetch(`${endpoint}/Returned?id=${encodeURIComponent(id)}`, { method: 'POST' }); if (response.ok) { const returned = await response.json(); returned.__new = true; state.returned.unshift(returned); state.returnedEdit = returned.Id; await refresh(); state.returned.unshift(returned); state.returnedEdit = returned.Id; render('returned'); } }
    });
    refresh();
})();
