using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services.Dto
{
    public class LoadSearchCriteria
    {

        public string UserLaneId { get; set; }
        public double? OrigLat { get; set; }
        public double? OrigLng { get; set; }
        public string OrigCity { get; set; }
        public string OrigState { get; set; }
        public string OrigCountry { get; set; }
        public int OrigDH { get; set; }
        public double? DestLat { get; set; }
        public double? DestLng { get; set; }
        public string destCity { get; set; }
        public string DestState { get; set; }
        public string DestCountry { get; set; }
        public int DestDH { get; set; }
        public DateTime? OrigDateStart { get; set; }
        public DateTime? OrigDateEnd { get; set; }
        public DateTime? DestDateStart { get; set; }
        public DateTime? DestDateEnd { get; set; }
        public string EquipString { get; set; }
        public string EquipmentType { get; set; }
        public string QuickSearch { get; set; }
        public IEnumerable<int> ServiceTypes { get; set; }
        //SearchType?: SearchTypeData;
    }
}
