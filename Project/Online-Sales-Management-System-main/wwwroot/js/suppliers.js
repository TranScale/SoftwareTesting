/* File: wwwroot/js/suppliers.js */

document.addEventListener("DOMContentLoaded", function () {
    // 1. Tự động submit form khi đổi Page Size
    const pageSizeSelect = document.querySelector('select[name="pageSize"]');
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener('change', function () {
            this.form.submit();
        });
    }

    // 2. Xác nhận khi xóa (Optional - Thêm an toàn)
    const deleteButtons = document.querySelectorAll('.btn-action.delete');
    deleteButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            if (!confirm('Are you sure you want to delete this supplier?')) {
                e.preventDefault();
            }
        });
    });
});