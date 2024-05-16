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
            //if (expression.NodeType == ExpressionType.Constant)
            //{
            //    return ((ConstantExpression)expression).Value;
            //}
            //else if (expression.NodeType == ExpressionType.MemberAccess)
            //{
            //    var memberExpression = (MemberExpression)expression;
            //    var propertyInfo = (PropertyInfo)memberExpression.Member;
            //    return propertyInfo.GetValue(((ConstantExpression)memberExpression.Expression).Value);
            //}
            //else
            //{
            //    throw new ArgumentException("Unsupported expression type.");
            //}
        }

    }
}