using eShop.Carrier.Models;
using LiteDB;
using System;

namespace eShop.Carrier.Data
{
    public class CarrierContext : IDisposable
    {
        private readonly LiteDatabase _db;

        public ILiteCollection<Product> Products => _db.GetCollection<Product>("products");
        public ILiteCollection<CargoChainSubscription> Subscriptions => _db.GetCollection<CargoChainSubscription>("subscriptions");

        public CarrierContext()
        {
            _db = new LiteDatabase(@"carrier.db");
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
