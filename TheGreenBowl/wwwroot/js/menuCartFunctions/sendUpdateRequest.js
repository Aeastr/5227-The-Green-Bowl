// Function to send update request
function sendUpdateRequest(form, operation, updateUrl) {
    const formData = new FormData(form);
    formData.set('operation', operation);
    const itemId = formData.get('itemId');
    const returnUrl = formData.get('returnUrl');

    // Get the antiforgery token from your hidden form
    const token = document.querySelector('#antiforgeryform input[name="__RequestVerificationToken"]').value;

    // Use the provided update URL
    fetch(updateUrl, {
        method: 'POST',
        body: formData,
        credentials: 'same-origin',
        headers: {
            'X-Requested-With': 'XMLHttpRequest',
            'RequestVerificationToken': token  // Add the token here
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                if (data.removed) {
                    // Item was removed from basket - create new Add to Basket button
                    const container = form.closest('.menu-item-actions');

                    // Get the current page URL for the handler
                    const addToBasketUrl = window.location.pathname + '?handler=AddToBasketAjax';

                    // Create new "Add to Basket" form
                    const newForm = document.createElement('form');
                    newForm.method = 'post';
                    newForm.className = 'addToBasketForm';
                    newForm.style.display = 'inline';

                    // Create hidden inputs
                    const itemIdInput = document.createElement('input');
                    itemIdInput.type = 'hidden';
                    itemIdInput.name = 'itemId';
                    itemIdInput.value = itemId;

                    const returnUrlInput = document.createElement('input');
                    returnUrlInput.type = 'hidden';
                    returnUrlInput.name = 'returnUrl';
                    returnUrlInput.value = returnUrl;

                    // Create button
                    const addButton = document.createElement('button');
                    addButton.type = 'button';
                    addButton.className = 'btn btn-sm btn-outline-primary add-to-basket-btn';
                    addButton.innerHTML = '<i class="fa-solid fa-cart-plus"></i> Add to Basket';

                    // Assemble form
                    newForm.appendChild(itemIdInput);
                    newForm.appendChild(returnUrlInput);
                    newForm.appendChild(addButton);

                    // Replace content
                    container.innerHTML = '';
                    container.appendChild(newForm);

                    // Add event listener to new button
                    addButton.addEventListener('click', function(e) {
                        e.preventDefault();
                        const itemId = newForm.querySelector('input[name="itemId"]').value;
                        const returnUrl = newForm.querySelector('input[name="returnUrl"]').value;

                        // Send AJAX request to add item to basket
                        fetch(addToBasketUrl, {
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
                                    showTemporaryMessage('Item added to basket!', 'success');

                                    // Create a new form element for the stepper
                                    const newStepperForm = document.createElement('form');
                                    newStepperForm.method = 'post';
                                    newStepperForm.className = 'updateQuantityForm';
                                    newStepperForm.setAttribute('action', updateUrl);

                                    // Create hidden inputs
                                    const itemIdInput = document.createElement('input');
                                    itemIdInput.type = 'hidden';
                                    itemIdInput.name = 'itemId';
                                    itemIdInput.value = itemId;

                                    const returnUrlInput = document.createElement('input');
                                    returnUrlInput.type = 'hidden';
                                    returnUrlInput.name = 'returnUrl';
                                    returnUrlInput.value = returnUrl;

                                    // Create the quantity controls div
                                    const quantityControls = document.createElement('div');
                                    quantityControls.className = 'quantity-controls';

                                    // Create stepper elements
                                    const decrementBtn = document.createElement('button');
                                    decrementBtn.type = 'button';
                                    decrementBtn.className = 'btn btn-sm btn-outline-danger decrement-btn';
                                    decrementBtn.innerHTML = '<i class="fa-solid fa-minus"></i>';

                                    const quantityInput = document.createElement('input');
                                    quantityInput.type = 'number';
                                    quantityInput.name = 'quantity';
                                    quantityInput.value = '1';
                                    quantityInput.min = '0';
                                    quantityInput.max = '1000';
                                    quantityInput.className = 'form-control mx-2 quantity-input';
                                    quantityInput.style = 'width: fit-content; min-width: 50px; padding: 0.2em; display:inline-block;';
                                    quantityInput.dataset.previousValue = '1';

                                    const incrementBtn = document.createElement('button');
                                    incrementBtn.type = 'button';
                                    incrementBtn.className = 'btn btn-sm btn-outline-success increment-btn';
                                    incrementBtn.innerHTML = '<i class="fa-solid fa-plus"></i>';

                                    // Assemble the controls
                                    quantityControls.appendChild(decrementBtn);
                                    quantityControls.appendChild(quantityInput);
                                    quantityControls.appendChild(incrementBtn);

                                    // Add to form
                                    newStepperForm.appendChild(itemIdInput);
                                    newStepperForm.appendChild(returnUrlInput);
                                    newStepperForm.appendChild(quantityControls);

                                    // Replace the Add to Basket form with the stepper
                                    container.innerHTML = '';
                                    container.appendChild(newStepperForm);

                                    // Add event listeners to the new stepper
                                    setupStepperEventListeners(newStepperForm, updateUrl);
                                } else {
                                    showTemporaryMessage('Failed to add item to basket: ' + data.message, 'danger');
                                }
                            })
                            .catch(err => {
                                console.error('Fetch error: ', err);
                                showTemporaryMessage('Error adding item to basket', 'danger');
                            });
                    });


                    showTemporaryMessage('Item removed from basket', 'info');
                } else {
                    // Update the quantity input
                    form.querySelector('.quantity-input').value = data.newQuantity;
                    form.querySelector('.quantity-input').dataset.previousValue = data.newQuantity;
                    showTemporaryMessage('Quantity updated', 'success');
                }
            } else {
                console.error('Update failed: ' + data.message);
                showTemporaryMessage('Failed to update quantity: ' + data.message, 'danger');
            }
        })
        .catch(err => {
            console.error('Fetch error: ', err);
            showTemporaryMessage('Error updating quantity', 'danger');
        });
}
