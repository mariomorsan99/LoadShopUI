using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class SecurityAppActionData : BaseData
    {
        [Required]
        [StringLength(250)]
        public string AppActionId { get; set; }
        public string AppActionDescription { get; set; }
    }
}
