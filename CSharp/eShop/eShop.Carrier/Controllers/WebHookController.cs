using eShop.Carrier.Data;
using eShop.Carrier.Models;
using eShop.Lib;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace eShop.Carrier.Controllers
{
    [Route("hook")]
    [ApiController]
    [EnableCors("AnyOriginPolicy")]
    public class WebHookController : ControllerBase
    {
        private readonly CargoChainService _cargoChainService;
        private readonly CarrierContext _context;

        public WebHookController(CargoChainService cargoChainService, CarrierContext context)
        {
            _cargoChainService = cargoChainService;
            _context = context;
        }

        [HttpPost]
        public async Task Handle(CargoChainHookRequest request)
        {
            if (request.ProfileType == ProfileTypes.Product)
            {
                var dbProduct = _context.Products.FindOne(x => x.CargoChainProfilePublicId == request.ProfilePublicId);

                if (dbProduct == null)
                {
                    // New product
                    var profile = await _cargoChainService.GetProfileByPublicId(request.ProfilePublicId);
                    dbProduct = new Product()
                    {
                        Id = Guid.NewGuid(),
                        CargoChainProfilePublicId = profile.ProfilePublicId,
                        CargoChainProfileSecretId = profile.ProfileSecretId
                    };

                    await SyncProductFromCargoChain(dbProduct);

                    _context.Products.Insert(dbProduct);
                    _context.Products.EnsureIndex(x => x.CargoChainProfilePublicId);
                }
                else
                {
                    // Existing product
                    await SyncProductFromCargoChain(dbProduct);
                    _context.Products.Update(dbProduct);
                }
            }
        }

        private async Task SyncProductFromCargoChain(Product dbProduct)
        {
            var events = await _cargoChainService.GetEvents(dbProduct.CargoChainProfileSecretId, dbProduct.CargoChainLastEvent);

            foreach (var evt in events)
            {
                switch (evt.EventBody.EventType)
                {
                    case ProductEventTypes.ProductInformation:
                        dbProduct.Name = _cargoChainService.GetPropertyValue(evt, "Name");
                        break;
                    case ProductEventTypes.DeliveryAddress:
                        dbProduct.DeliveryAddress = _cargoChainService.GetPropertyValue(evt, "DeliveryAddress");
                        break;
                    case ProductEventTypes.ProductState:
                        dbProduct.State = Enum.Parse<ProductState>(_cargoChainService.GetPropertyValue(evt, "State"));
                        break;
                }

                dbProduct.CargoChainLastEvent = evt.EventHash;
            }
        }
    }
}
