using System;
using System.Linq;
using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using Loadshop.Web.API.Models.Loads;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Loadshop.DomainServices.Utilities;
using Loadshop.DomainServices.Exceptions;
using Loadshop.API.Models;
using Loadshop.Web.API.Security;
using System.Threading.Tasks;
using Loadshop.Web.API.Utility;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.Web.API.Models;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = AuthorizationConstants.IsCarrierOrShipperPolicy)]
    public class LoadsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ILoadService _loadService;
        private readonly IUserProfileService _userProfileService;
        private readonly IUserContext _userContext;
        private readonly IConfigurationRoot _configuration;
        private readonly Utilities _util;

        public LoadsController(IMapper mapper, ILoadService loadService, IUserProfileService userProfileService, IUserContext userContext, IConfigurationRoot configuration, Utilities util)
        {
            _mapper = mapper;
            _loadService = loadService;
            _userProfileService = userProfileService;
            _userContext = userContext;
            _configuration = configuration;
            _util = util;
        }

        [HttpGet]
        [Route("api/[controller]/{id}")]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 200)]
        [Authorize(Policy = AuthorizationConstants.CanAccessLoadDetail)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(id, _userContext.UserId.Value);

                LoadDetailViewModel result = _mapper.Map<LoadDetailViewModel>(load);
                result.ViewOnly = await this._userProfileService.IsViewOnlyUserAsync(_userContext.UserId.Value);

                return Success(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<ResponseMessage<LoadDetailViewModel>>(ex);
            }
        }

        [HttpPost]
        [Route("api/[controller]/{loadId}/audit/{auditTypeId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AuditLoad(Guid loadId, string auditTypeId)
        {
            if (Enum.TryParse(auditTypeId, out AuditTypeData auditTypeEnum))
            {
                var numRowsInserted = await _loadService.CreateLoadAuditLogEntryAsync(loadId, auditTypeEnum);
                return Success(numRowsInserted);
            }

            return Error<int>(new ArgumentException("Invalid AuditTypeId provided"));

        }

        [HttpGet]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(ResponseMessage<PageableResponse<LoadData>>), 200)]
        [Authorize(Policy = AuthorizationConstants.CanAccessCarrierLoadsPolicy)]
        public async Task<IActionResult> Search(SearchTypeData searchType)
        {
            return await Search(searchType, null);
        }

        [HttpPost]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(ResponseMessage<PageableResponse<LoadData>>), 200)]
        [Authorize(Policy = AuthorizationConstants.CanAccessCarrierLoadsPolicy)]
        public async Task<IActionResult> Search(SearchTypeData searchType, [FromBody] LoadSearchCriteria loadSearchCriteria)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("Invalid UserId");
                }

                switch (searchType)
                {
                    case SearchTypeData.UserLanes:
                        // We can do this out of order of a typical linq query b/c the Pageable Query will handle applying it to the IQueryable in the correct order.
                        var pageableLoads = _loadService
                                                .GetAllOpenLoads(_userContext.UserId.Value)
                                                .HandleSorting(Request)
                                                .HandlePaging(Request);

                        if (loadSearchCriteria != null)
                        {
                            pageableLoads.Filter(QueryFilters.GetCarrierLoadFilter(loadSearchCriteria));
                        }

                        return Success(await pageableLoads.ToPageableResponse());

                    case SearchTypeData.Booked:
                        {
                            pageableLoads = _loadService
                                                .GetBookedLoads(_userContext.UserId.Value)
                                                .HandleSorting(Request)
                                                .HandlePaging(Request);

                            if (loadSearchCriteria != null)
                            {
                                pageableLoads.Filter(QueryFilters.GetCarrierLoadFilter(loadSearchCriteria));
                            }

                            var pageableResult = await pageableLoads.ToPageableResponse();

                            if (string.IsNullOrWhiteSpace(Request.Query[PageableQueryConstants.OrderByQuery]))
                            {
                                var loads = pageableResult.Data;

                                var visibilityTypes = _util.GetVisibilityTypes(_userContext.UserId.Value);

                                if (visibilityTypes != null && visibilityTypes.Count > 0)
                                {
                                    var loadsToProcess = loads.Where(x => x.DestLateDtTm >= DateTime.Now).ToList();

                                    if (loadsToProcess.Count() > 0)
                                    {
                                        _util.CheckLoadsForVisibility(loadsToProcess, _userContext.Token, visibilityTypes);
                                    }
                                }

                                var loadsWithWarnings = loads.Where(x => x.ShowVisibilityWarning == true).OrderBy(x => x.OriginLateDtTm).ToList();
                                var loadsNoWarnings = loads.Where(x => x.ShowVisibilityWarning == false).OrderByDescending(x => x.OriginLateDtTm).ToList();

                                pageableResult.Data.Clear();
                                pageableResult.Data.AddRange(loadsWithWarnings);
                                pageableResult.Data.AddRange(loadsNoWarnings);
                            }
                            return Success(pageableResult);
                        }
                    case SearchTypeData.Delivered:

                        pageableLoads = _loadService
                                            .GetDeliveredLoads(_userContext.UserId.Value)
                                            .HandleSorting(Request)
                                            .HandlePaging(Request);

                        if (loadSearchCriteria != null)
                        {
                            pageableLoads.Filter(QueryFilters.GetCarrierLoadFilter(loadSearchCriteria));
                        }

                        return Success(await pageableLoads.ToPageableResponse());

                    default:
                        return Error<List<LoadData>>(new Exception("SearchType not valid"));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<ResponseMessage<List<LoadData>>>(ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("api/[controller]/num-requiring-visibility-info")]
        [ProducesResponseType(typeof(ResponseMessage<VisibilityBadge>), 200)]
        public IActionResult GetNumLoadsRequiringVisibilityInfo()
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("Invalid UserId");
                }

                return Success(_loadService.GetNumLoadsRequiringVisibilityInfo(_userContext.UserId.Value));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPut]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(ResponseMessage<LoadData>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Marketplace_Loads_Book)]
        public async Task<IActionResult> Put([FromBody]LoadData load)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new Exception("Invalid UserId");
                }

                if (await this._userProfileService.IsViewOnlyUserAsync(_userContext.UserId.Value))
                {
                    return Error<LoadData>(new ValidationException("You currently have view only access to Loadshop and cannot book loads at this time."));
                }

                try
                {
                    return Success(_loadService.PendingAcceptLoad(load, _userContext.UserId.Value));
                }
                catch (ValidationException valEx)
                {
                    return Error<LoadData>(valEx);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<ResponseMessage<LoadData>>(ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPut]
        [Route("api/[controller]/visibilitydata")]
        [ProducesResponseType(typeof(ResponseMessage<SaveVisibilityDataResponse>), 200)]
        public async Task<IActionResult> PutVisibilityData([FromBody]LoadData load)
        {
            try
            {
                if (!load.LoadId.HasValue)
                {
                    throw new Exception("Invalid LoadId");
                }

                if (!_userContext.UserId.HasValue)
                {
                    throw new Exception("Invalid UserId");
                }

                if (await this._userProfileService.IsViewOnlyUserAsync(_userContext.UserId.Value))
                {
                    return Error<SaveVisibilityDataResponse>(new ValidationException("You currently have view only access to Loadshop."));
                }

                try
                {
                    var loadClaimData = new LoadClaimData();

                    if (!string.IsNullOrWhiteSpace(load.VisibilityPhoneNumber) || !string.IsNullOrWhiteSpace(load.VisibilityTruckNumber))
                    {
                        loadClaimData = _mapper.Map<LoadClaimData>(_util.UpdateVisibilityData(load, _userContext.UserId.Value));
                    }

                    var badge = _loadService.GetNumLoadsRequiringVisibilityInfo(_userContext.UserId.Value);

                    return Success(new SaveVisibilityDataResponse
                    {
                        LoadClaim = loadClaimData,
                        VisibilityBadge = badge
                    });
                }
                catch (ValidationException valEx)
                {
                    return Error<SaveVisibilityDataResponse>(valEx);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
