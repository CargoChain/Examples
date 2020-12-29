using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eShop.Carrier.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; }
        public List<ProductPosition> Positions { get; set; }
        public ProductState State { get; set; }
        public string CargoChainProfilePublicId { get; set; }
        public string CargoChainProfileSecretId { get; set; }
    }
}
