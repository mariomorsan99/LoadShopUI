using AutoMapper;
using Loadshop.DomainServices.Common.Services.Crud;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class CarrierAdminService : CrudService<CarrierEntity, CarrierProfileData, LoadshopDataContext>, ICarrierAdminService
    {
        private readonly ISecurityService _securityService;

        public CarrierAdminService(LoadshopDataContext context, IMapper mapper, ILogger<CarrierAdminService> logger, IUserContext userContext, ISecurityService securityService)
            : base(context, mapper, logger, userContext)
        {
            _securityService = securityService;
        }

        protected override async Task<IQueryable<CarrierEntity>> InterceptCollectionQuery(IQueryable<CarrierEntity> query)
        {
            return await Task.FromResult(query.Include(carrier => carrier.CarrierScacs));
        }

        protected override async Task<CarrierEntity> GetByKeyQuery(params object[] keys)
        {
            var carrierId = keys.FirstOrDefault() as string;

            carrierId.NullArgumentCheck(nameof(carrierId));

            return await LoadCarrier(carrierId);
        }

        protected override async Task<CarrierEntity> UpdateQuery(CarrierProfileData inData, params object[] keys)
        {
            var carrierId = keys.FirstOrDefault() as string;

            carrierId.NullArgumentCheck(nameof(carrierId));
            return await LoadCarrier(carrierId);
        }

        private async Task<CarrierEntity> LoadCarrier(string carrierId)
        {
            return await Context.Carriers
                                        .Include(carrier => carrier.CarrierScacs)
                                        .SingleOrDefaultAsync(carrier => carrier.CarrierId.ToLower() == carrierId.ToLower());
        }

        protected override async Task UpdateLogic(CarrierProfileData data, CarrierEntity entity, CrudResult<CarrierProfileData> result)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Carrier_Add_Edit);

            entity.CarrierScacs.MapList(data.Scacs, carrierEntity => carrierEntity.Scac, carrierData => carrierData.Scac, Mapper, true);

            //Validate Carrier if ModelState Errors added CrudService Update will automatically set status to Invalid
            ValidateCarrier(entity, result);

            await Task.CompletedTask;
        }

        private void ValidateCarrier(CarrierEntity carrier, CrudResult result)
        {
            if (!carrier.CarrierSuccessSpecialistId.HasValue)
            {
                result.ModelState.AddModelError($"urn:root:{nameof(CarrierEntity.CarrierSuccessSpecialistId)}", "Carrier Success Specialist is required");
            }

            if (!carrier.CarrierSuccessTeamLeadId.HasValue)
            {
                result.ModelState.AddModelError($"urn:root:{nameof(CarrierEntity.CarrierSuccessTeamLeadId)}", "Carrier Team Lead is required");
            }

        }
    }
}
