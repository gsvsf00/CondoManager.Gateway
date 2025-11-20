using CondoManager.Api.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CondoManager.Api.ModelBinders
{
    public class LoginRequestBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var request = bindingContext.HttpContext.Request;

            // Read the body
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;

            string? emailOrPhone = root.TryGetProperty("emailOrPhone", out var val)
                ? val.GetString()
                : null;

            string? password = root.TryGetProperty("password", out var passVal)
                ? passVal.GetString()
                : null;

            var model = new LoginRequest
            {
                Password = password ?? string.Empty
            };

            if (!string.IsNullOrEmpty(emailOrPhone))
            {
                if (Regex.IsMatch(emailOrPhone, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    model.Email = emailOrPhone;
                }
                else
                {
                    model.Phone = Regex.Replace(emailOrPhone, @"\D", ""); // keep only digits
                }
            }

            bindingContext.Result = ModelBindingResult.Success(model);
        }
    }
}
