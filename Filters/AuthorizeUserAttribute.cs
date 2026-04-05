using ECOMMERCE_NEXOSOFT.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Filters
{
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        private readonly int[] _rolesPermitidos;

        public AuthorizeUserAttribute(params int[] rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var idUsuario = context.HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetService(typeof(NexosoftDbContext)) as NexosoftDbContext;

            if (dbContext == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var usuario = dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefault(u => u.IdUsuario == idUsuario.Value);

            if (usuario == null)
            {
                context.HttpContext.Session.Clear();
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Sincroniza el rol actual de BD con la sesión
            context.HttpContext.Session.SetInt32("Rol", usuario.IdRol);

            if (!_rolesPermitidos.Contains(usuario.IdRol))
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}