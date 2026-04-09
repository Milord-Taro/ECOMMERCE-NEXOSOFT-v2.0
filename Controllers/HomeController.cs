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
                .Select(c => new Categorium
                {
                    IdCategoria = c.IdCategoria,
                    CodCategoria = c.CodCategoria,
                    NombreCategoria = c.NombreCategoria ?? string.Empty,
                    Descripcion = c.Descripcion ?? string.Empty,
                    VisiblePublico = c.VisiblePublico
                })
                .Take(6)
                .ToListAsync();

            var productos = await _context.Productos
                .Where(p => p.VisiblePublico)
                .Select(p => new Producto
                {
                    IdProducto = p.IdProducto,
                    CodProducto = p.CodProducto,
                    IdCategoria = p.IdCategoria,
                    IdSubcategoria = p.IdSubcategoria,
                    IdTienda = p.IdTienda,
                    NombreProducto = p.NombreProducto ?? string.Empty,
                    DescripcionCorta = p.DescripcionCorta ?? string.Empty,
                    SkuProducto = p.SkuProducto ?? string.Empty,
                    CodigoBarrasProducto = p.CodigoBarrasProducto ?? string.Empty,
                    UnidadMedidaProducto = p.UnidadMedidaProducto ?? string.Empty,
                    MarcaProducto = p.MarcaProducto ?? string.Empty,
                    Favorito = p.Favorito,
                    VisiblePublico = p.VisiblePublico,
                    PrecioVentaProducto = p.PrecioVentaProducto
                })
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
        public IActionResult Error404()
        {
            return View("Error404");
        }

        public IActionResult Error500()
        {
            return View("Error500");
        }

        public async Task<IActionResult> Categorias()
        {
            var categorias = await _context.Categoria
                .Where(c => c.VisiblePublico)
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();

            return View(categorias);
        }

        public IActionResult Ofertas()
        {
            return View();
        }

        public IActionResult Nosotros()
        {
            return View();
        }

        public IActionResult Contacto()
        {
            return View();
        }
    }
}