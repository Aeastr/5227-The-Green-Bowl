document.addEventListener('DOMContentLoaded', function() {
    // Determine which page we're on
    const isIndexPage = window.location.pathname.includes('/Menu/Index');
    const isDetailsPage = window.location.pathname.includes('/Menu/Details');

    // Set the correct handler URLs based on current page
    const currentPagePath = window.location.pathname;
    const addToBasketUrl = currentPagePath + '?handler=AddToBasketAjax';
    const updateQuantityUrl = currentPagePath + '?handler=UpdateQuantity';

    // Card item click behavior (for card layouts)
    const menuCards = document.querySelectorAll('.menu-item-card');
    menuCards.forEach(card => {
        card.addEventListener('click', function(e) {
            // Prevent navigation if clicking on an action button or form element
            if (e.target.closest('a') || e.target.closest('button') || e.target.closest('input') ||
                e.target.closest('.menu-item-actions') || e.target.closest('.menu-item-admin-actions')) {
                return;
            }
            const itemID = this.getAttribute('data-item-id');
            if (isDetailsPage) {
                window.location.href = './Item/ItemDetails?id=' + itemID;
            }
            else{
                window.location.href = './Menu/Item/ItemDetails?id=' + itemID;
            }
        });
    });

    // Set up existing quantity steppers
    document.querySelectorAll('.updateQuantityForm').forEach(form => {
        setupStepperEventListeners(form, updateQuantityUrl);
    });

    // Add event listeners to "Add to Basket" buttons
    document.querySelectorAll('.addToBasketForm').forEach(form => {
        const addButton = form.querySelector('.add-to-basket-btn');

        addButton.addEventListener('click', function(e) {
            e.preventDefault(); // Prevent any default behavior

            const itemId = form.querySelector('input[name="itemId"]').value;
            const returnUrl = form.querySelector('input[name="returnUrl"]').value;

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
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.success) {
                        // Show success message
                        showTemporaryMessage('Item added to basket!', 'success');

                        // Get the container (could be td or menu-item-actions div)
                        const container = isDetailsPage
                            ? form.closest('td')
                            : form.closest('.menu-item-actions');

                        // Create a new form element for the stepper
                        const newForm = document.createElement('form');
                        newForm.method = 'post';
                        newForm.className = 'updateQuantityForm';
                        newForm.setAttribute('action', updateQuantityUrl);

                        // Create hidden inputs
                        const itemIdInput = document.createElement('input');
                        itemIdInput.type = 'hidden';
                        itemIdInput.name = 'itemId';
                        itemIdInput.value = itemId;

                        const returnUrlInput = document.createElement('input');
                        returnUrlInput.type = 'hidden';
                        returnUrlInput.name = 'returnUrl';
                        returnUrlInput.value = returnUrl;

                        // Different layouts for Index (card) vs Details (table)
                        if (isIndexPage) {
                            // Create the quantity controls div for card layout
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

                            // Assemble the controls
                            quantityControls.appendChild(decrementBtn);
                            quantityControls.appendChild(quantityInput);
                            quantityControls.appendChild(incrementBtn);

                            // Add to form
                            newForm.appendChild(itemIdInput);
                            newForm.appendChild(returnUrlInput);
                            newForm.appendChild(quantityControls);
                        } else {
                            // For table layout (details page)
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

                            // Add to form
                            newForm.appendChild(itemIdInput);
                            newForm.appendChild(returnUrlInput);
                            newForm.appendChild(decrementBtn);
                            newForm.appendChild(quantityInput);
                            newForm.appendChild(incrementBtn);
                        }

                        // Replace the content
                        container.innerHTML = '';
                        container.appendChild(newForm);

                        // Add event listeners to the new stepper
                        setupStepperEventListeners(newForm, updateQuantityUrl);
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
});

