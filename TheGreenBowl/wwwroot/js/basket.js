document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.updateQuantityForm').forEach(form => {
        // Intercept form submit so that pressing Enter doesn't cause a full submission
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            // If the form is submitted (via Enter), force an "update" operation.
            sendUpdateRequest(form, 'update');
        });

        const quantityInput = form.querySelector('.quantity-input');
        const incBtn = form.querySelector('.increment-btn');
        const decBtn = form.querySelector('.decrement-btn');

        // When increment button is clicked:
        incBtn.addEventListener('click', function () {
            let current = parseInt(quantityInput.value, 10) || 0;
            quantityInput.value = current + 1;
            quantityInput.dataset.previousValue = quantityInput.value; // update previous value
            sendUpdateRequest(form, 'increment');
        });

        // When decrement button is clicked:
        decBtn.addEventListener('click', function () {
            let current = parseInt(quantityInput.value, 10) || 0;
            if (current > 0) {
                quantityInput.value = current - 1;
                quantityInput.dataset.previousValue = quantityInput.value;
                sendUpdateRequest(form, 'decrement');
            }
        });

        // Listen for manual changes on the input with a debounce:
        let debounceTimeout = null;
        quantityInput.addEventListener('input', function () {
            clearTimeout(debounceTimeout);
            // Do not update if the field is empty now.
            if (this.value.trim() === "") return;
            debounceTimeout = setTimeout(() => {
                let val = parseInt(quantityInput.value, 10);
                if (!isNaN(val)) {
                    quantityInput.dataset.previousValue = val;
                    sendUpdateRequest(form, 'update');
                }
            }, 800);
        });

        // On blur, if empty, restore previous value.
        quantityInput.addEventListener('blur', function () {
            if (this.value.trim() === "") {
                this.value = this.dataset.previousValue;
            } else {
                let val = parseInt(this.value, 10);
                if (!isNaN(val) && val !== parseInt(this.dataset.previousValue, 10)) {
                    this.dataset.previousValue = val;
                    sendUpdateRequest(form, 'update');
                }
            }
        });
    });
});


// AJAX update request sends the form data to the server.
function sendUpdateRequest(form, operation) {
    const formData = new FormData(form);
    formData.set('operation', operation);

    // Use the form's action URL which should include the right handler.
    fetch(form.action, {
        method: 'POST',
        body: formData,
        credentials: 'same-origin',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.json())
        .then(data => {
            // We recieve the response from our handler..
            if (data.success) {
                // Update the overall basket total.
                const totalCell = document.getElementById('totalCell');
                if (totalCell) {
                    totalCell.textContent = parseFloat(data.newTotal)
                        .toLocaleString(undefined, { style: 'currency', currency: 'GBP' });
                }
                const row = form.closest('tr');
                if (data.removed) {
                    // If the response remove was true, we take action
                    if (row) row.parentElement.removeChild(row);
                } else {
                    // Else update the quantity and subtotal for the item
                    form.querySelector('.quantity-input').value = data.newQuantity;
                    const subtotalCell = row.querySelector('.subtotal-cell');
                    if (subtotalCell) {
                        subtotalCell.textContent = parseFloat(data.newSubtotal)
                            .toLocaleString(undefined, { style: 'currency', currency: 'GBP' });
                    }
                }
            } else {
                console.error('Update failed: ' + data.message);
            }
        })
        .catch(err => {
            console.error('Fetch error: ', err);
        });
}