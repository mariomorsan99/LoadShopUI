using System.Collections.Generic;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Common.Services.Crud
{
    public interface ICrudService<TData>
    {
        Task<CrudResult<TData>> Create(TData data);
        Task<CrudResult> Delete(params object[] keys);
        Task<CrudResult<TData>> GetByKey(params object[] keys);
        Task<CrudResult<IEnumerable<TListData>>> GetCollection<TListData>(int? take = null, int? skip = null);
        Task<CrudResult<TData>> Update(TData data, bool map, params object[] keys);
    }
}