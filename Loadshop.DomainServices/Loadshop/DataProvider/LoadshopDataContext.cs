using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using TMS.Infrastructure.EntityFramework;

namespace Loadshop.DomainServices.Loadshop.DataProvider
{
    public class LoadshopDataContext : DbContextCore
    {
        public static readonly LoggerFactory _loggerFactory = new LoggerFactory(new ILoggerProvider[]
        {
            new DebugLoggerProvider()
        });

        public LoadshopDataContext(DbContextOptions<LoadshopDataContext> options) : base(options)
        {

        }

        public LoadshopDataContext() : base("TestConnectionString") {
        
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RemovePluralizingTableNameConvention();
            modelBuilder.RemoveEntityFromTableNameConvention();
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           // optionsBuilder.UseLoggerFactory(_loggerFactory);
            //optionsBuilder.EnableSensitiveDataLogging(true);
            base.OnConfiguring(optionsBuilder);
        }

        // This is needed for mocking unit tests
        public virtual int SaveChanges(string userId)
        {
            return base.SaveChanges(userId);
        }

        // This is needed for mocking unit tests
        public virtual async Task<int> SaveChangesAsync(string userId, System.Threading.CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(userId, cancellationToken);
        }

        public virtual DbSet<CustomerEntity> Customers { get; set; }
        [NotMapped]
        public virtual DbSet<CustomerApiEntity> CustomerApis { get; set; }
        public virtual DbSet<CustomerContactEntity> CustomerContacts { get; set; }
        public virtual DbSet<EquipmentEntity> Equipment { get; set; }
        public virtual DbSet<LoadCarrierScacEntity> LoadCarrierScacs { get; set; }
        public virtual DbSet<LoadCarrierScacRestrictionEntity> LoadCarrierScacRestrictions { get; set; }
        public virtual DbSet<LoadClaimEntity> LoadClaims { get; set; }
        public virtual DbSet<LoadContactEntity> LoadContacts { get; set; }
        public virtual DbSet<LoadEntity> Loads { get; set; }
        public virtual DbSet<LoadHistoryEntity> LoadHistories { get; set; }
        public virtual DbSet<LoadAuditLogEntity> LoadAuditLogs { get; set; }
        [NotMapped]
        public virtual DbSet<LoadViewEntity> LoadViews { get; set; }
        [NotMapped]
        public virtual DbSet<ShippingLoadViewEntity> ShippingLoadViews { get; set; }
        [NotMapped]
        public virtual DbSet<LoadDetailViewEntity> LoadDetailViews { get; set; }
        public virtual DbSet<LoadStopEntity> LoadStops { get; set; }
        public virtual DbSet<LoadStopContactEntity> LoadStopContacts { get; set; }
        public virtual DbSet<LoadTransactionEntity> LoadTransactions { get; set; }
        public virtual DbSet<LoadLineItemEntity> LoadLineItems { get; set; }
        public virtual DbSet<MessageTypeEntity> MessageTypes { get; set; }
        public virtual DbSet<TransactionTypeEntity> TransactionTypes { get; set; }
        public virtual DbSet<UserEntity> Users { get; set; }
        public virtual DbSet<UserLaneEntity> UserLanes { get; set; }
        public virtual DbSet<UserLaneLoadEntity> UserLaneLoads { get; set; }
        public virtual DbSet<UserLaneEquipmentEntity> UserLaneEquipments { get; set; }
        public virtual DbSet<UserLaneMessageTypeEntity> UserLaneMessageTypes { get; set; }
        public virtual DbSet<UserNotificationEntity> UserNotifications { get; set; }
        public virtual DbSet<NotificationMessageEntity> NotificationMessages { get; set; }
        public virtual DbSet<NotificationDataEntity> NotificationDatas { get; set; }
        public virtual DbSet<NotificationEntity> Notifications { get; set; }
        public virtual DbSet<LoadCarrierGroupEntity> LoadCarrierGroups { get; set; }
        public virtual DbSet<LoadCarrierGroupCarrierEntity> LoadCarrierGroupCarriers { get; set; }
        public virtual DbSet<LoadCarrierGroupEquipmentEntity> LoadCarrierGroupEquipment { get; set; }
        public virtual DbSet<LoadCarrierGroupTypeEntity> LoadCarrierGroupTypes { get; set; }
        public virtual DbSet<CommodityEntity> Commodities { get; set; }
        public virtual DbSet<CarrierEntity> Carriers { get; set; }
        public virtual DbSet<CarrierScacEntity> CarrierScacs { get; set; }
        public virtual DbSet<TransportationModeEntity> TransportationModes { get; set; }
        public virtual DbSet<StopTypeEntity> StopTypes { get; set; }
        public virtual DbSet<AppointmentSchedulerConfirmationTypeEntity> AppointmentSchedulerConfirmationTypes { get; set; }
        public virtual DbSet<UnitOfMeasureEntity> UnitOfMeasures { get; set; }
        public virtual DbSet<CustomerTransactionLogEntity> CustomerTransactionLogs { get; set; }
        public virtual DbSet<ServiceTypeEntity> ServiceTypes { get; set; }
        public virtual DbSet<LoadServiceTypeEntity> LoadServiceTypes { get; set; }
        public virtual DbSet<LocationEntity> Locations { get; set; }
        public virtual DbSet<SpecialInstructionEntity> SpecialInstructions { get; set; }
        public virtual DbSet<SpecialInstructionEquipmentEntity> SpecialInstructionEquipments { get; set; }
        public virtual DbSet<CustomerLoadTypeEntity> CustomerLoadTypes { get; set; }
        public virtual DbSet<UserCommunicationEntity> UserCommunications { get; set; }
        public virtual DbSet<UserCommunicationAcknowledgementEntity> UserCommunicationAcknowledgements { get; set; }
        public virtual DbSet<PostedLoadCarrierGroupEntity> PostedLoadCarrierGroups { get; set; }
        public virtual DbSet<AgreementDocumentEntity> AgreementDocuments { get; set; }
        public virtual DbSet<UserAgreementDocumentEntity> UserAgreements { get; set; }

