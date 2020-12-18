using System;
using System.ComponentModel.DataAnnotations;

namespace eShop.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public ProductState State { get; set; }
    }
}
