/**
 * Renders a menu card from a menu object.
 * @param {Object} menu - The menu data
 * @param {boolean} [isNew=false] - If true, checkboxes are rendered as checked for new menus.
 * @returns {string} HTML string for the menu card.
 */
function renderMenuCard(menu, isNew = false) {
    // Create categories HTML (assuming menu.categories is an array)
    const categoriesHtml = menu.categories && menu.categories.length
        ? menu.categories.map(
            cat => `<span class="category-pill"># ${cat.name}</span>`
        ).join('')
        : '';

    // Build the card HTML
    return `
    <div class="menu-item-card" data-menu-id="${menu.menuID}">
      <div class="menu-item-content">
        <h5>${menu.name}</h5>
        <p class="menu-item-description">
          ${
        menu.description && menu.description.length > 100
            ? menu.description.substring(0, 100) + "..."
            : menu.description || '<span class="fst-italic text-muted">No description</span>'
    }
        </p>
        <div class="menu-categories mb-2">
          ${categoriesHtml}
        </div>
        <div class="menu-item-actions">
          <a href="./ManageItems/${menu.menuID}" class="btn-pill btn-sm btn-outline btn-success">
            <i class="fa-solid fa-list icon-left"></i> Items
          </a>
          <button type="button" class="btn-pill btn-sm btn-outline btn-primary"
                  data-bs-toggle="modal" data-bs-target="#editMenuModal"
                  data-menu-id="${menu.menuID}"
                  data-menu-name="${menu.name}"
                  data-menu-description="${menu.description}"
                  data-menu-categories="${menu.categories ? menu.categories.map(c => c.categoryID).join(',') : ''}"
                  aria-label="Edit Menu">
            <i class="fa-solid fa-edit"></i>
          </button>
          <button type="button" class="btn-pill btn-sm btn-outline btn-danger"
                  data-bs-toggle="modal" data-bs-target="#deleteMenuModal"
                  data-menu-id="${menu.menuID}"
                  data-menu-name="${menu.name}"
                  aria-label="Delete Menu">
            <i class="fa-solid fa-trash"></i>
          </button>
        </div>
      </div>
    </div>
  `;
}
