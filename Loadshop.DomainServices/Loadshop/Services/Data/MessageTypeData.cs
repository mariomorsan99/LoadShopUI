using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class MessageTypeData : BaseData
    {
        [Key]
        public string MessageTypeId { get; set; }
        public string MessageTypeDesc { get; set; }
    }
}
