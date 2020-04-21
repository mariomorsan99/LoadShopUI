using System.Collections.Generic;
using System.Linq;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using Microsoft.Extensions.Configuration;
using Loadshop.DomainServices.Exceptions;
using AutoMapper;
using Loadshop.API.Models;
using Loadshop.API.Models.ViewModels;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Enum;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Customer.API.Controllers.v2
{
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class LoadsController : BaseController
    {
        private readonly string _systemUser = "system";

        private readonly IMapper _mapper;
        private readonly ILoadService _loadService;
        private readonly IConfigurationRoot _configuration;
        private readonly IUserContext _user;

        public LoadsController(IMapper mapper, ILoadService loadService, IConfigurationRoot configuration, IUserContext userContext)
        {
            _mapper = mapper;
            _loadService = loadService;
            _configuration = configuration;
            _user = userContext;
        }

        [HttpPost]
        [Route("CreateLoad")]
        [ProducesResponseType(typeof(ResponseMessage<string>), 200)]
        [ProducesResponseType(typeof(ResponseMessage<string>), 400)]
        public async Task<IActionResult> CreateLoad([FromBody]LoadDetailViewModel load)
        {
            try
            {
                var loadData = _mapper.Map<LoadDetailData>(load);
                /**
                 * DO NOT YET consider auto-posting via the v2 customer API
                 * [10:08 AM] Kind, Brian K.
                 *     Now I am questioning the true purpose of Auto Post To Marketplace.  Based on what you just gave me I am thinking the Auto Post to Marketplace was really for  the old way.  
                 * ​[10:08 AM] Vetta, Joe (Consultant)
                 *     (laugh)
                 * ​[10:11 AM] Kind, Brian K.
                 *     How far are we in the rabbit hole are we in fixing the bug?  Can we leave the new way logic alone.  Knowing my scenario #2  is just the way it is for now.
                 * ​[10:11 AM] Vetta, Joe (Consultant)
                 *     Yes, it's a simple thing for me to undo
                 * ​[10:11 AM] Kind, Brian K.
                 *     So original issue was EDI (Old Way), shipper had Auto Post to marketplace set to 'N'.  We fixed it.
                 * ​[10:12 AM] Kind, Brian K.
                 *     I can reach out to Kevin and explain.  So basically new way does not care about Auto Post to marketplace for now.
                 * ​[10:13 AM] Kind, Brian K.
                 *     Old way will
                 */
                loadData.LoadTransaction = new LoadTransactionData()
                {
                    TransactionType = TransactionTypeData.PendingAdd
                };

                var result = _loadService.GenerateReturnURL(new LoadDetailData[] { loadData });
                var options = new CreateLoadOptionsDto()
                {
                    ValidateAddress = OrderAddressValidationEnum.Validate,
                    ManuallyCreated = false
                };
                var createResponse = await _loadService.CreateLoad(loadData, customerId: _user.UserId.Value, username: _systemUser, options);
                if (!createResponse.IsSuccess)
                {
                    return BadRequest(new ResponseMessage<string>(), createResponse.ModelState);

                }

                return Success(result);
            }
            catch (ValidationException e)
            {
                return Error<string>(e);
            }
        }

        [HttpPost]
        [Route("CreateLoads")]
        [ProducesResponseType(typeof(ResponseMessage<string>), 200)]
        [ProducesResponseType(typeof(ResponseMessage<string>), 400)]
        public async Task<IActionResult> CreateLoads([FromBody]LoadDetailViewModel[] loads)
        {
            try
            {
                /**
                 * DO NOT YET consider auto-posting via the v2 customer API
                 * [10:08 AM] Kind, Brian K.
                 *     Now I am questioning the true purpose of Auto Post To Marketplace.  Based on what you just gave me I am thinking the Auto Post to Marketplace was really for  the old way.  
                 * ​[10:08 AM] Vetta, Joe (Consultant)
                 *     (laugh)
                 * ​[10:11 AM] Kind, Brian K.
                 *     How far are we in the rabbit hole are we in fixing the bug?  Can we leave the new way logic alone.  Knowing my scenario #2  is just the way it is for now.
                 * ​[10:11 AM] Vetta, Joe (Consultant)
                 *     Yes, it's a simple thing for me to undo
                 * ​[10:11 AM] Kind, Brian K.
                 *     So original issue was EDI (Old Way), shipper had Auto Post to marketplace set to 'N'.  We fixed it.
                 * ​[10:12 AM] Kind, Brian K.
                 *     I can reach out to Kevin and explain.  So basically new way does not care about Auto Post to marketplace for now.
                 * ​[10:13 AM] Kind, Brian K.
                 *     Old way will
                 */
                var loadDetails = _mapper.Map<LoadDetailData[]>(loads);
                foreach (var load in loadDetails)
                {
                    if (load != null)
                    {
                        load.LoadTransaction = new LoadTransactionData()
                        {
                            TransactionType = TransactionTypeData.PendingAdd
                        };
                    }
                }

                var options = new CreateLoadOptionsDto()
                {
                    ValidateAddress = OrderAddressValidationEnum.Validate,
                    ManuallyCreated = false,
                    AddSmartSpot = false,
                    OverrideLineHaulWithSmartSpot = false,
                    RemoveLineHaulRate = true
                };
                var createResponse = await _loadService.CreateLoadsWithContinueOnFailure(loadDetails, _user.UserId.Value, _systemUser, options);

                var response = new ResponseMessage<string>();

                if (createResponse.Data.Any())
                {
                    var result = _loadService.GenerateReturnURL(createResponse.Data);
                    response.Data = result;
                }

                if (!createResponse.IsSuccess)
                {
                    return BadRequest(response, createResponse.ModelState);
                }

                return Success(response);
            }
            catch (ValidationException e)
            {
                return Error<string>(e);
            }
        }

        [HttpGet]
        [Route("GetLoadById")]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 200)]
        public IActionResult GetLoadById(string id)
        {
            try
            {
                var result = _loadService.GetLoadByCustomerReferenceId(id, customerId: _user.UserId.Value);
                return Success(_mapper.Map<LoadDetailViewModel>(result));
            }
            catch (ValidationException e)
            {
                return Error<List<LoadDetailViewModel>>(e);
            }
        }

        [HttpPost]
        [Route("UpdateLoad")]
        [ProducesResponseType(typeof(ResponseMessage<string>), 200)]
        [ProducesResponseType(typeof(ResponseMessage<string>), 400)]
        public async Task<IActionResult> UpdateLoad([FromBody]LoadDetailViewModel load, [FromQuery]UpdateLoadOptionsViewModel options)
        {
            try
            {
                /**
                 * DO NOT YET consider auto-posting via the v2 customer API
                 * [10:08 AM] Kind, Brian K.
                 *     Now I am questioning the true purpose of Auto Post To Marketplace.  Based on what you just gave me I am thinking the Auto Post to Marketplace was really for  the old way.  
                 * ​[10:08 AM] Vetta, Joe (Consultant)
                 *     (laugh)
                 * ​[10:11 AM] Kind, Brian K.
                 *     How far are we in the rabbit hole are we in fixing the bug?  Can we leave the new way logic alone.  Knowing my scenario #2  is just the way it is for now.
                 * ​[10:11 AM] Vetta, Joe (Consultant)
                 *     Yes, it's a simple thing for me to undo
                 * ​[10:11 AM] Kind, Brian K.
                 *     So original issue was EDI (Old Way), shipper had Auto Post to marketplace set to 'N'.  We fixed it.
                 * ​[10:12 AM] Kind, Brian K.
                 *     I can reach out to Kevin and explain.  So basically new way does not care about Auto Post to marketplace for now.
                 * ​[10:13 AM] Kind, Brian K.
                 *     Old way will
                 */
                var loadData = _mapper.Map<LoadDetailData>(load);
                loadData.LoadTransaction = new LoadTransactionData()
                {
                    TransactionType = TransactionTypeData.PendingUpdate
                };
                var updateOptions = new UpdateLoadOptions()
                {
                    MapObject = true,
                    ValidateAddress = true,
                    ManuallyCreated = false,
                    UpdateSpecialInstructions = false,
                    AppendComments = options?.AppendComments ?? false
                };

                var result = _loadService.GenerateReturnURL(new LoadDetailData[] { loadData });
                var updateResponse = await _loadService.UpdateLoad(loadData, _user.UserId.Value, _systemUser, updateOptions);
                if (!updateResponse.IsSuccess)
                {
                    return BadRequest(new ResponseMessage<string>(), updateResponse.ModelState);
                }

                return Success(result);
            }
            catch (ValidationException e)
            {
                return Error<string>(e);
            }
        }

        [HttpPost]
        [Route("UpdateRates")]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public IActionResult UpdateRates([FromBody]LoadUpdateScacViewModel scacs)
        {
            try
            {
                var scacsData = _mapper.Map<LoadUpdateScacData>(scacs);
                _loadService.UpdateScacs(scacsData, customerId: _user.UserId.Value, username: _systemUser);
                return Success(true);
            }
            catch (ValidationException e)
            {
                return Error<bool>(e);
            }
        }

        [HttpPost]
        [Route("UpdateFuel")]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public IActionResult UpdateFuel([FromBody]LoadUpdateFuelViewModel fuel)
        {
            try
            {
                var fuelData = _mapper.Map<LoadUpdateFuelData>(fuel);
                _loadService.UpdateFuel(fuelData, customerId: _user.UserId.Value, username: _systemUser);
                return Success(true);
            }
            catch (ValidationException e)
            {
                return Error<bool>(e);
            }
        }

        [HttpPost]
        [Route("RemoveLoad")]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 200)]
        [ProducesResponseType(typeof(ResponseMessage<LoadDetailViewModel>), 400)]
        public async Task<IActionResult> RemoveLoad([FromBody]LoadDetailViewModel load)
        {
            try
            {
                var loadData = _mapper.Map<LoadDetailData>(load);
                loadData.LoadTransaction = new LoadTransactionData()
                {
                    TransactionType = TransactionTypeData.Removed
                };

                var removeResponse = await _loadService.UpdateLoad(loadData, _user.UserId.Value, _systemUser, new UpdateLoadOptions());
                if (!removeResponse.IsSuccess)
                {
                    return BadRequest(new ResponseMessage<string>(), removeResponse.ModelState);
                }
                return Success(_mapper.Map<LoadDetailViewModel>(removeResponse.Data));
            }
            catch (ValidationException e)
            {
                return Error<LoadDetailViewModel>(e);
            }
        }
    }
}
