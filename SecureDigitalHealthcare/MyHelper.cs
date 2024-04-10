using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SecureDigitalHealthcare
{
    public static class MyHelper
    {
        public static List<string> GetErrorListFromModelState
                                              (ModelStateDictionary modelState)
        {
            var query = from state in modelState.Values
                        from error in state.Errors
                        select error.ErrorMessage;

            var errorList = query.ToList();
            return errorList;
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
    }
}
