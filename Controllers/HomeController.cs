using System.Diagnostics;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ECOMMERCE_NEXOSOFT.Data;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NexosoftDbContext _context;

        public HomeController(ILogger<HomeController> logger, NexosoftDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categoria
                .Take(6)
                .ToListAsync();

            var productos = await _context.Productos
     .Where(p => p.Favorito)
     .Take(8)
     .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Categorias = categorias,
                ProductosDestacados = productos
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult NotFoundPage()
        {
            return View("NotFound");
        }
        public IActionResult Error500()
        {
            return View("Error500");
        }
    }
}
