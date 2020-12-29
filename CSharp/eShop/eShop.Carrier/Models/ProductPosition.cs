using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Carrier.Models
{
    public class ProductPosition
    {
        public Guid Id { get; set; }
        public string Position { get; set; }
        public string Temperature { get; set; }
    }
}
