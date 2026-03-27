using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
            var rol = context.HttpContext.Session.GetInt32("Rol");

            if (rol == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (!_rolesPermitidos.Contains(rol.Value))
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}