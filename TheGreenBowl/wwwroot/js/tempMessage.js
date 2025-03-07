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