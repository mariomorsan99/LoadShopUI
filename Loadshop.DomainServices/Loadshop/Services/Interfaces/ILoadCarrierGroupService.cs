using System;
using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ILoadCarrierGroupService
    {
        LoadCarrierGroupDetailData GetLoadCarrierGroup(long loadCarrierGroupId);
        List<LoadCarrierGroupDetailData> GetLoadCarrierGroups();
        IList<ShippingLoadCarrierGroupData> GetLoadCarrierGroupsForLoad(Guid loadId);
        SaveLoadCarrierGroupResponse UpdateLoadCarrierGroup(LoadCarrierGroupDetailData group, string username);
        SaveLoadCarrierGroupResponse CreateLoadCarrierGroup(LoadCarrierGroupDetailData group, string username);
        void DeleteLoadCarrierGroup(long id);

        List<LoadCarrierGroupCarrierData> GetLoadCarrierGroupCarriers(long loadCarrierGroupId);
        List<LoadCarrierGroupCarrierData> AddLoadCarrierGroupCarriers(List<LoadCarrierGroupCarrierData> carriers, string username);
        void DeleteLoadCarrierGroupCarrier(long loadCarrierGroupCarrierId);
        void DeleteAllLoadCarrierGroupCarriers(long loadCarrierGroupId);
        List<LoadCarrierGroupTypeData> GetLoadCarrierGroupTypes();
    }
}
