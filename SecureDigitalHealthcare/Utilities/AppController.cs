using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Reflection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace SecureDigitalHealthcare.Utilities
{
    public class AppController<TController> where TController : Controller
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public AppController(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public string GetActionUrl(string methodName)
        {
            string controllerName = typeof(TController).Name.Replace("Controller", "");

            return $"/{controllerName}/{methodName}";
        }

        public string CallAction<TAction>(Expression<Func<TController, TAction>> actionExpression, string? protocol = null)
        {
            var methodName = ((MethodCallExpression)actionExpression.Body).Method.Name;
            var controllerName = typeof(TController).Name.Replace("Controller", "");

            var actionContext = _actionContextAccessor.ActionContext;
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
            var routeValues = GetRouteValues(actionExpression);

            if (protocol != null)
            {
                return urlHelper.Action(methodName, controllerName, routeValues, protocol);
            }

            return urlHelper.Action(methodName, controllerName, routeValues);
        }

        private Dictionary<string, object> GetRouteValues<TAction>(Expression<Func<TController, TAction>> actionExpression)
        {
            var methodCallExpression = (MethodCallExpression)actionExpression.Body;
            var method = methodCallExpression.Method;

            var parameters = method.GetParameters()
                .Zip(methodCallExpression.Arguments, (param, arg) =>
                {
                    var name = param.Name;
                    var value = GetValueFromExpression(arg);
                    return (name, value);
                })
                .ToDictionary(tuple => tuple.name, tuple => tuple.value);

            // Handle nested properties if the parameter is a complex object
            var routeValues = new Dictionary<string, object>();
            foreach (var parameter in parameters)
            {
                if (parameter.Value != null && !parameter.Value.GetType().IsPrimitive && !(parameter.Value is string) && !(parameter.Value is DateTime))
                {
                    var nestedRouteValues = GetRouteValuesFromObject(parameter.Value);
                    foreach (var nestedValue in nestedRouteValues)
                    {
                        routeValues[nestedValue.Key] = nestedValue.Value;
                    }
                }
                else
                {
                    routeValues[parameter.Key] = parameter.Value;
                }
            }

            return routeValues;
        }

        private object GetValueFromExpression(Expression expression)
        {
            LambdaExpression lambdaExpression = Expression.Lambda(expression);
            Delegate compiledDelegate = lambdaExpression.Compile();
            object value = compiledDelegate.DynamicInvoke();
            return value;
        }

        private Dictionary<string, object> GetRouteValuesFromObject(object obj)
        {
            var routeValues = new Dictionary<string, object>();

            foreach (var property in obj.GetType().GetProperties())
            {
                var name = property.Name;
                var value = property.GetValue(obj);
                if (value is DateTime dateTimeValue)
                {
                    routeValues.Add(name, dateTimeValue.ToString("o")); // ISO 8601 format
                }
                else
                {
                    routeValues.Add(name, value);
                }
            }

            return routeValues;
        }
    }

    public class AppControllerDepricated<TController> where TController : Controller
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public AppControllerDepricated(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public string GetActionUrl(string methodName)
        {
            string controllerName = typeof(TController).Name.Replace("Controller", "");

            return $"/{controllerName}/{methodName}";
        }

        public string CallAction<TAction>(Expression<Func<TController, TAction>> actionExpression, string? protocol = null)
        {
            var methodName = ((MethodCallExpression)actionExpression.Body).Method.Name;
            var controllerName = typeof(TController).Name.Replace("Controller", "");

            var actionContext = _actionContextAccessor.ActionContext;
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
            var routeValues = GetRouteValues(actionExpression);

            if (protocol != null)
            {
                return urlHelper.Action(methodName, controllerName, routeValues, protocol);
            }

            return urlHelper.Action(methodName, controllerName, routeValues);
        }
        private object GetRouteValues<TAction>(Expression<Func<TController, TAction>> actionExpression)
        {
            var methodCallExpression = (MethodCallExpression)actionExpression.Body;
            var method = methodCallExpression.Method;

            var parameters = method.GetParameters()
                .Zip(methodCallExpression.Arguments, (param, arg) =>
                {
                    var name = param.Name;
                    var value = GetValueFromExpression(arg);
                    return (name, value);
                })
                .ToDictionary(tuple => tuple.name, tuple => tuple.value);

            return parameters;
        }

        private object GetValueFromExpression(Expression expression)
        {// Convert the expression into a lambda expression
            LambdaExpression lambdaExpression = Expression.Lambda(expression);

            // Compile the lambda expression into a delegate
            Delegate compiledDelegate = lambdaExpression.Compile();

            // Invoke the delegate To get the value
            object value = compiledDelegate.DynamicInvoke();

            return value;
        }

    }
}