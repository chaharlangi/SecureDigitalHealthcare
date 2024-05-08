using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace SecureDigitalHealthcare
{
    public static class MyHelper
    {
        public static readonly string ProfilePicturesFolderName = "ProfilePictures";

        public static List<string> GetErrorListFromModelState(ModelStateDictionary modelState)
        {
            var query = from state in modelState.Values
                        from error in state.Errors
                        select error.ErrorMessage;

            var errorList = query.ToList();
            return errorList;
        }
        public static string GetErrorListFromModelStateString(ModelStateDictionary modelState)
        {
            var query = from state in modelState.Values
                        from error in state.Errors
                        select error.ErrorMessage;

            var errorList = query.ToList();

            string message = "Pay attention:\n\n";
            for (int i = 0; i < errorList.Count; i++)
            {
                string? error = errorList[i];
                message += $"{i + 1}: " + error + "\n";
            }
            return message;
        }


        public static Dictionary<string, string> GetSubmittedFormData(HttpRequest request)
        {
            Dictionary<string, string> formData = new Dictionary<string, string>();
            foreach (var key in request.Form.Keys)
            {
                formData.Add(key, request.Form[key]);
            }

            return formData;
        }

        public static string GetRootProjectPath(this IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                return environment.ContentRootPath;
            }
            else
            {
                return Directory.GetParent(environment.ContentRootPath).FullName;
            }
        }
        public static string GetWWWRootProjectPath(this IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                return environment.WebRootPath;
            }
            else
            {
                return environment.ContentRootPath;
            }
        }


    }
}
