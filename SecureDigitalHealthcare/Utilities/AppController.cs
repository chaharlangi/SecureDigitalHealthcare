using SecureDigitalHealthcare.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace SecureDigitalHealthcare.Utilities
{
    public class UrlHelper<TController> where TController : Controller
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public UrlHelper(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public string Action<TAction>(Expression<Func<TController, TAction>> actionExpression, object routeValues = null)
        {
            var methodName = ((MethodCallExpression)actionExpression.Body).Method.Name;
            var controllerName = typeof(TController).Name.Replace("Controller", "");

            var actionContext = _actionContextAccessor.ActionContext;
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

            return urlHelper.Action(methodName, controllerName, routeValues);
        }
    }
}
