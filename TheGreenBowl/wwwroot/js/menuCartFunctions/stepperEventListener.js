// Function to set up event listeners for a stepper form
function setupStepperEventListeners(form, updateUrl) {
    // Intercept form submit
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        sendUpdateRequest(form, 'update', updateUrl);
    });

    const quantityInput = form.querySelector('.quantity-input');
    const incBtn = form.querySelector('.increment-btn');
    const decBtn = form.querySelector('.decrement-btn');

    // Store initial value
    quantityInput.dataset.previousValue = quantityInput.value;

    // When increment button is clicked
    incBtn.addEventListener('click', function (e) {
        e.preventDefault();
        let current = parseInt(quantityInput.value, 10) || 0;
        quantityInput.value = current + 1;
        quantityInput.dataset.previousValue = quantityInput.value;
        sendUpdateRequest(form, 'increment', updateUrl);
    });

    // When decrement button is clicked
    decBtn.addEventListener('click', function (e) {
        e.preventDefault();
        let current = parseInt(quantityInput.value, 10) || 0;
        if (current > 0) {
            quantityInput.value = current - 1;
            quantityInput.dataset.previousValue = quantityInput.value;
            sendUpdateRequest(form, 'decrement', updateUrl);
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
                sendUpdateRequest(form, 'update', updateUrl);
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
                sendUpdateRequest(form, 'update', updateUrl);
            }
        }
    });
}