        //Security Entities
        public virtual DbSet<SecurityAccessRoleEntity> SecurityAccessRoles { get; set; }
        public virtual DbSet<SecurityAppActionEntity> SecurityAppActions { get; set; }

        //Security Relationships
        public virtual DbSet<SecurityUserAccessRoleEntity> SecurityUserAccessRoles { get; set; }
        public virtual DbSet<SecurityAccessRoleAppActionEntity> SecurityAccessRoleAppActions { get; set; }
        public virtual DbSet<SecurityAccessRoleParentEntity> SecurityAccessRoleParents { get; set; }
        public virtual DbSet<UserCarrierScacEntity> UserCarrierScacs { get; set; }
        public virtual DbSet<UserShipperEntity> UserShippers { get; set; }
        public virtual DbSet<CustomerCarrierScacContractEntity> CustomerCarrierScacContracts { get; set; }
        public virtual DbSet<RatingQuestionEntity> RatingQuestions { get; set; }
        public virtual DbSet<RatingQuestionAnswerEntity> RatingQuestionAnswers { get; set; }
        public virtual DbSet<LoadStatusTransactionEntity> LoadStatusTransactions { get; set; }
        public virtual DbSet<SmartSpotPriceQuoteLogEntity> SmartSpotPriceQuoteLogs { get; set; }

        public virtual DbSet<LoadDocumentEntity> LoadDocuments { get; set; }
        public virtual DbSet<LoadCurrentStatusEntity> LoadCurrentStatuses { get; set; }

        public virtual decimal GetDATGuardRate(string originZip, string destZip, string equipmentId, DateTime pickupDate)
        {
            var sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@originZip", originZip));
            sqlParams.Add(new SqlParameter("@destZip", destZip));
            sqlParams.Add(new SqlParameter("@equipmentId", equipmentId));
            sqlParams.Add(new SqlParameter("@pickupDate", pickupDate));

            var rate = Database.SqlQuery<decimal>("exec spGetDATGuardRate @originZip, @destZip, @equipmentId, @pickupDate", sqlParams.ToArray()).FirstOrDefault();
            return rate;
        }

        public virtual async Task<int> CreateLoadAuditLogEntryAsync(Guid loadId, AuditTypeData auditType, Guid userId, string userName, string firstName, string lastName)
        {
            var userIdParam = new SqlParameter("@userId", userId);
            var loadIdParam = new SqlParameter("@loadId", loadId);
            var auditTypeId = new SqlParameter("@auditTypeId", auditType.ToString("G"));
            var topsUsername = new SqlParameter("@topsUsername", userName);
            var topsFirstName = new SqlParameter("@topsFirstName", firstName);
            var topsLastName = new SqlParameter("@topsLastName", lastName);

            var commandText = "exec LoadBoard.dbo.spCreateLoadAuditLogEntry @userId, @loadId, @auditTypeId, @topsUsername, @topsFirstName, @topsLastName";
            return await Database.ExecuteSqlCommandAsync(commandText, userIdParam, loadIdParam, auditTypeId, topsUsername, topsFirstName, topsLastName);
        }

    }
}
