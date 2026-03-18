using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECOMMERCE_NEXOSOFT.Filters
{
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        private readonly int[] _roles;

        public AuthorizeUserAttribute(params int[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            var usuario = session.GetString("Usuario");
            var rol = session.GetInt32("Rol");

            if (usuario == null || rol == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(rol.Value))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}