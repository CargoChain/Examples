using eShop.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Data
{
    public class ShopContext : IDisposable
    {
        private LiteDatabase _db;

        public ILiteCollection<Product> Products => _db.GetCollection<Product>("products");

        public ShopContext()
        {
            _db = new LiteDatabase(@"eshop.db");
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
