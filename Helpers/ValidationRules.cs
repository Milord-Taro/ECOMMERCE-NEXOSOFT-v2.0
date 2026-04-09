namespace ECOMMERCE_NEXOSOFT.Helpers
{
    public static class ValidationRules
    {
        public const string NamePattern = @"^(?=.{3,50}$)[A-Za-zÁÉÍÓÚáéíóúÑñ]+(?:[ -][A-Za-zÁÉÍÓÚáéíóúÑñ]+)*$";

        public const string PhonePattern = @"^(3\d{9}|6\d{9})$";

        public const string NumericIdentificationPattern = @"^\d{5,10}$";

        public const string PassportPattern = @"^[A-Za-z0-9]{5,10}$";

        public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d])\S{8,100}$";

        public const string AddressPattern = @"^[A-Za-zÁÉÍÓÚáéíóúÑñ0-9#\-\.,/ ]{10,150}$";

        public const string StoreNamePattern = @"^(?=.{2,30}$)(?=.*[A-Za-zÁÉÍÓÚáéíóúÑñ0-9])[A-Za-zÁÉÍÓÚáéíóúÑñ0-9&\-. ]+$";

        public const string NitPattern = @"^\d{9,10}$";

        public const string BarcodePattern = @"^\d{8,13}$";

        public const string SkuPattern = @"^[A-Za-z0-9\-]{1,20}$";

        public static readonly string[] ValidIdentificationTypes = { "cc", "ti", "ppt", "pasaporte" };

        public const string CategoryOrSubcategoryNamePattern = @"^(?=.{3,40}$)(?=.*[A-Za-zÁÉÍÓÚáéíóúÑñ0-9]).+$";

        public const string CategoryDescriptionPattern = @"^(?=.{3,100}$)(?=.*[A-Za-zÁÉÍÓÚáéíóúÑñ0-9]).+$";

        public const string SubcategoryDescriptionPattern = @"^(?=.{3,150}$)(?=.*[A-Za-zÁÉÍÓÚáéíóúÑñ0-9]).+$";

        public const string ProductNamePattern = @"^(?=.{3,50}$)(?=.*[A-Za-zÁÉÍÓÚáéíóúÑñ0-9]).+$";

        public const string BrandPattern = @"^(?=.{2,50}$)(?=.*[A-Za-zÁÉÍÓÚáéíóúÑñ0-9]).+$";
    }
}