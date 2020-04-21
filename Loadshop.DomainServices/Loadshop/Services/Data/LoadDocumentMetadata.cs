using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    /// <summary>
    /// Metadata about a load document that is stored in Loadshop, note the actual doc and path are stored in KBXL Document Service
    /// </summary>
    public class LoadDocumentMetadata
    {
        public Guid LoadDocumentId { get; set; }
        public LoadDocumentTypeData LoadDocumentType { get; set; }
        public Guid LoadId { get; set; }
        public string FileName { get; set; }
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
    }
}
    