using eShop.Lib;
using eShop.Shop.Models;
using LiteDB;
using System;

namespace eShop.Shop.Data
{
    public class ShopContext : IDisposable
    {
        private readonly LiteDatabase _db;

        public ILiteCollection<Product> Products => _db.GetCollection<Product>("products");
        public ILiteCollection<CargoChainSubscription> Subscriptions => _db.GetCollection<CargoChainSubscription>("subscriptions");

        public ShopContext()
        {
            _db = new LiteDatabase(@"shop.db");
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
