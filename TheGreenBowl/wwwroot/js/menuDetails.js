document.addEventListener('DOMContentLoaded', function() {
    // Menu item row click behavior
    const menuRows = document.querySelectorAll('.menuItem-row');
    menuRows.forEach(row => {
        row.addEventListener('click', function(e) {
            // Prevent navigation if clicking on an action button or form element
            if (e.target.closest('a') || e.target.closest('button') || e.target.closest('input')) {
                return;
            }
            const itemID = this.getAttribute('data-item-id');
            window.location.href = './Item/ItemDetails?id=' + itemID;
        });
    });

    // Quantity stepper functionality
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

        // Store initial value
        quantityInput.dataset.previousValue = quantityInput.value;

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

// Update the sendUpdateRequest function in your Scripts section
function sendUpdateRequest(form, operation) {
    const formData = new FormData(form);
    formData.set('operation', operation);
    const itemId = formData.get('itemId');
    const returnUrl = formData.get('returnUrl');

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
            if (data.success) {
                if (data.removed) {
                    // Instead of dynamically replacing the stepper with an "Add to Basket" button,
                    // simply reload the page when an item is removed
                    window.location.reload();
                } else {
                    // Update the quantity input
                    form.querySelector('.quantity-input').value = data.newQuantity;
                }
            } else {
                console.error('Update failed: ' + data.message);
            }
        })
        .catch(err => {
            console.error('Fetch error: ', err);
        });
}


// Add event listeners to "Add to Basket" buttons
document.querySelectorAll('.addToBasketForm').forEach(form => {
    const addButton = form.querySelector('.add-to-basket-btn');

    addButton.addEventListener('click', function() {
        const itemId = form.querySelector('input[name="itemId"]').value;
        const returnUrl = form.querySelector('input[name="returnUrl"]').value;

        // Send AJAX request to add item to basket
        fetch(window.location.pathname + '?handler=AddToBasketAjax', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                itemId: itemId,
                returnUrl: returnUrl
            })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Show success message
                    showTemporaryMessage('Item added to basket!', 'success');

                    // Replace the "Add to Basket" button with a stepper
                    const cell = form.closest('td');

                    // Create a new form element for the stepper
                    const newForm = document.createElement('form');
                    newForm.method = 'post';
                    newForm.className = 'updateQuantityForm';
                    newForm.setAttribute('action', window.location.pathname + '?handler=UpdateQuantity');

                    // Create hidden inputs
                    const itemIdInput = document.createElement('input');
                    itemIdInput.type = 'hidden';
                    itemIdInput.name = 'itemId';
                    itemIdInput.value = itemId;

                    const returnUrlInput = document.createElement('input');
                    returnUrlInput.type = 'hidden';
                    returnUrlInput.name = 'returnUrl';
                    returnUrlInput.value = returnUrl;

                    // Create the stepper elements
                    const decrementBtn = document.createElement('button');
                    decrementBtn.type = 'button';
                    decrementBtn.className = 'btn btn-sm btn-outline-danger decrement-btn';
                    decrementBtn.innerHTML = '<i class="fa-solid fa-minus"></i>';

                    const quantityInput = document.createElement('input');
                    quantityInput.type = 'number';
                    quantityInput.name = 'quantity';
                    quantityInput.value = '1'; // New item starts with quantity 1
                    quantityInput.min = '0';
                    quantityInput.max = '1000';
                    quantityInput.className = 'form-control mx-2 quantity-input';
                    quantityInput.style = 'width: fit-content; min-width: 50px; padding: 0.2em; display:inline-block;';
                    quantityInput.dataset.previousValue = '1';

                    const incrementBtn = document.createElement('button');
                    incrementBtn.type = 'button';
                    incrementBtn.className = 'btn btn-sm btn-outline-success increment-btn';
                    incrementBtn.innerHTML = '<i class="fa-solid fa-plus"></i>';

                    // Add CSRF token
                    const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]');
                    if (antiforgeryToken) {
                        const tokenInput = document.createElement('input');
                        tokenInput.type = 'hidden';
                        tokenInput.name = '__RequestVerificationToken';
                        tokenInput.value = antiforgeryToken.value;
                        newForm.appendChild(tokenInput);
                    }

                    // Assemble the form
                    newForm.appendChild(itemIdInput);
                    newForm.appendChild(returnUrlInput);
                    newForm.appendChild(decrementBtn);
                    newForm.appendChild(quantityInput);
                    newForm.appendChild(incrementBtn);

                    // Replace the content
                    cell.innerHTML = '';
                    cell.appendChild(newForm);

                    // Add event listeners to the new stepper
                    setupStepperEventListeners(newForm);
                } else {
                    showTemporaryMessage('Failed to add item to basket: ' + data.message, 'danger');
                }
            })
            .catch(err => {
                console.error('Fetch error: ', err);
                showTemporaryMessage('Error adding item to basket', 'danger');
            });
    });
});

// Function to set up event listeners for a stepper form
function setupStepperEventListeners(form) {
    // Intercept form submit
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        sendUpdateRequest(form, 'update');
    });

    const quantityInput = form.querySelector('.quantity-input');
    const incBtn = form.querySelector('.increment-btn');
    const decBtn = form.querySelector('.decrement-btn');

    // When increment button is clicked
    incBtn.addEventListener('click', function () {
        let current = parseInt(quantityInput.value, 10) || 0;
        quantityInput.value = current + 1;
        quantityInput.dataset.previousValue = quantityInput.value;
        sendUpdateRequest(form, 'increment');
    });

    // When decrement button is clicked
    decBtn.addEventListener('click', function () {
        let current = parseInt(quantityInput.value, 10) || 0;
        if (current > 0) {
            quantityInput.value = current - 1;
            quantityInput.dataset.previousValue = quantityInput.value;
            sendUpdateRequest(form, 'decrement');
        }
    });

    // Listen for manual changes on the input with a debounce
    let debounceTimeout = null;
    quantityInput.addEventListener('input', function () {
        clearTimeout(debounceTimeout);
        if (this.value.trim() === "") return;
        debounceTimeout = setTimeout(() => {
            let val = parseInt(quantityInput.value, 10);
            if (!isNaN(val)) {
                quantityInput.dataset.previousValue = val;
                sendUpdateRequest(form, 'update');
            }
        }, 800);
    });

    // On blur, if empty, restore previous value
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
}

// Function to show a temporary message
function showTemporaryMessage(message, type) {
    // Check if there's already a message container
    let alertContainer = document.getElementById('temporary-alert-container');

    if (!alertContainer) {
        // Create a container for alerts if it doesn't exist
        alertContainer = document.createElement('div');
        alertContainer.id = 'temporary-alert-container';
        alertContainer.style.position = 'fixed';
        alertContainer.style.top = '20px';
        alertContainer.style.right = '20px';
        alertContainer.style.zIndex = '1050';
        document.body.appendChild(alertContainer);
    }

    // Create the alert element
    const alert = document.createElement('div');
    alert.className = `alert alert-${type} alert-dismissible fade show`;
    alert.role = 'alert';
    alert.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    // Add the alert to the container
    alertContainer.appendChild(alert);

    // Remove the alert after 3 seconds
    setTimeout(() => {
        alert.classList.remove('show');
        setTimeout(() => {
            alertContainer.removeChild(alert);
        }, 150);
    }, 3000);
}
