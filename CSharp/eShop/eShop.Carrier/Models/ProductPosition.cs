using System;
using System.ComponentModel.DataAnnotations;

namespace eShop.Carrier.Models
{
    public class ProductPosition
    {
        public Guid Id { get; set; }
        [Required]
        public string Position { get; set; }
        public string Temperature { get; set; }
        public DateTimeOffset PositionAt { get; set; }
        public bool ProductDelivered { get; set; }
    }
}
