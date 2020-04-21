using Loadshop.DomainServices.Common.Services.Crud;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Loadshop.Web.Controllers
{
    [Route("api/{controller}")]
    public abstract class BaseCrudController<TData> : BaseController
    {
        protected ICrudService<TData> CrudService { get; }
        protected const string TakeQuery = "take";
        protected const string SkipQuery = "skip";
        protected const string FilterQuery = "query";

        public BaseCrudController(ICrudService<TData> crudService)
        {
            CrudService = crudService;
        }

        protected virtual async Task<IActionResult> GetCollection<TListData>()
        {
            var take = TryParse(Request.Query[TakeQuery]);
            var skip = TryParse(Request.Query[SkipQuery]);
            
            CrudResult<IEnumerable<TListData>> result = await GetCollectionServiceCall<TListData>(take, skip);

            switch (result.Status)
            {
                case CrudResultStatus.Successful:
                    return Success(result.Data);
                case CrudResultStatus.Forbidden:
                    return Forbidden<IEnumerable<TListData>>(result.Exceptions.FirstOrDefault()?.Message);
                case CrudResultStatus.Error:
                    return Error<IEnumerable<TListData>>(result.Exceptions.FirstOrDefault());
                default:
                    throw new System.Exception("Invalid Result from service");
            }
        }

        protected virtual async Task<CrudResult<IEnumerable<TListData>>> GetCollectionServiceCall<TListData>(int? take, int? skip)
        {
            return await CrudService.GetCollection<TListData>(take, skip);
        }

        protected virtual async Task<IActionResult> GetByKey(params object[] keys)
        {
            var result = await CrudService.GetByKey(keys);

            switch (result.Status)
            {
                case CrudResultStatus.Successful:
                    return Success(result.Data);
                case CrudResultStatus.Forbidden:
                    return Forbidden<TData>(result.Exceptions.FirstOrDefault()?.Message);
                case CrudResultStatus.Error:
                    return Error<TData>(result.Exceptions.FirstOrDefault());
                default:
                    throw new Exception("Invalid Result from service");
            }
        }

        protected virtual async Task<IActionResult> Update(TData data, bool map, params object[] keys)
        {
            var result = await CrudService.Update(data, true, keys);

            switch (result.Status)
            {
                case CrudResultStatus.Successful:
                    return Success(result.Data);
                case CrudResultStatus.Forbidden:
                    return Forbidden<TData>(result.Exceptions.FirstOrDefault()?.Message);
                case CrudResultStatus.Error:
                    return Error<TData>(result.Exceptions.FirstOrDefault());
                case CrudResultStatus.Invalid:
                    return BadRequest(new API.Models.ResponseMessage<TData>(), result.ModelState);
                default:
                    throw new Exception("Invalid Result from service");
            }
        }

        protected virtual async Task<IActionResult> Create(TData data)
        {
            var result = await CrudService.Create(data);

            switch (result.Status)
            {
                case CrudResultStatus.Successful:
                    return Success(result.Data);
                case CrudResultStatus.Forbidden:
                    return Forbidden<TData>(result.Exceptions.FirstOrDefault()?.Message);
                case CrudResultStatus.Error:
                    return Error<TData>(result.Exceptions.FirstOrDefault());
                case CrudResultStatus.Invalid:
                    return BadRequest(new API.Models.ResponseMessage<TData>(), result.ModelState);
                default:
                    throw new Exception("Invalid Result from service");
            }
        }

        protected virtual async Task<IActionResult> Delete(params object[] keys)
        {
            CrudResult result = await CrudService.Delete(keys);

            switch (result.Status)
            {
                case CrudResultStatus.Successful:
                    return Success(result.Successful);
                case CrudResultStatus.Forbidden:
                    return Forbidden<TData>(result.Exceptions.FirstOrDefault()?.Message);
                case CrudResultStatus.Error:
                    return Error<TData>(result.Exceptions.FirstOrDefault());
                case CrudResultStatus.Invalid:
                    return BadRequest(new API.Models.ResponseMessage<TData>(), result.ModelState);
                default:
                    throw new Exception("Invalid Result from service");
            }
        }

        protected async Task<IActionResult> ResultHelper<T>(Func<Task<CrudResult<T>>> execute)
        {
            var result = await execute();

            switch (result.Status)
            {
                case CrudResultStatus.Successful:
                    return Success(result.Data);
                case CrudResultStatus.Forbidden:
                    return Forbidden<TData>(result.Exceptions.FirstOrDefault()?.Message);
                case CrudResultStatus.Error:
                    return Error<TData>(result.Exceptions.FirstOrDefault());
                case CrudResultStatus.Invalid:
                    return BadRequest(new API.Models.ResponseMessage<T>(), result.ModelState);
                default:
                    throw new Exception("Invalid Result from service");
            }
        }

        protected int? TryParse(string str)
        {
            int outInt;

            if (int.TryParse(str, out outInt))
                return outInt;

            return null;
        }

    }
}
