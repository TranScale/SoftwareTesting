/* File: wwwroot/js/categories-index.js */

function showToast(message, type = 'success') {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = 'toast-msg';
    const icon = type === 'success' 
        ? '<i class="fas fa-check-circle text-success"></i>' 
        : '<i class="fas fa-exclamation-circle text-danger"></i>';
    
    toast.innerHTML = `${icon} <span>${message}</span>`;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

async function toggleTrending(id, checkbox) {
    checkbox.disabled = true;
    try {
        const response = await fetch('/Admin/Categories/ToggleTrending/' + id, { 
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            const status = checkbox.checked ? "Đã bật" : "Đã tắt";
            showToast(`${status} Trending thành công!`, 'success');
        } else {
            throw new Error('Server error');
        }
    } catch (error) {
        console.error(error);
        checkbox.checked = !checkbox.checked;
        showToast('Lỗi kết nối server!', 'error');
    } finally {
        checkbox.disabled = false;
    }
}