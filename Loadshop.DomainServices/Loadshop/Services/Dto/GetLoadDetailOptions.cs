using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Dto
{
    public class GetLoadDetailOptions
    {
        /// <param name="loadId">Filter by LoadId.</param>
        public Guid? LoadId { get; set; } = null;
        /// <param name="referenceLoadId">Filter by Reference Load Id.</param>
        public string ReferenceLoadId { get; set; } = null;
        /// <param name="customerIdentUserId">Filter by customer identity server user ID.</param>
        public Guid? CustomerIdentUserId { get; set; } = null;
        /// <param name="transactionTypes">Filter by list of transaction type IDs.</param>
        public List<string> TransactionTypes { get; set; } = null;
        /// <param name="includeContacts">Include associated Contact records.</param>
        public bool IncludeContacts { get; set; } = false;
        /// <param name="includeStops">Include associated Load Stop records.</param>
        public bool IncludeStops { get; set; } = false;
        /// <param name="includeEquipment">Include associated Equipment record.</param>
        public bool IncludeEquipment { get; set; } = false;
        /// <param name="includeDocuments">Include associated Load Document records.</param>
        public bool IncludeDocuments { get; set; } = false;
        /// <param name="includeCurrentStatuses">Include associated Load Current Status records.</param>
        public bool IncludeCurrentStatuses { get; set; } = false;
        /// <summary>
        /// Include Load service Types
        /// </summary>
        public bool IncludeServiceTypes { get; set; } = false;
    }
}
