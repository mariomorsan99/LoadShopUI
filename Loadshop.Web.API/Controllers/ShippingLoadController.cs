using System;
using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using Microsoft.AspNetCore.Authorization;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Exceptions;
using System.Net;
using Loadshop.Web.API.Models;
using Loadshop.Web.API.Security;
using Loadshop.API.Models;
using System.Threading.Tasks;
using AutoMapper;
using Loadshop.Web.API.Models.Shipping;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Enum;
using Loadshop.Web.API.Utility;
using System.Linq;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{

    [Route("api/[controller]")]
    [Authorize(Policy = AuthorizationConstants.IsShipperPolicy)]
    public class ShippingLoadController : BaseController
    {
        private readonly IShippingService _shippingService;
        private readonly ILoadService _loadService;
        private readonly ILocationService _locationService;
        private readonly IUserContext _userContext;
        private readonly IUserProfileService _userProfileService;
        private readonly IMapper _mapper;

        public ShippingLoadController(IShippingService shippingService,
            ILoadService loadService,
            ILocationService locationService,
            IUserContext userContext,
            IUserProfileService userProfileService,
            IMapper mapper)
        {
            _shippingService = shippingService;
            _loadService = loadService;
            _locationService = locationService;
            _userContext = userContext;
            _userProfileService = userProfileService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{loadId}")]
        [ProducesResponseType(typeof(ResponseMessage<ShippingLoadData>), 200)]
        public IActionResult GetById(Guid loadId)
        {
            return Success(_shippingService.GetLoad(loadId));
        }

        [HttpGet]
        [Route("GetLoadsForHomeTab")]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shopit_Load_View_Active)]
        [ProducesResponseType(typeof(ResponseMessage<List<ShippingLoadData>>), 200)]
        public IActionResult GetLoadsForHomeTab()
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("Invalid UserId");
                }

                return Success(_shippingService.GetLoadsForHomeTab(_userContext.UserId.Value));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        [Route("GetLoadsBySearchType/{searchType}")]
        [ProducesResponseType(typeof(ResponseMessage<List<ShippingLoadViewData>>), 200)]
        public async Task<IActionResult> GetLoadsBySearchType(ShipperSearchTypeData searchType, [FromBody]LoadSearchCriteria loadSearchCriteria)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("Invalid UserId");
                }

                var pageableQuery = _shippingService.GetLoadsBySearchType(searchType);

                if (loadSearchCriteria != null)
                {
                    pageableQuery.Filter(DomainServices.Utilities.QueryFilters.GetShippingLoadFilter(loadSearchCriteria));
                }

                var response = await pageableQuery
                                        .HandleSorting(Request)
                                        .HandlePaging(Request)
                                        .ToPageableResponse();

                return Success(response);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet]
        [Route("GetLoadsBySearchType/{searchType}")]
        [ProducesResponseType(typeof(ResponseMessage<List<ShippingLoadViewData>>), 200)]
        public async Task<IActionResult> GetLoadsBySearchType(ShipperSearchTypeData searchType)
        {
            return await GetLoadsBySearchType(searchType, null);
        }

        [HttpPost]
        [Route("RemoveLoad/{loadId}")]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Remove)]
        [ProducesResponseType(typeof(ResponseMessage<ShippingLoadData>), 200)]
        public async Task<IActionResult> RemoveLoad([FromRoute] Guid loadId)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("Invalid UserId");
                }

                return Success(await _shippingService.RemoveLoad(loadId, _userContext.UserName));

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        [Route("RemoveCarrier/{loadId}")]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shop_It_Load_Edit)]
        [ProducesResponseType(typeof(ResponseMessage<ShippingLoadData>), 200)]
        public async Task<IActionResult> RemoveCarrier([FromRoute] Guid loadId, [FromBody] RatingQuestionAnswerData ratingQuestionAnswer)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("Invalid UserId");
                }

                return Success(await _shippingService.RemoveCarrierFromLoad(loadId, _userContext.UserName, ratingQuestionAnswer));

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        [Route("DeleteLoad/{loadId}")]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Delete)]
        [ProducesResponseType(typeof(ResponseMessage<ShippingLoadData>), 200)]
        public async Task<IActionResult> DeleteLoad([FromRoute]Guid loadId)
        {
            return await DeleteDetailLoad(loadId, null);
        }

        [HttpPost]
        [Route("DeleteDetailLoad/{loadId}")]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Delete)]
        [ProducesResponseType(typeof(ResponseMessage<ShippingLoadData>), 200)]
        public async Task<IActionResult> DeleteDetailLoad([FromRoute]Guid loadId, [FromBody] RatingQuestionAnswerData ratingQuestionAnswer)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("Invalid UserId");
                }

                return Success(await _shippingService.DeleteLoad(loadId, _userContext.UserName, ratingQuestionAnswer));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet]
        [Route("AuditLog/{loadId}")]
        [ProducesResponseType(typeof(ResponseMessage<LoadAuditLogData[]>), 200)]
        public IActionResult GetLoadAuditLogs(Guid loadId)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new Exception("Invalid UserId");
                }

                return Success(_shippingService.GetLoadAuditLogs(loadId));
            }
            catch (Exception e)
            {
                return Error<LoadAuditLogData[]>(e);
            }
        }

        [HttpGet]
        [Route("CarrierScac/{loadId}")]
        [ProducesResponseType(typeof(ResponseMessage<LoadCarrierScacData[]>), 200)]
        public IActionResult GetCarrierScacs(Guid loadId)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new Exception("Invalid UserId");
                }

                return Success(_shippingService.GetLoadCarrierScacs(loadId));
            }
            catch (Exception e)
            {
                return Error<LoadCarrierScacData[]>(e);
            }
        }

        [HttpGet]
        [Route("CarrierScacRestriction/{loadId}")]
        [ProducesResponseType(typeof(ResponseMessage<LoadCarrierScacRestrictionData[]>), 200)]
        public IActionResult GetCarrierScacRestrictions(Guid loadId)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new Exception("Invalid UserId");
                }

                return Success(_shippingService.GetLoadCarrierScacRestrictions(loadId));
            }
            catch (Exception e)
            {
                return Error<LoadCarrierScacRestrictionData[]>(e);
            }
        }

        [HttpPost]
        [Route("PostLoads")]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shopit_Load_Post)]
        [ProducesResponseType(typeof(ResponseMessage<PostLoadsClientResponse>), 200)]
        public async Task<IActionResult> PostLoads([FromBody] List<PostingLoad> loads)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new Exception("Invalid UserId");
                }

                var request = new PostLoadsRequest
                {
                    CurrentUsername = _userContext.UserName,
                    Loads = loads
                };

                var response = await _shippingService.PostLoadsAsync(request);
                ValidationProblemDetails problemDetails = null;
                if (!response.IsSuccess)
                {
                    problemDetails = new ValidationProblemDetails(response.ModelState)
                    {
                        Title = "Post Loads Error",
                        Detail = "One or more loads failed to post to the Marketplace.  See loads for error details",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                    };
                }

                return Success(new PostLoadsClientResponse
                {
                    PostedLoads = response.PostedLoads,
                    ValidationProblemDetails = problemDetails
                });
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet]
        [Route("ManuallyCreated/{loadId?}")]
        [ProducesResponseType(typeof(ResponseMessage<OrderEntryViewModel>), 200)]
        public IActionResult GetManuallyCreatedById(Guid? loadId)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("Invalid UserId");
                }

                var load = loadId.HasValue ?
                    _shippingService.GetLoadDetailById(loadId.Value) :
                    _shippingService.GetDefaultLoadDetail(_userContext.UserId.Value);

                var vm = _mapper.Map<OrderEntryViewModel>(load);
                SetLoadStopDateTimes(vm);

                return Success(vm);
            }
            catch (ValidationException exception)
            {
                return Error<OrderEntryViewModel>(exception);
            }
        }

        /// <summary>
        /// Sets Early/Late Date/Time fields on view model
        /// </summary>
        /// <param name="vm"></param>
        private void SetLoadStopDateTimes(OrderEntryViewModel vm)
        {
            if (vm != null && vm.LoadStops != null)
            {
                foreach (var stop in vm.LoadStops)
                {
                    if (stop != null)
                    {
                        if (stop.EarlyDtTm.HasValue)
                        {
                            stop.EarlyDate = stop.EarlyDtTm.Value.Date;
                            stop.EarlyTime = GetTimeValue(stop.EarlyDtTm.Value);
                        }
                        if (stop.LateDtTm.HasValue)
                        {
                            stop.LateDate = stop.LateDtTm.Value.Date;
                            stop.LateTime = GetTimeValue(stop.LateDtTm.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a time value from a date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private string GetTimeValue(DateTime date)
        {
            var time = $"{date.Hour.ToString().PadLeft(2, '0')}{date.Minute.ToString().PadLeft(2, '0')}";
            if (time == "0000")
            {
                time = null;
            }

            return time;
        }

        [HttpGet]
        [Route("locations/{searchTerm}")]
        [ProducesResponseType(typeof(ResponseMessage<List<LocationViewModel>>), 200)]
        public async Task<IActionResult> SearchLocations(string searchTerm)
        {
            try
            {
                var customerId = await _userProfileService.GetPrimaryCustomerId(_userContext.UserId.Value);
                if (!customerId.HasValue)
                {
                    throw new Exception("User does not have a primary customer.");
                }

                var locations = _locationService.SearchLocations(customerId.Value, searchTerm);
                return Success(_mapper.Map<List<LocationViewModel>>(locations));
            }
            catch (ValidationException exception)
            {
                return Error<List<LocationViewModel>>(exception);
            }
        }

        [HttpPost]
        [Route("CreateLoad")]
        [ProducesResponseType(typeof(ResponseMessage<OrderEntryViewModel>), 200)]
        [ProducesResponseType(typeof(Dictionary<string, List<string>>), 400)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Create)]

        public async Task<IActionResult> CreateLoad([FromBody]OrderEntryViewModel orderEntryVM)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new Exception("Invalid UserId");
                }

                // LoadService.CreateLoad expects the Customer.IdentUserId, not the Customer.CustomerId
                var customerIdentUserId = await _userProfileService.GetPrimaryCustomerIdentUserId(_userContext.UserId.Value);
                if (!customerIdentUserId.HasValue)
                {
                    throw new Exception("User does not have a primary customer.");
                }

                // Saving locations requires FK to CustomerID
                var customerId = await _userProfileService.GetPrimaryCustomerId(_userContext.UserId.Value);
                if (!customerId.HasValue)
                {
                    throw new Exception("User does not have a primary customer.");
                }

                /**
                 * The UI allows entering an Order Number, which is effectively the ReferenceLoadDisplay, so in order
                 * to generate a ReferenceLoadId, we need to get the user's primary customer's TOPS Owner ID and then
                 * prepend that to the ReferenceLoadDisplay to get the ReferenceLoadId
                 */
                var customerOwnerId = await _userProfileService.GetPrimaryCustomerOwner(_userContext.UserId.Value);
                orderEntryVM.ReferenceLoadId = CalculateReferenceLoadId(customerOwnerId, orderEntryVM.ReferenceLoadDisplay);

                SetLoadStopDtTms(orderEntryVM);

                var load = _mapper.Map<LoadDetailData>(orderEntryVM);
                load.LoadTransaction = new LoadTransactionData()
                {
                    TransactionType = TransactionTypeData.PendingAdd
                };

                /**
                 * LoadStop.ApptType is a calculated field, so the logic for doing so and setting its value on
                 * all load stops is kept in the LoadService.  We call it here to update the values before calling
                 * the shared CreateLoad code, so the load stops will pass validation
                 */
                load.LoadStops = _loadService.SetApptTypeOnLoadStops(load.LoadStops);

                var options = new CreateLoadOptionsDto()
                {
                    ValidateAddress = OrderAddressValidationEnum.Validate,
                    ManuallyCreated = true
                };

                var createResponse = await _loadService.CreateLoad(load, customerIdentUserId.Value, _userContext.UserName, options);
                if (!createResponse.IsSuccess)
                {
                    var problemDetails = new ValidationProblemDetails(createResponse.ModelState)
                    {
                        Title = "Create Load Error",
                        Detail = "One or more errors occurred when trying to save this load.  See form for error details",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                    };
                    return BadRequest(problemDetails);
                }
                else
                {
                    SaveLocations(customerId.Value, load.LoadStops);
                }

                return Success(orderEntryVM);
            }
            catch (ValidationException exception)
            {
                return Error<List<LocationViewModel>>(exception);
            }
        }

        /// <summary>
        /// Sets Early/Late DtTm fields on view model
        /// </summary>
        /// <param name="vm"></param>
        private void SetLoadStopDtTms(OrderEntryViewModel vm)
        {
            if (vm != null && vm.LoadStops != null)
            {
                foreach (var stop in vm.LoadStops)
                {
                    if (stop != null)
                    {
                        stop.EarlyDtTm = GetDate(stop.EarlyDate, stop.EarlyTime);
                        stop.LateDtTm = GetDate(stop.LateDate, stop.LateTime);
                    }
                }
            }
        }

        /// <summary>
        /// Gets date from date and time string
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private DateTime? GetDate(DateTime? date, string time)
        {
            DateTime? result = null;
            if (date.HasValue)
            {
                result = date.Value.Date;
                if (!string.IsNullOrWhiteSpace(time) && time.Length >= 4)
                {
                    int.TryParse(time.Substring(0, 2), out int hours);
                    int.TryParse(time.Substring(2, 2), out int minutes);

                    result = result.Value.AddHours(hours).AddMinutes(minutes);
                }
                else
                {
                    result = result.Value.AddHours(23).AddMinutes(59);
                }
            }

            return result;
        }

        /// <summary>
        /// Saves customer locations
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="loadStops"></param>
        private void SaveLocations(Guid customerId, List<LoadStopData> loadStops)
        {
            if (loadStops != null)
            {
                var locations = _mapper.Map<List<LocationData>>(loadStops);
                _locationService.AddOrUpdateLocations(customerId, locations, _userContext.UserName);
            }
        }

        [HttpPost]
        [Route("UpdateLoad")]
        [ProducesResponseType(typeof(ResponseMessage<OrderEntryViewModel>), 200)]
        [ProducesResponseType(typeof(Dictionary<string, List<string>>), 400)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Edit)]
        public async Task<IActionResult> UpdateLoad([FromBody]OrderEntryViewModel orderEntryVM)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new Exception("Invalid UserId");
                }

                // LoadService.CreateLoad expects the Customer.IdentUserId, not the Customer.CustomerId
                var customerIdentUserId = await _userProfileService.GetPrimaryCustomerIdentUserId(_userContext.UserId.Value);
                if (!customerIdentUserId.HasValue)
                {
                    throw new Exception("User does not have a primary customer.");
                }

                // Saving locations requires FK to CustomerID
                var customerId = await _userProfileService.GetPrimaryCustomerId(_userContext.UserId.Value);
                if (!customerId.HasValue)
                {
                    throw new Exception("User does not have a primary customer.");
                }

                SetLoadStopDtTms(orderEntryVM);

                var load = _mapper.Map<LoadDetailData>(orderEntryVM);
                load.LoadTransaction = new LoadTransactionData()
                {
                    TransactionType = TransactionTypeData.PendingAdd
                };

                load.LoadStops = _loadService.SetApptTypeOnLoadStops(load.LoadStops);

                var updateOptions = new UpdateLoadOptions()
                {
                    MapObject = true,
                    ValidateAddress = true,
                    ManuallyCreated = true
                };

                var response = await _loadService.UpdateLoad(load, customerIdentUserId.Value, _userContext.UserName, updateOptions);
                if (!response.IsSuccess)
                {
                    var problemDetails = new ValidationProblemDetails(response.ModelState)
                    {
                        Title = "Update Load Error",
                        Detail = "One or more errors occurred when trying to save this load.  See form for error details",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                    };
                    return BadRequest(problemDetails);
                }
                else
                {
                    SaveLocations(customerId.Value, load.LoadStops);
                }

                return Success(orderEntryVM);
            }
            catch (ValidationException exception)
            {
                return Error<List<LocationViewModel>>(exception);
            }
        }

        private static string CalculateReferenceLoadId(string primaryCustomerOwnerId, string referenceLoadDisplay)
        {
            var result = referenceLoadDisplay;
            if (!string.IsNullOrWhiteSpace(primaryCustomerOwnerId) && !string.IsNullOrWhiteSpace(referenceLoadDisplay))
            {
                result = $"{primaryCustomerOwnerId}-{referenceLoadDisplay}";
            }

            return result;
        }
    }
}
