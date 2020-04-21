using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class BaseServiceResponse
    {
        public ModelStateDictionary ModelState { get; set; } = new ModelStateDictionary();
        public bool IsSuccess => ModelState.IsValid;

        internal void AddError(string urn, string message)
        {
            this.ModelState.AddModelError(urn, message);
        }

        public string GetAllMessages() => string.Join("\r\n", ModelState.Select(_ => _.Value).SelectMany(_ => _.Errors).Select(_ => _.ErrorMessage));
    }
}
