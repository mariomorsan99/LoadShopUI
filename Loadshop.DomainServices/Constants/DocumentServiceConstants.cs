using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Loadshop.DomainServices.Constants
{
    public class DocumentServiceConstants
    {
        /// <summary>
        /// Document App Code used internally for Document Service
        /// </summary>
        public const string AppCode = "Loadshop";

        public const string Property_Name_LoadshopLoadId = "Loadshop Load Id";
        public const string Property_Name_BillingLoadId = "Billing Load Id";
        public const string Property_Name_PlatformPlusLoadId = "PlatformPlus Load Id";
        public const string Property_Name_ReferenceLoadId = "Reference Load Id";
    }

    /// <summary>
    /// Property ID used internally for Document Service
    /// </summary>
    public enum LoadshopDocumentServicePropertyTypes
    {
        LoadId = 400
    }

    /// <summary>
    /// Document Types used internally for Document Service
    /// </summary>
    public enum LoadshopDocumentServiceDocumentTypes
    {
        [Description("Proof of Delivery")]
        ProofOfDelivery = 1001,
        [Description("Other")]
        Other = 1003
    }
}
