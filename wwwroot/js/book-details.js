(function () {
    const confirmBtn = document.getElementById('confirmBorrowBtn');
    const borrowModalEl = document.getElementById('borrowModal');
    const claimModalEl = document.getElementById('claimModal');
    if (!confirmBtn || !borrowModalEl || !claimModalEl) return;

    const borrowModal = bootstrap.Modal.getOrCreateInstance(borrowModalEl);
    const claimModal = bootstrap.Modal.getOrCreateInstance(claimModalEl);

    confirmBtn.addEventListener('click', async () => {
        const bookId = confirmBtn.dataset.bookId;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        confirmBtn.disabled = true;

        try {
            const response = await fetch(`/Book/Borrow/${encodeURIComponent(bookId)}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `__RequestVerificationToken=${encodeURIComponent(token || '')}`
            });

            if (!response.ok) {
                const message = await response.text();
                alert(message || 'This book could not be reserved.');
                confirmBtn.disabled = false;
                return;
            }

            borrowModal.hide();
            claimModal.show();
        } catch {
            alert('A network error occurred. Please try again.');
            confirmBtn.disabled = false;
        }
    });

    claimModalEl.addEventListener('hidden.bs.modal', () => {
        location.reload();
    });
})();
