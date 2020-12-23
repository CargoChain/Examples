using eShop.Data;
using eShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ShopContext _context;
        private readonly CargoChainService _cargoChainService;
        private readonly CargoChainConfiguration _cargoChainConfiguration;

        public HomeController(
            ILogger<HomeController> logger,
            ShopContext context,
            CargoChainService cargoChainService,
            IOptionsMonitor<CargoChainConfiguration> optionsMonitor)
        {
            _cargoChainConfiguration = optionsMonitor.CurrentValue;
            _logger = logger;
            _context = context;
            _cargoChainService = cargoChainService;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .FindAll()
                .OrderBy(x => x.Name)
                .ToList();

            ViewBag.PublicViewUrl = _cargoChainConfiguration.PublicViewUrl;

            return View(products);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product)
        {
            product.Id = Guid.NewGuid();
            product.State = ProductState.Available;

            var cargoChainProfile = await _cargoChainService.CreateProductProfile(product);
            product.CargoChainProfilePublicId = cargoChainProfile.ProfilePublicId;
            product.CargoChainProfileSecretId = cargoChainProfile.ProfileSecretId;

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
        public async Task<IActionResult> Order(Product product)
        {
            var dbProduct = _context.Products.FindById(product.Id);
            dbProduct.DeliveryAddress = product.DeliveryAddress;
            dbProduct.State = ProductState.Ordered;

            await _cargoChainService.OrderProductProfile(dbProduct);
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
