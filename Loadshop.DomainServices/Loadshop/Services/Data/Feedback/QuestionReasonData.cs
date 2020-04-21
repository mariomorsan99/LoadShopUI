using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class QuestionReasonData
    {
        public int? QuestionReasonId { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public bool DisplayedNegative { get; set; }
        public bool DisplayedPositive { get; set; }
        public bool IsActive { get; set; }
    }
}
