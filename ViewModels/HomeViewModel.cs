using ECOMMERCE_NEXOSOFT.Models;

namespace ECOMMERCE_NEXOSOFT.ViewModels
{
    public class HomeViewModel
    {
        public List<Categorium> Categorias { get; set; } = new();

        public List<Producto> ProductosDestacados { get; set; } = new();
    }
}