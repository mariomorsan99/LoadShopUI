using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserCommunicationShipperData : BaseData
    {
        public Guid? UserCommuncationShipperId { get; set; }
        public Guid? UserCommuncationId { get; set; }
        public Guid CustomerId { get; set; }
    }
}
