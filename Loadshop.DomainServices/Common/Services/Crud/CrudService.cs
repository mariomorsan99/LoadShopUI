using AutoMapper;
using Loadshop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Loadshop.DomainServices.Security;

namespace Loadshop.DomainServices.Common.Services.Crud
{
    //To Do: replace default cancellation token with one taken from caller
    public abstract class CrudService<TEntity, TData, TContext> : ICrudService<TData>
        where TEntity : BaseEntity
        //where TContext : DbContextCore
        //This has to be here because of unit test
        //DbContextCore needs to make SaveChanges and SaveChagnes Async Overrideable
        where TContext : Loadshop.DataProvider.LoadshopDataContext
    {
        protected TContext Context { get; }
        protected IMapper Mapper { get; }
        protected ILogger Logger { get; }
        protected IUserContext UserContext { get; }

        public CrudService(TContext context, IMapper mapper, ILogger logger, IUserContext userContext)
        {
            this.Context = context;
            this.Mapper = mapper;
            this.Logger = logger;
            this.UserContext = userContext;
        }

        /// <summary>
        /// Get a collection of data object mapped from the specified entity. Collection has already be enumerated.
        /// </summary>
        /// <param name="filter">Expression to filter the list</param>
        /// <param name="take">Limit the list to a count. Use for server side paging</param>
        /// <param name="skip">Skip a certain count. Use for server side paging</param>
        /// <returns>Returns a CrudResult that encapsulates the data returned and contains any exceptions or warnings</returns>
        public virtual async Task<CrudResult<IEnumerable<TListData>>> GetCollection<TListData>(int? take = null, int? skip = null)
        {
            return await GetCollection<TListData>(null, take, skip);
        }

        protected virtual async Task<CrudResult<IEnumerable<TListData>>> GetCollection<TListData>(Expression<Func<TEntity, bool>> filter, int? take = null, int? skip = null)
        {
            try
            {
                IQueryable<TEntity> query = Context.Set<TEntity>() as IQueryable<TEntity>;

                if (filter != null)
                    query = query.Where(filter);

                if (skip != null)
                    query = query.Skip(skip.Value);
                if (take != null)
                    query = query.Take(take.Value);

                query = await InterceptCollectionQuery(query);

                var entities = await query.ToListAsync();

                var mappedData = Mapper.Map<IEnumerable<TListData>>(entities);

                await GetCollectionLogic(mappedData, entities);

                return Success(mappedData);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogError(ex, $"Unauthorized access getting list of {nameof(TEntity)}");
                return Forbidden<IEnumerable<TListData>>(ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error getting list of {nameof(TEntity)}");
                return Fail<IEnumerable<TListData>>(ex);
            }
        }

        /// <summary>
        /// Override this method to append any custom logic to query to for GetCollection
        /// </summary>
        /// <param name="query">IQuerable at its current state. Append any extra logic such as query.Where and return the new query</param>
        /// <returns>Updated IQuerable</returns>
        protected virtual async Task<IQueryable<TEntity>> InterceptCollectionQuery(IQueryable<TEntity> query)
        {
            return await Task.FromResult(query);
        }

        protected async virtual Task GetCollectionLogic<TListData>(IEnumerable<TListData> data, IEnumerable<TEntity> entity)
        {
            await Task.FromResult<object>(null);
        }

        /// <summary>
        /// Get Single Entity by Key. Intercept the query by overriding GetByKeyQuery to customize query such as using includes
        /// </summary>
        /// <param name="keys">Order object array of keys. Typically will just contain Id</param>
        /// <returns>CrudResult that encapsulates the mapped data object and contains any exceptions or warnings</returns>
        public virtual async Task<CrudResult<TData>> GetByKey(params object[] keys)
        {
            try
            {
                var entity = await GetByKeyQuery(keys);
                var data = Mapper.Map<TEntity, TData>(entity);

                await GetByKeyLogic(data, entity);

                return Success(data);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Erroring GetByKey {nameof(TEntity)}: {ToCommmaDelimitedList(keys)}");
                return Fail<TData>(ex);
            }
        }

        /// <summary>
        /// Customize query to return Entity by Key(s)
        /// </summary>
        /// <param name="keys">Array of entity keys</param>
        /// <returns>Loaded entity</returns>
        protected async virtual Task<TEntity> GetByKeyQuery(params object[] keys)
        {
            return await Context.Set<TEntity>().FindAsync(keys);
        }

        protected async virtual Task GetByKeyLogic(TData data, TEntity entity)
        {
            await Task.FromResult<object>(null);
        }

        /// <summary>
        /// Create new entity by passing data object to map to new entity
        /// </summary>
        /// <param name="data">Data to be used to create new entity</param>
        /// <returns>Data object that represents the created entity</returns>
        public virtual async Task<CrudResult<TData>> Create(TData data)
        {
            try
            {
                var result = CrudResult<TData>.Create();
                var entity = CreateMap(data);

                await CreateLogic(data, entity, result);

                if (result.ModelState.IsValid)
                {
                    var set = Context.Set<TEntity>();
                    set.Add(entity);
                    await Context.SaveChangesAsync(UserContext.UserName, default);

                    result.Data = Mapper.Map<TData>(entity);

                    await GetByKeyLogic(result.Data, entity);
                }
                else
                {
                    result.Status = CrudResultStatus.Invalid;
                }

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogError(ex, $"Unauthorized access creating {nameof(TEntity)}");
                return Forbidden<TData>(ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error Creating {nameof(TEntity)}");
                return Fail<TData>(ex);
            }
        }

        /// <summary>
        /// Override the mapping of the Data object to new entity. By default Automapper will be used
        /// </summary>
        /// <param name="data">Data to be mapped to the new entity</param>
        /// <returns>New entity with data mapped</returns>
        protected virtual TEntity CreateMap(TData data)
        {
            return Mapper.Map<TEntity>(data); ;
        }

        /// <summary>
        /// Hook to add any additonal logic that needs to be executed during entity creation such as validation
        /// </summary>
        /// <param name="data">Data that was mapped to new entity</param>
        /// <param name="entity">Newly created entity that was created in CreateMap</param>
        /// <param name="result">Result object that will allow you to append any warrnings or exceptions</param>
        protected virtual async Task CreateLogic(TData data, TEntity entity, CrudResult<TData> result)
        {
            await Task.FromResult<object>(null);
        }

        /// <summary>
        /// Update an entity and prisist it to the db
        /// </summary>
        /// <param name="data">Data to map to the updating entity</param>
        /// <param name="map">Flag to allow configure a full mapping of data from the data object to the entity</param>
        /// <param name="keys">Key(s) of the entity to update</param>
        /// <returns></returns>
        public virtual async Task<CrudResult<TData>> Update(TData data, bool map, params object[] keys)
        {
            try
            {
                var result = CrudResult<TData>.Create();
                var set = Context.Set<TEntity>();

                var entity = await UpdateQuery(data, keys);

                UpdateMap(data, entity, map);

                await UpdateLogic(data, entity, result);

                if (result.IsValid)
                {
                    await Context.SaveChangesAsync(UserContext.UserName, default);

                    result.Data = Mapper.Map<TData>(entity);

                    await GetByKeyLogic(result.Data, entity);
                }
                else
                {
                    result.Status = CrudResultStatus.Invalid;
                }

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogError(ex, $"Unauthorized access updating {nameof(TEntity)}");
                return Forbidden<TData>(ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error Creating {nameof(TEntity)}");
                return Fail<TData>(ex);
            }
        }

        /// <summary>
        /// Override Query to get the entity to be updated. By default the DbSet.FindAsync(keys) will be used. Use this to include any extra data with entity
        /// </summary>
        /// <param name="dbSet">DbSet for Entity</param>
        /// <param name="data">Data object used to update entity</param>
        /// <param name="keys">Object Array of key(s) of updating entity</param>
        /// <returns></returns>
        protected virtual async Task<TEntity> UpdateQuery(TData data, params object[] keys)
        {
            return await Context.Set<TEntity>().FindAsync(keys);
        }

        /// <summary>
        /// Abstract method to Map back data object to Entity. Must be implemented by concrete class
        /// </summary>
        /// <param name="data">Data object to update entity</param>
        /// <param name="entity">updating entity</param>
        /// <param name="map">bool that to be used to determin if a full mapping is needed</param>
        protected virtual void UpdateMap(TData data, TEntity entity, bool map)
        {
            if (map)
                Mapper.Map(data, entity);
        }

        /// <summary>
        /// Execute any logic when updating an entity such as validation
        /// </summary>
        /// <param name="data">Data object used to update entity</param>
        /// <param name="entity">Entity being updated</param>
        /// <param name="result">Result object that warnings and exceptions can be added to</param>
        protected virtual async Task UpdateLogic(TData data, TEntity entity, CrudResult<TData> result)
        {
            await Task.FromResult<object>(null);
        }

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="keys">Key(s) of entity to delete</param>
        /// <returns></returns>
        public virtual async Task<CrudResult> Delete(params object[] keys)
        {
            try
            {
                var set = Context.Set<TEntity>();
                var entity = await DeleteQuery(keys);

                var result = CrudResult.Create();

                await DeleteLogic(result, entity);

                if (result.IsValid)
                {
                    if (entity is ISoftDelete)
                    {
                        var softDelete = entity as ISoftDelete;
                        softDelete.IsDeleted = true;
                    }
                    else
                    {
                        set.Remove(entity);
                    }
                    await Context.SaveChangesAsync(UserContext.UserName, default);
                }
                else
                {
                    result.Status = CrudResultStatus.Invalid;
                }

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogError(ex, $"Unauthorized access deleting {nameof(TEntity)}");
                return Forbidden<TData>(ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Could not delete {nameof(TEntity)}: {ToCommmaDelimitedList(keys)}");
                return CrudResult.Create(ex);
            }
        }

        /// <summary>
        /// Override query used to fetch entity to be deleted. By Default DbSet.FindAsync will be used
        /// </summary>
        /// <param name="dbSet"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        protected virtual async Task<TEntity> DeleteQuery(params object[] keys)
        {
            return await Context.Set<TEntity>().FindAsync(keys);
        }

        /// <summary>
        /// Execute any logic that needs to happen when an entity is deteled such as deleting relationships and validating entity can be deleted.
        /// </summary>
        /// <param name="entity">Entity to be deleted</param>
        protected virtual async Task DeleteLogic(CrudResult result, TEntity entity)
        {
            await Task.FromResult<object>(null);
        }

        /// <summary>
        /// Helper method to create Successful CrudResult
        /// </summary>
        /// <typeparam name="T">Data type for the CrudResult</typeparam>
        /// <param name="data">Data to be included with result</param>
        /// <returns></returns>
        CrudResult<T> Success<T>(T data)
        {
            return CrudResult<T>.Create(data);
        }

        /// <summary>
        /// Helper mehtod to create failed CrudResult
        /// </summary>
        /// <typeparam name="T">Data type for the CrudResult</typeparam>
        /// <param name="ex">Exception that caused the failed result</param>
        /// <returns></returns>
        CrudResult<T> Fail<T>(Exception ex)
        {
            var result = CrudResult<T>.Create(ex);
            result.Status = CrudResultStatus.Error;
            return result;
        }

        CrudResult<T> Forbidden<T>(Exception ex)
        {
            var result = CrudResult<T>.Create(ex);
            result.Status = CrudResultStatus.Forbidden;
            return result;
        }

        /// <summary>
        /// Helper Method to transform keys to comma delimited list
        /// </summary>
        /// <param name="objs">keys to transform</param>
        /// <returns>String of comma sepperated Ids</returns>
        string ToCommmaDelimitedList(object[] objs)
        {
            return string.Join(",", objs.Select(obj => objs.ToString()));
        }
    }
}
