using eShop.Lib;
using eShop.Shop.Data;
using eShop.Shop.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace eShop.Shop.Controllers
{
    [Route("hook")]
    [ApiController]
    //[EnableCors("AnyOriginPolicy")]
    public class WebHookController : ControllerBase
    {
        private readonly CargoChainService _cargoChainService;
        private readonly ShopContext _context;
        private readonly ILogger<WebHookController> _logger;

        public WebHookController(CargoChainService cargoChainService, ShopContext context, ILogger<WebHookController> logger)
        {
            _cargoChainService = cargoChainService;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task Handle(CargoChainHookRequest request)
        {
            if (request.ProfileType == ProfileTypes.Product)
            {
                var dbProduct = _context.Products.FindOne(x => x.CargoChainProfilePublicId == request.ProfilePublicId);

                if (dbProduct == null)
                {
                    // Unknown product -> error, we should know it
                    _logger.LogError("Unknown profile with Public ID: " + request.ProfilePublicId);
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
                    case ProductEventTypes.ProductState:
                        var newState = Enum.Parse<ProductState>(_cargoChainService.GetPropertyValue(evt, "State"));
                        if (newState == ProductState.Delivered)
                            dbProduct.State = newState;
                        break;
                }

                dbProduct.CargoChainLastEvent = evt.EventHash;
            }
        }
    }
}
