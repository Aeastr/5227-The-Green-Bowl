/**
 * Menu Management JavaScript
 * Handles all interactions for the Manage Menus page
 */

document.addEventListener('DOMContentLoaded', function() {
    // Get anti-forgery token
    const antiForgeryToken = document.querySelector('#antiforgeryform input[name="__RequestVerificationToken"]').value;
    console.log('Anti-forgery token:', antiForgeryToken ? 'Found' : 'Not found');


    // Edit Menu Modal
    setupEditMenuModal();

    // Delete Menu Modal
    setupDeleteMenuModal();

    // Create Menu Modal
    setupCreateMenuModal();

    // Add Category Modal
    setupAddCategoryModal();

    // Edit Category Modal
    setupEditCategoryModal();

    /**
     * Setup Edit Menu Modal
     */
    function setupEditMenuModal() {
        const editMenuModal = document.getElementById('editMenuModal');
        if (!editMenuModal) return;

        // Set up modal data when shown
        editMenuModal.addEventListener('show.bs.modal', function(event) {
            const button = event.relatedTarget;
            if (!button) return;

            const menuId = button.getAttribute('data-menu-id');
            const menuName = button.getAttribute('data-menu-name');
            const menuDescription = button.getAttribute('data-menu-description');
            const menuCategories = button.getAttribute('data-menu-categories').split(',').filter(Boolean);

            document.getElementById('editMenuId').value = menuId;
            document.getElementById('editMenuName').value = menuName;
            document.getElementById('editMenuDescription').value = menuDescription;

            // Reset all checkboxes
            document.querySelectorAll('.category-checkbox').forEach(checkbox => {
                checkbox.checked = false;
            });

            // Check the appropriate categories
            menuCategories.forEach(categoryId => {
                const checkbox = document.getElementById(`edit_category_${categoryId}`);
                if (checkbox) checkbox.checked = true;
            });
        });

        // Handle save button click
        const saveMenuChangesBtn = document.getElementById('saveMenuChanges');
        if (saveMenuChangesBtn) {
            saveMenuChangesBtn.addEventListener('click', function() {
                const form = document.getElementById('editMenuForm');
                if (form && form.checkValidity()) {
                    updateMenu();
                } else if (form) {
                    form.classList.add('was-validated');
                }
            });
        }
    }

    /**
     * Setup Delete Menu Modal
     */
    function setupDeleteMenuModal() {
        const deleteMenuModal = document.getElementById('deleteMenuModal');
        if (!deleteMenuModal) return;

        // Set up modal data when shown
        deleteMenuModal.addEventListener('show.bs.modal', function(event) {
            const button = event.relatedTarget;
            if (!button) return;

            const menuId = button.getAttribute('data-menu-id');
            const menuName = button.getAttribute('data-menu-name');

            document.getElementById('deleteMenuName').textContent = menuName;

            // Set up the confirm delete button
            const confirmDeleteBtn = document.getElementById('confirmDeleteMenu');
            if (confirmDeleteBtn) {
                confirmDeleteBtn.onclick = function() {
                    deleteMenu(menuId);
                };
            }
        });
    }

    /**
     * Setup Create Menu Modal
     */
    function setupCreateMenuModal() {
        const saveNewMenuBtn = document.getElementById('saveNewMenu');
        if (!saveNewMenuBtn) return;

        saveNewMenuBtn.addEventListener('click', function() {
            const form = document.getElementById('createMenuForm');
            if (form && form.checkValidity()) {
                createMenu();
            } else if (form) {
                form.classList.add('was-validated');
            }
        });
    }

    /**
     * Setup Add Category Modal
     */
    function setupAddCategoryModal() {
        const addCategoryForm = document.getElementById('addCategoryForm');
        if (!addCategoryForm) return;

        addCategoryForm.addEventListener('submit', function(e) {
            e.preventDefault();

            const categoryName = document.getElementById('newCategoryName').value;
            const categoryDescription = document.getElementById('newCategoryDescription').value;

            if (!categoryName.trim()) {
                document.getElementById('newCategoryName').classList.add('is-invalid');
                return;
            }

            createCategory(categoryName, categoryDescription);
        });
    }

    /**
     * Setup Edit Category Modal
     */
    function setupEditCategoryModal() {
        const editCategoryForm = document.getElementById('editCategoryForm');
        if (!editCategoryForm) return;

        // Load category data into edit modal
        const editButtons = document.querySelectorAll('.edit-category-btn');
        editButtons.forEach(button => {
            button.addEventListener('click', function() {
                const categoryId = this.getAttribute('data-category-id');
                const categoryName = this.getAttribute('data-category-name');

                document.getElementById('editCategoryId').value = categoryId;
                document.getElementById('editCategoryName').value = categoryName;

                // Fetch the category description via AJAX
                fetch(`/api/Categories/${categoryId}`)
                    .then(response => {
                        if (!response.ok) throw new Error('Network response was not ok');
                        return response.json();
                    })
                    .then(data => {
                        document.getElementById('editCategoryDescription').value = data.description || '';
                    })
                    .catch(error => {
                        console.error('Error fetching category details:', error);
                        showTemporaryMessage('Failed to load category details', 'danger');
                    });
            });
        });

        // Handle edit category form submission
        editCategoryForm.addEventListener('submit', function(e) {
            e.preventDefault();

            const categoryId = document.getElementById('editCategoryId').value;
            const categoryName = document.getElementById('editCategoryName').value;
            const categoryDescription = document.getElementById('editCategoryDescription').value;

            if (!categoryName.trim()) {
                document.getElementById('editCategoryName').classList.add('is-invalid');
                return;
            }

            updateCategory(categoryId, categoryName, categoryDescription);
        });
    }

    /**
     * Function to update a menu
     */
    function updateMenu() {
        const menuId = document.getElementById('editMenuId').value;
        const menuName = document.getElementById('editMenuName').value;
        const menuDescription = document.getElementById('editMenuDescription').value;
        const selectedCategories = [];
        document.querySelectorAll('.category-checkbox:checked').forEach(checkbox => {
            selectedCategories.push(parseInt(checkbox.value));
        });

        fetch('/Menu/Admin/ManageMenus?handler=UpdateMenu', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiForgeryToken
            },
            body: JSON.stringify({
                menuID: parseInt(menuId),
                name: menuName,
                description: menuDescription,
                selectedCategoryIds: selectedCategories
            })
        })
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Close the modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('editMenuModal'));
                    modal.hide();

                    // Update the menu card by replacing the HTML inside it.
                    // Assume your API returns updated menu data in data.updatedMenu.
                    const updatedMenu = data.updatedMenu;
                    if (updatedMenu) {
                        const menuCard = document.querySelector(`.menu-item-card[data-menu-id="${menuId}"]`);
                        if (menuCard) {
                            // Replace the card's inner HTML using our component.
                            menuCard.outerHTML = renderMenuCard(updatedMenu);
                        }
                        else{
                            console.log("No menu card found to update");
                        }
                    }
                    else{
                        console.log("No updated menu data returned");
                    }
                    showTemporaryMessage('Menu updated successfully', 'success');
                } else {
                    showTemporaryMessage('Error updating menu: ' + (data.message || 'Unknown error'), 'danger');
                }
            })
            .catch(error => {
                console.error('Error updating menu:', error);
                showTemporaryMessage('Error updating menu: ' + error.message, 'danger');
            });
    }



    /**
     * Function to create a new menu
     */
    function createMenu() {
        const menuName = document.getElementById('createMenuName').value;
        const menuDescription = document.getElementById('createMenuDescription').value;
        const selectedCategories = [];
        document.querySelectorAll('.create-category-checkbox:checked').forEach(checkbox => {
            selectedCategories.push(parseInt(checkbox.value));
        });

        fetch('/Menu/Admin/ManageMenus?handler=CreateMenu', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiForgeryToken
            },
            body: JSON.stringify({
                name: menuName,
                description: menuDescription,
                selectedCategoryIds: selectedCategories
            })
        })
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Close create modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('createMenuModal'));
                    modal.hide();

                    // Assume data.newMenu has the new menu data (including categories)
                    const menuList = document.querySelector('.menu-items-grid');
                    if (menuList && data.newMenu) {
                        // Append new menu card generated from our component
                        menuList.insertAdjacentHTML('beforeend', renderMenuCard(data.newMenu, true));
                    }
                    showTemporaryMessage('Menu created successfully', 'success');
                } else {
                    showTemporaryMessage('Error creating menu: ' + (data.message || 'Unknown error'), 'danger');
                }
            })
            .catch(error => {
                console.error('Error creating menu:', error);
                showTemporaryMessage('Error creating menu: ' + error.message, 'danger');
            });
    }


    /**
     * Function to delete a menu
     */
    function deleteMenu(menuId) {
        fetch('/Menu/Admin/ManageMenus?handler=DeleteMenu', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiForgeryToken
            },
            body: JSON.stringify({ menuID: parseInt(menuId) })
        })
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Close the modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('deleteMenuModal'));
                    modal.hide();

                    // Remove the deleted menu's card element from the DOM
                    const menuCard = document.querySelector(`.menu-item-card[data-menu-id="${menuId}"]`);
                    if (menuCard) {
                        menuCard.parentNode.removeChild(menuCard);
                    }

                    // Show a success message
                    showTemporaryMessage('Menu deleted successfully', 'success');
                } else {
                    showTemporaryMessage('Error deleting menu: ' + (data.message || 'Unknown error'), 'danger');
                }
            })
            .catch(error => {
                console.error('Error deleting menu:', error);
                showTemporaryMessage('Error deleting menu: ' + error.message, 'danger');
            });
    }

    /**
     * Function to create a new category
     */
    function createCategory(name, description) {
        fetch('/api/Categories', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiForgeryToken
            },
            body: JSON.stringify({
                name: name,
                description: description
            })
        })
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                // Close the modal and reset the form
                const modal = bootstrap.Modal.getInstance(document.getElementById('addCategoryModal'));
                modal.hide();
                document.getElementById('addCategoryForm').reset();

                showTemporaryMessage('Category added successfully. Refreshing page...', 'success');

                // Reload the page to show the new category
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            })
            .catch(error => {
                console.error('Error creating category:', error);
                showTemporaryMessage('Error creating category: ' + error.message, 'danger');
            });
    }

    /**
     * Function to update a category
     */
    function updateCategory(id, name, description) {
        fetch(`/api/Categories/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiForgeryToken
            },
            body: JSON.stringify({
                categoryID: parseInt(id),
                name: name,
                description: description
            })
        })
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                // Close the modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('editCategoryModal'));
                modal.hide();

                showTemporaryMessage('Category updated successfully. Refreshing page...', 'success');

                // Reload the page to show the updated category
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            })
            .catch(error => {
                console.error('Error updating category:', error);
                showTemporaryMessage('Error updating category: ' + error.message, 'danger');
            });
    }
});
