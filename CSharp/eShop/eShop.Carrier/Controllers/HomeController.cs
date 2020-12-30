using eShop.Carrier.Data;
using eShop.Carrier.Models;
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

        public HomeController(ILogger<HomeController> logger, CarrierContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .Find(x => x.State == Lib.ProductState.Ordered)
                .OrderBy(x => x.Name)
                .ToList();

            return View(products);
        }

        public IActionResult Delivered()
        {
            var products = _context.Products
                .Find(x => x.State == Lib.ProductState.Delivered)
                .OrderBy(x => x.Name)
                .ToList();

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
        public IActionResult AddPosition(ProductPositionViewModel model)
        {
            var dbProduct = _context.Products.FindById(model.Product.Id);
            if (dbProduct.Positions == null)
            {
                dbProduct.Positions = new List<ProductPosition>();
            }
            model.Position.Id = Guid.NewGuid();
            model.Position.PositionAt = DateTimeOffset.Now;
            dbProduct.Positions.Add(model.Position);

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
