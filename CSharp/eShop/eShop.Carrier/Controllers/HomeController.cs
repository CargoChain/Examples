using eShop.Carrier.Data;
using eShop.Carrier.Models;
using eShop.Lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Carrier.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CarrierContext _context;
        private readonly CargoChainService _cargoChainService;
        private readonly CargoChainConfiguration _cargoChainConfiguration;

        public HomeController(
            ILogger<HomeController> logger,
            CarrierContext context,
            CargoChainService cargoChainService,
            CargoChainConfiguration cargoChainConfiguration)
        {
            _logger = logger;
            _context = context;
            _cargoChainService = cargoChainService;
            _cargoChainConfiguration = cargoChainConfiguration;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .Find(x => x.State == ProductState.Ordered)
                .OrderBy(x => x.Name)
                .ToList();

            ViewBag.PublicViewUrl = _cargoChainConfiguration.PublicViewUrl;

            return View(products);
        }

        public IActionResult Delivered()
        {
            var products = _context.Products
                .Find(x => x.State == ProductState.Delivered)
                .OrderBy(x => x.Name)
                .ToList();

            ViewBag.PublicViewUrl = _cargoChainConfiguration.PublicViewUrl;

            return View(products);
        }

        public IActionResult Details(Guid id)
        {
            var product = _context.Products.FindById(id);
            return View(product);
        }

        public IActionResult AddPosition(Guid id)
        {
            var product = _context.Products.FindById(id);
            var model = new ProductPositionViewModel { Product = product, Position = new ProductPosition() };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPosition(ProductPositionViewModel model)
        {
            var dbProduct = _context.Products.FindById(model.Product.Id);
            if (dbProduct.Positions == null)
            {
                dbProduct.Positions = new List<ProductPosition>();
            }
            model.Position.Id = Guid.NewGuid();
            model.Position.PositionAt = DateTimeOffset.Now;
            dbProduct.Positions.Add(model.Position);

            await _cargoChainService.AddProductPosition(dbProduct.CargoChainProfileSecretId, model.Position);

            if (model.Position.ProductDelivered)
            {
                dbProduct.State = ProductState.Delivered;
                await _cargoChainService.UpdateProfileState(dbProduct.CargoChainProfileSecretId, dbProduct.State);
            }

            _context.Products.Update(dbProduct);

            return RedirectToAction(nameof(Details), new { id = model.Product.Id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
