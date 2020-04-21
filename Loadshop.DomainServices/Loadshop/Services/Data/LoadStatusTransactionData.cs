using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadStatusTransactionData
    {
        public Guid MessageId { get; set; }
        public DateTimeOffset MessageTime { get; set; }
        public Guid LoadId { get; set; }
        public DateTimeOffset TransactionDtTm { get; set; }
    }
}
