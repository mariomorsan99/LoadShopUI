using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ISpecialInstructionsService
    {
        Task<List<SpecialInstructionData>> GetSpecialInstructionsAsync();
        Task<SpecialInstructionData> GetSpecialInstructionAsync(long specialInstructionsId);
        Task<SaveSpecialInstructionResponse> CreateSpecialInstructionAsync(SpecialInstructionData instruction, string username);
        Task DeleteSpecialInstructionAsync(long id);
        Task<SaveSpecialInstructionResponse> UpdateSpecialInstructionAsync(SpecialInstructionData instruction, string username);
        Task<List<SpecialInstructionData>> GetSpecialInstructionsForLoadAsync(LoadEntity load);
    }
}
