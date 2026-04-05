document.addEventListener("DOMContentLoaded", function () {
    function applyMaxLengthSanitizer(selector, maxLength) {
        document.querySelectorAll(selector).forEach(input => {
            input.setAttribute("maxlength", maxLength);

            input.addEventListener("input", function () {
                if (this.value.length > maxLength) {
                    this.value = this.value.slice(0, maxLength);
                }
            });
        });
    }

    function applyDigitsOnly(selector, maxLength = null) {
        document.querySelectorAll(selector).forEach(input => {
            input.setAttribute("inputmode", "numeric");

            if (maxLength) {
                input.setAttribute("maxlength", maxLength);
            }

            input.addEventListener("input", function () {
                let value = this.value.replace(/\D/g, "");

                if (maxLength) {
                    value = value.slice(0, maxLength);
                }

                this.value = value;
            });
        });
    }

    function applyEmailNormalizer(selector, maxLength = 100) {
        document.querySelectorAll(selector).forEach(input => {
            input.setAttribute("maxlength", maxLength);

            input.addEventListener("input", function () {
                this.value = this.value.replace(/\s/g, "").slice(0, maxLength);
            });
        });
    }

    function applyTextMaxLength(selector, maxLength) {
        document.querySelectorAll(selector).forEach(input => {
            input.setAttribute("maxlength", maxLength);

            input.addEventListener("input", function () {
                this.value = this.value.slice(0, maxLength);
            });
        });
    }

    function applyAlphaNumericNoSpaces(selector, maxLength) {
        document.querySelectorAll(selector).forEach(input => {
            input.setAttribute("maxlength", maxLength);

            input.addEventListener("input", function () {
                this.value = this.value.replace(/[^a-zA-Z0-9-]/g, "").slice(0, maxLength);
            });
        });
    }

    // Nombres y apellidos
    applyTextMaxLength(".js-name-50", 50);

    // Identificación / NIT numéricos
    applyDigitsOnly(".js-id-10", 10);

    // Teléfono
    applyDigitsOnly(".js-phone-10", 10);

    // Correo
    applyEmailNormalizer(".js-email-100", 100);

    // Dirección
    applyTextMaxLength(".js-address-150", 150);

    // Tienda
    applyTextMaxLength(".js-store-30", 30);

    // Textos genéricos
    applyTextMaxLength(".js-text-40", 40);
    applyTextMaxLength(".js-text-100", 100);
    applyTextMaxLength(".js-text-150", 150);

    // Código de barras
    applyDigitsOnly(".js-barcode-13", 13);

    // SKU
    applyAlphaNumericNoSpaces(".js-sku-30", 30);

    // Contraseñas
    applyMaxLengthSanitizer(".js-password-100", 100);
});