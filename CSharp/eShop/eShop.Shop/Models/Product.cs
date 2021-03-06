﻿using eShop.Lib;
using System;
using System.ComponentModel.DataAnnotations;

namespace eShop.Shop.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; }
        public ProductState State { get; set; }
        public string CargoChainProfilePublicId { get; set; }
        public string CargoChainProfileSecretId { get; set; }
        public string CargoChainLastEvent { get; set; }
    }
}
