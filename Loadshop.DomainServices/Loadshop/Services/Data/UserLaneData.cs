using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserLaneData
    {
        public string UserLaneId { get; set; }
        public string UserId { get; set; }
        public string OrigCity { get; set; }
        public string OrigState { get; set; }
        public string OrigCountry { get; set; }
        public decimal? OrigLat { get; set; }
        public decimal? OrigLng { get; set; }
        public int? OrigDH { get; set; }
        public string DestCity { get; set; }
        public string DestState { get; set; }
        public string DestCountry { get; set; }
        public decimal? DestLat { get; set; }
        public decimal? DestLng { get; set; }
        public int? DestDH { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }

        public List<UserLaneLoadData> UserLaneLoads { get; set; }
        public List<UserLaneMessageTypeData> UserLaneMessageTypes { get; set; }
        public List<string> EquipmentIds { get; set; }
        public SearchTypeData SearchType { get; set; }
    }
}
