using Loadshop.DomainServices.Loadshop.Services.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Loadshop.Web.API.Models
{
    public class PostLoadsClientResponse
    {
        public List<ShippingLoadData> PostedLoads { get; set; }
        public ValidationProblemDetails ValidationProblemDetails { get; set; }
    }
}
