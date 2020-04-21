using System;
using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Exceptions;
using AutoMapper;
using Loadshop.API.Models;
using Loadshop.API.Models.ViewModels;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Customer.API.Controllers.v1
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class LoadsController : BaseController
    {
        private readonly string _systemUser = "system";

        private readonly IMapper _mapper;
        private readonly ILoadService _loadService;
        private readonly INotificationService _notificationService;
        private readonly IUserContext _user;

        public LoadsController(IMapper mapper, ILoadService loadService, INotificationService notificationService, IUserContext userContext)
        {
            _mapper = mapper;
            _loadService = loadService;
            _notificationService = notificationService;
            _user = userContext;
        }

        [HttpGet]
        [Route("GetAllOpenLoads")]
        [ProducesResponseType(typeof(ResponseMessage<List<LoadDetailViewModel>>), 200)]
        public IActionResult GetAllOpenLoads()
        {
            try
            {
                var loads = _loadService.GetAllOpenLoadsByCustomerId(customerId: _user.UserId.Value);
                return Success(_mapper.Map<List<LoadDetailViewModel>>(loads));
            }
            catch (ValidationException e)
            {
                return Error<List<LoadDetailViewModel>>(e);
            }
        }

        [HttpGet]
        [Route("GetAllPendingAcceptedLoads")]
        [ProducesResponseType(typeof(ResponseMessage<List<LoadDetailViewModel>>), 200)]
        public IActionResult GetAllPendingAcceptedLoads()
        {
            try
            {
                var loads = _loadService.GetAllPendingAcceptedLoads(customerId: _user.UserId.Value);
                return Success(_mapper.Map<List<LoadDetailViewModel>>(loads));
            }
            catch (ValidationException e)
            {
                return Error<List<LoadDetailViewModel>>(e);
            }
        }

        [HttpGet]
        [Route("GetLoadById")]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 200)]
        public IActionResult GetLoadById(string id)
        {
            try
            {
                var load = _loadService.GetLoadByCustomerReferenceId(id, customerId: _user.UserId.Value);
                return Success(_mapper.Map<LoadDetailViewModel>(load));
            }
            catch (ValidationException e)
            {
                return Error<List<LoadDetailViewModel>>(e);
            }
        }

        [HttpGet]
        [Route("HasPendingLoadshopClaim")]
        [ProducesResponseType(typeof(ResponseMessage<List<LoadDetailViewModel>>), 200)]
        public IActionResult HasPendingLoadshopClaim(string id)
        {
            try
            {
                var loads = _loadService.HasPendingLoadshopClaim(id);
                return Success(_mapper.Map<List<LoadDetailViewModel>>(loads));
            }
            catch (ValidationException e)
            {
                return Error<List<LoadDetailViewModel>>(e);
            }
        }

        /// <summary>
        /// API to create a load
        /// </summary>
        /// <param name="useSmartSpotPricing">Flag to indicate whether to use Smart Spot Pricing and override line haul</param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateLoad")]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 200)]
        public async Task<IActionResult> CreateLoad([FromBody]LoadDetailViewModel load, [FromQuery]bool useSmartSpotPricing = false)
        {
            try
            {
                var loadData = _mapper.Map<LoadDetailData>(load);
                var custIdentUserId = _user.UserId.Value;
                var autoPost = load.AutoPostLoad ?? _loadService.GetAutoPostToLoadshop(custIdentUserId);

                if (autoPost)
                {
                    loadData.LoadTransaction = new LoadTransactionData
                    {
                        TransactionType = TransactionTypeData.New
                    };

                    /**
                     * Even when the shipper profile says the load should AutoPost, we do not
                     * allow that if the load does not have a line haul rate and the TOPS
                     * LoadshopShipperMapping is set to not use SmartSpot pricing.
                     * Also, do not allow the load to auto post if the load uses a Fuel Rate 
                     * set by the customer and Fuel Rate is 0.
                     */
                    if ((loadData.LineHaulRate == 0m && !useSmartSpotPricing) 
                        || (load.UseCustomerFuel && load.FuelRate == 0m))
                    {
                        loadData.LoadTransaction = new LoadTransactionData
                        {
                            TransactionType = TransactionTypeData.PendingAdd
                        };
                    }
                }
                else
                {
                    loadData.LoadTransaction = new LoadTransactionData
                    {
                        TransactionType = TransactionTypeData.PendingAdd
                    };
                }

                var options = new CreateLoadOptionsDto()
                {
                    ManuallyCreated = false,
                    AddSpecialInstructions = true,
                    AddSmartSpot = useSmartSpotPricing,
                    OverrideLineHaulWithSmartSpot = useSmartSpotPricing
                };

                var response = await _loadService.CreateLoad(loadData, customerId: _user.UserId.Value, username: _systemUser, options);
                if (response.IsSuccess)
                    return Success(_mapper.Map<LoadDetailViewModel>(response.Data));

                throw new ValidationException(response.GetAllMessages());
            }
            catch (ValidationException e)
            {
                return Error<LoadDetailViewModel>(e);
            }
        }

        /// <summary>
        /// API to update the load
        /// </summary>
        /// <param name="load"></param>
        /// <param name="ignoreFuel">Flag to indicate whether to ignore fuel cost and use previous value, this is used if the source system didn't calc / pass value</param>
        /// <param name="ignoreLineHaulRate">Flag to indicate whether to ignore line haul rate and use previous value, this is used if the source system didn't calc /pass value</param>
        /// <param name="useSmartSpotPricing">Flag to indicate whether to use Smart Spot Pricing and override line haul</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateLoad")]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 200)]
        public async Task<IActionResult> UpdateLoad([FromBody]LoadDetailViewModel load , [FromQuery]bool ignoreFuel = false,
                    [FromQuery]bool ignoreLineHaulRate = false, [FromQuery]bool useSmartSpotPricing = false)
        {
            try
            {
                var loadData = _mapper.Map<LoadDetailData>(load);

                var custIdentUserId = _user.UserId.Value;
                var autoPost = load.AutoPostLoad ?? _loadService.GetAutoPostToLoadshop(custIdentUserId);
                /**
                 * Normal V1 behavior is to set the load to Updated, which means the load shows up on the Marketplace
                 * immedaitely.  For some reason TRS_Loadshop_LoadshopIntegrationService still calls this v1 end-point
                 * when updating loads, even though we don't normally want loads to go directly to the marketplace,
                 * now that shippers have a Post tab to work with directly in Loadshop.
                 * 
                 * That means, we need to check the Shipper Profile before we update the load with an Updated transaction.
                 * If the shipper's profile says to not auto post the load, then we should update the load with a
                 * Pending Update transaction, which keeps the load on the Shipper's Post tab, and does not show it on the
                 * Marketplace tab.
                 * 
                 * The V2 UpdateLoad end-point automatically sets loads to Pending Update, so I'm not entirely sure why
                 * we're still calling V1, so we may have to revist this and/or retire this end-point at some future date.
                 */
                if (autoPost)
                {
                    loadData.LoadTransaction = new LoadTransactionData
                    {
                        TransactionType = TransactionTypeData.Updated
                    };

                    /**
                     * Even when the shipper profile says the load should AutoPost, we do not
                     * allow that if the load does not have a line haul rate and the TOPS
                     * LoadshopShipperMapping is set to not use SmartSpot pricing.
                     */
                    if (loadData.LineHaulRate == 0m && !useSmartSpotPricing)
                    {
                        loadData.LoadTransaction = new LoadTransactionData
                        {
                            TransactionType = TransactionTypeData.PendingUpdate
                        };
                    }
                }
                else
                {
                    loadData.LoadTransaction = new LoadTransactionData
                    {
                        TransactionType = TransactionTypeData.PendingUpdate
                    };
                }

                var updateOptions = new UpdateLoadOptions()
                {
                    MapObject = true,
                    ManuallyCreated = false,
                    UpdateSpecialInstructions = false,
                    AppendComments = false,
                    IgnoreLineHaulRate = ignoreLineHaulRate,
                    IgnoreFuel = ignoreFuel,
                    AddSmartSpot = useSmartSpotPricing,
                    OverrideLineHaulWithSmartSpot = useSmartSpotPricing
                };

                var result = await _loadService.UpdateLoad(loadData, _user.UserId.Value, _systemUser, updateOptions);
                if (result.IsSuccess)
                    return Success(_mapper.Map<LoadDetailViewModel>(result.Data));

                throw new ValidationException(result.GetAllMessages());
            }
            catch (ValidationException e)
            {
                return Error<LoadDetailViewModel>(e);
            }
        }

        [HttpPost]
        [Route("AcceptLoad")]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 200)]
        public IActionResult AcceptLoad([FromBody]LoadDetailViewModel load)
        {
            try
            {
                var loadData = _mapper.Map<LoadDetailData>(load);
                loadData.LoadTransaction = new LoadTransactionData()
                {
                    TransactionType = TransactionTypeData.Accepted
                };

                var result = _loadService.AcceptLoad(loadData, customerId: _user.UserId.Value, username: _systemUser);
                return Success(_mapper.Map<LoadDetailViewModel>(result));
            }
            catch (ValidationException e)
            {
                return Error<LoadDetailViewModel>(e);
            }
        }

        [HttpPost]
        [Route("RemoveLoad")]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 200)]
        public async Task<IActionResult> RemoveLoad([FromBody]LoadDetailViewModel load)
        {
            try
            {
                var loadData = _mapper.Map<LoadDetailData>(load);
                loadData.LoadTransaction = new LoadTransactionData()
                {
                    TransactionType = TransactionTypeData.Removed
                };

                var result = await _loadService.UpdateLoad(loadData, customerId: _user.UserId.Value, username: _systemUser, new UpdateLoadOptions());
                if (result.IsSuccess)
                    return Success(_mapper.Map<LoadDetailViewModel>(result.Data));

                throw new ValidationException(result.GetAllMessages());
            }
            catch (ValidationException e)
            {
                return Error<LoadDetailViewModel>(e);
            }
        }

        [HttpPost]
        [Route("DeleteLoad")]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 200)]
        public IActionResult DeleteLoad([FromBody]LoadDetailViewModel load)
        {
            try
            {
                var loadData = _mapper.Map<LoadDetailData>(load);
                var result = _loadService.DeleteLoad(loadData, customerId: _user.UserId.Value, username: _systemUser);
                return Success(_mapper.Map<LoadDetailViewModel>(result));
            }
            catch (ValidationException e)
            {
                return Error<LoadDetailViewModel>(e);
            }
        }

        [HttpPost]
        [Route("CreateNotificationDetails")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public async Task<IActionResult> CreateNotificationDetails()
        {
            try
            {
                await _notificationService.CreateNotificationDetailsAsync();
                return Success(true);
            }
            catch (Exception e)
            {
                return Error<bool>(e + e.StackTrace);
            }
        }
        [HttpPost]
        [Route("CreateNotifications")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public async Task<IActionResult> CreateNotifications()
        {
            try
            {
               await _notificationService.CreateNotificationsAsync();
                return Success(true);
            }
            catch (Exception e)
            {
                return Error<bool>(e);
            }
        }

        [HttpPost]
        [Route("PreviewUpdateLoad")]
        [ProducesResponseType(typeof(ResponseMessage<CheckUpdateLoadStatusViewModel>), 200)]
        public async Task<IActionResult> PreviewUpdateLoad([FromBody]LoadDetailViewModel load)
        {
            try
            {
                var loadData = _mapper.Map<LoadDetailData>(load);
                loadData.LoadTransaction = new LoadTransactionData()
                {
                    TransactionType = TransactionTypeData.Updated
                };
                var updateOptions = new UpdateLoadOptions()
                {
                    MapObject = true,
                    ManuallyCreated = false,
                    UpdateSpecialInstructions = false,
                    AppendComments = true, // overlay any comments
                    SaveChanges = false,// ensure we don't apply the updates
                    IgnoreFuel = true,
                    IgnoreLineHaulRate = true,
                    AddSmartSpot = false
                };

                var result = await _loadService.PreviewUpdateLoad(loadData, _user.UserId.Value, _systemUser, updateOptions);
                if (string.IsNullOrEmpty(result.ErrorMessage))
                    return Success(new CheckUpdateLoadStatusViewModel()
                    {
                        CanLoadRemainInLoadshop = result.LoadKeptInMarketplace
                    });

                throw new ValidationException(result.ErrorMessage);
            }
            catch (ValidationException e)
            {
                return Error<LoadDetailViewModel>(e);
            }
        }
    }
}
