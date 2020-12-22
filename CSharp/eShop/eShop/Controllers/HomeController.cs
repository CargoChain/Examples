using eShop.Data;
using eShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ShopContext _context;

        public HomeController(ILogger<HomeController> logger, ShopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .FindAll()
                .OrderBy(x => x.Name)
                .ToList();

            return View(products);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Product product)
        {
            product.Id = Guid.NewGuid();
            product.State = ProductState.Available;
            _context.Products.Insert(product);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Order(Guid id)
        {
            var product = _context.Products.FindById(id);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Order(Product product)
        {
            var dbProduct = _context.Products.FindById(product.Id);
            dbProduct.DeliveryAddress = product.DeliveryAddress;
            dbProduct.State = ProductState.Ordered;
            _context.Products.Update(dbProduct);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
