using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadTransactionData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypeData TransactionType { get; set; }
        public string Scac { get; set; }
        public decimal? LineHaulRate { get; set; }
        public decimal? FuelRate { get; set; }
        public Guid? UserId { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public long? LoadBoardId { get; set; }
    }
}
