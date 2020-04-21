using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserLaneLoadData : BaseData
    {
        public Guid UserLaneLoadId { get; set; }
        public Guid UserLaneId { get; set; }
        public string LoadId { get; set; }
        public int OrigDH { get; set; }
        public int DestDH { get; set; }
        public bool IsVisible { get; set; }

        public UserLaneData UserLane { get; set; }
        public LoadDetailData Load { get; set; }
    }
}
