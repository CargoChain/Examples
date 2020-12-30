namespace eShop.Lib
{
    public static class ProfileTypes
    {
        public const string Product = "eShopProduct";
    }

    public static class ProductEventTypes
    {
        public const string ProductState = "ProductState";
        public const string DeliveryAddress = "DeliveryAddress";
        public const string ProductInformation = "ProductInformation";
    }

    public enum ProductState
    {
        Available,
        Ordered,
        Delivered
    }
}
