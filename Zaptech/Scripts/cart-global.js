$(document).ready(function () {
    // Update cart count on every page load
    updateCartCount();

    function updateCartCount() {
        $.get('/CustomerCart/GetCartCount', function (data) {
            $('#cartCount').text(data.count);
        });
    }

    // Handle add to cart buttons
    $('.add-to-cart').click(function (e) {
        e.preventDefault();
        const productId = $(this).data('id');

        $.post('/CustomerCart/AddToCart', { productId: productId }, function (response) {
            if (response.success) {
                updateCartCount();
                showAlert(response.message, 'success');
            } else {
                showAlert(response.message, 'danger');
                if (response.redirect) {
                    setTimeout(() => {
                        window.location.href = response.redirect;
                    }, 1500);
                }
            }
        }).fail(function () {
            showAlert('Error adding to cart', 'danger');
        });
    });

    function showAlert(message, type) {
        const alert = $(`
            <div class="alert alert-${type} alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 9999;">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `);
        $('body').append(alert);
        setTimeout(() => alert.alert('close'), 3000);
    }
});