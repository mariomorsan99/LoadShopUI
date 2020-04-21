using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IEquipmentService
    {
        List<EquipmentData> GetEquipment();
    }
}
