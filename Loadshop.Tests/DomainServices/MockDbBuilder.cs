using Loadshop.DomainServices.Loadshop.DataProvider;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using System;
using MockQueryable.Moq;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System.Linq.Expressions;
using Loadshop.DomainServices.Loadshop.Services.Dto;

namespace Loadshop.Tests.DomainServices
{
    public class MockDbBuilder
    {
        private Mock<LoadshopDataContext> _mockDb = new Mock<LoadshopDataContext>();

        private List<LoadEntity> _loads = new List<LoadEntity>();
        private List<LoadDetailViewEntity> _loadDetailViews = new List<LoadDetailViewEntity>();
        private List<LocationEntity> _locations = new List<LocationEntity>();
        private List<CustomerEntity> _customers = new List<CustomerEntity>();
        private List<CustomerApiEntity> _customerApis = new List<CustomerApiEntity>();
        private List<LoadAuditLogEntity> _loadAuditLogs = new List<LoadAuditLogEntity>();
        private List<LoadCarrierScacEntity> _loadCarrierScacs = new List<LoadCarrierScacEntity>();
        private List<LoadCarrierScacRestrictionEntity> _loadCarrierScacRestrictions = new List<LoadCarrierScacRestrictionEntity>();
        private List<CarrierScacEntity> _carrierScacs = new List<CarrierScacEntity>();
        public Mock<DbSet<CarrierEntity>> MockCarriers { get; private set; } = null;
        private List<CarrierEntity> _carriers = new List<CarrierEntity>();
        private List<CustomerTransactionLogEntity> _customerTransactionLogs = new List<CustomerTransactionLogEntity>();
        private List<MessageTypeEntity> _messageTypes = new List<MessageTypeEntity>();
        private List<LoadServiceTypeEntity> _loadServiceTypes = new List<LoadServiceTypeEntity>();
        private List<ServiceTypeEntity> _serviceTypes = new List<ServiceTypeEntity>();
        private List<LoadClaimEntity> _loadClaims = new List<LoadClaimEntity>();
        private List<RatingQuestionEntity> _ratingQuestions = new List<RatingQuestionEntity>();
        private List<RatingQuestionAnswerEntity> _ratingQuestionAnswers = new List<RatingQuestionAnswerEntity>();
        private List<LoadCarrierGroupEquipmentEntity> _loadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentEntity>();
        private List<LoadStopEntity> _loadStops = new List<LoadStopEntity>();
        private List<LoadLineItemEntity> _loadLineItems = new List<LoadLineItemEntity>();        
        private List<EquipmentEntity> _equipment = new List<EquipmentEntity>();
        private List<PostedLoadCarrierGroupEntity> _postedLoadCarrierGroups = new List<PostedLoadCarrierGroupEntity>();
        private List<AgreementDocumentEntity> _agreementDocuments = new List<AgreementDocumentEntity>();
        private List<UserAgreementDocumentEntity> _userAgreements = new List<UserAgreementDocumentEntity>();
        private List<LoadViewEntity> _bookedLoads = new List<LoadViewEntity>();
        private List<NotificationEntity> _notifications = new List<NotificationEntity>();
        private List<SecurityAccessRoleEntity> _securityAccessRoles = new List<SecurityAccessRoleEntity>();

        public Mock<DbSet<SecurityAccessRoleParentEntity>> MockSecurityAccessRoleParent { get; private set; } = null;
        private List<SecurityAccessRoleParentEntity> _securityAccessRoleParents = new List<SecurityAccessRoleParentEntity>();

        private List<SecurityUserAccessRoleEntity> _securityUserAccessRoles = new List<SecurityUserAccessRoleEntity>();
        private List<SecurityAccessRoleAppActionEntity> _securityAccessRoleAppActions = new List<SecurityAccessRoleAppActionEntity>();
        private List<SecurityAppActionEntity> _securityAppActions = new List<SecurityAppActionEntity>();
        private List<LoadDocumentEntity> _loadDocuments = new List<LoadDocumentEntity>();

        public Mock<DbSet<LoadCarrierGroupCarrierEntity>> MockLoadCarrierGroupCarriers { get; private set; } = null;
        private List<LoadCarrierGroupCarrierEntity> _loadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>();

        public Mock<DbSet<LoadCarrierGroupEntity>> MockLoadCarrierGroups { get; private set; } = null;
        private List<LoadCarrierGroupEntity> _loadCarrierGroups = new List<LoadCarrierGroupEntity>();

        public Mock<DbSet<LoadHistoryEntity>> MockLoadHistories { get; private set; } = null;
        private List<LoadHistoryEntity> _loadHistories = new List<LoadHistoryEntity>();

        public Mock<DbSet<LoadTransactionEntity>> MockLoadTransactions { get; private set; } = null;
        private List<LoadTransactionEntity> _loadTransactions = new List<LoadTransactionEntity>();

        public Mock<DbSet<LoadStatusTransactionEntity>> MockLoadStatusTransactions { get; private set; } = null;
        private List<LoadStatusTransactionEntity> _loadStatusTransactions = new List<LoadStatusTransactionEntity>();

        public Mock<DbSet<SmartSpotPriceQuoteLogEntity>> MockSSPQuoteLogs { get; private set; } = null;
        private List<SmartSpotPriceQuoteLogEntity> _sspQuoteLogs = new List<SmartSpotPriceQuoteLogEntity>();

        public Mock<DbSet<NotificationDataEntity>> MockNotificationData { get; private set; } = null;
        private List<NotificationDataEntity> _notificationData = new List<NotificationDataEntity>();

        public Mock<DbSet<NotificationMessageEntity>> MockNotificationMessages { get; private set; } = null;
        private List<NotificationMessageEntity> _notificationMessages = new List<NotificationMessageEntity>();

        public Mock<DbSet<UserLaneEntity>> MockUserLanes { get; private set; } = null;
        private List<UserLaneEntity> _userLanes = new List<UserLaneEntity>();

        public Mock<DbSet<UserLaneEquipmentEntity>> MockUserLaneEquipments { get; private set; } = null;
        private List<UserLaneEquipmentEntity> _userLaneEquipment = new List<UserLaneEquipmentEntity>();

        public Mock<DbSet<UserLaneMessageTypeEntity>> MockUserLaneMessageTypes { get; private set; } = null;

        public Mock<DbSet<TransportationModeEntity>> MockTransportationModes { get; private set; } = null;
        private List<TransportationModeEntity> _transportationModes = new List<TransportationModeEntity>();

        public Mock<DbSet<StopTypeEntity>> MockStopTypes { get; private set; } = null;
        private List<StopTypeEntity> _stopTypes = new List<StopTypeEntity>();

        public Mock<DbSet<AppointmentSchedulerConfirmationTypeEntity>> MockAppointmentSchedulerConfirmationType { get; private set; } = null;
        private List<AppointmentSchedulerConfirmationTypeEntity> _appointmentSchedulerConfirmationTypes = new List<AppointmentSchedulerConfirmationTypeEntity>();

        public Mock<DbSet<UnitOfMeasureEntity>> MockUnitsOfMeasure { get; private set; } = null;
        private List<UnitOfMeasureEntity> _unitsOfMeasure = new List<UnitOfMeasureEntity>();

        private List<UserLaneMessageTypeEntity> _userLaneMessageTypes = new List<UserLaneMessageTypeEntity>();

        public Mock<DbSet<UserNotificationEntity>> MockUserNotifications { get; private set; } = null;
        private List<UserNotificationEntity> _userNotifications = new List<UserNotificationEntity>();

        public Mock<DbSet<UserShipperEntity>> MockUserShippers { get; private set; } = null;
        private List<UserShipperEntity> _userShippers = new List<UserShipperEntity>();

        public Mock<DbSet<UserCarrierScacEntity>> MockUserCarrierScacs { get; private set; } = null;
        private List<UserCarrierScacEntity> _userCarrierScacs = new List<UserCarrierScacEntity>();

        public Mock<DbSet<UserEntity>> MockUsers { get; private set; } = null;
        private List<UserEntity> _users = new List<UserEntity>();

        public Mock<DbSet<UserCommunicationEntity>> MockUserCommunications { get; private set; } = null;
        private List<UserCommunicationEntity> _userCommunications = new List<UserCommunicationEntity>();

        public Mock<DbSet<UserCommunicationAcknowledgementEntity>> MockUserCommunicationAcknowledgements { get; private set; } = null;
        private List<UserCommunicationAcknowledgementEntity> _userCommunicationAcknowlegedments = new List<UserCommunicationAcknowledgementEntity>();
        public Mock<DbSet<SpecialInstructionEntity>> MockSpecialInstructions { get; private set; } = null;
        private List<SpecialInstructionEntity> _specialInstructions = new List<SpecialInstructionEntity>();

        public Mock<DbSet<SpecialInstructionEquipmentEntity>> MockSpecialInstructionEquipment { get; private set; } = null;
        private List<SpecialInstructionEquipmentEntity> _specialInstructionEquipment = new List<SpecialInstructionEquipmentEntity>();

        public Mock<DbSet<CustomerCarrierScacContractEntity>> MockCustomerCarrierScacContracts { get; private set; } = null;
        private List<CustomerCarrierScacContractEntity> _customerCarrierScacContracts = new List<CustomerCarrierScacContractEntity>();


        public Mock<LoadshopDataContext> Build()
        {
            UpdateMockDbSetup();
            return _mockDb;
        }

        public MockDbBuilder WithLoad(LoadEntity load)
        {
            _loads.Add(load);
            return this;
        }

        public MockDbBuilder WithLoads(params LoadEntity[] loads)
        {
            _loads.AddRange(loads);
            return this;
        }

        public MockDbBuilder WithRatingQuestions(List<RatingQuestionEntity> ratingQuestions)
        {
            _ratingQuestions.AddRange(ratingQuestions);
            return this;
        }


        public MockDbBuilder WithRatingQuestionAnswers(List<RatingQuestionAnswerEntity> ratingQuestionAnswers)
        {
            _ratingQuestionAnswers.AddRange(ratingQuestionAnswers);
            return this;
        }
        public MockDbBuilder WithAgreementDocuments(List<AgreementDocumentEntity> documents)
        {
            _agreementDocuments.AddRange(documents);
            return this;
        }

        public MockDbBuilder WithLoadDocuments(List<LoadDocumentEntity> documents)
        {
            _loadDocuments.AddRange(documents);
            return this;
        }

        public MockDbBuilder WithUserAgreements(List<UserAgreementDocumentEntity> agreements)
        {
            _userAgreements.AddRange(agreements);
            return this;
        }

        public MockDbBuilder WithLoads(List<LoadEntity> loads)
        {
            _loads.AddRange(loads);
            return this;
        }

        public MockDbBuilder WithLoadTransaction(LoadTransactionEntity transaction)
        {
            _loadTransactions.Add(transaction);
            return this;
        }

        public MockDbBuilder WithLoadTransactions(List<LoadTransactionEntity> transactions)
        {
            _loadTransactions.AddRange(transactions);
            return this;
        }

        public MockDbBuilder WithLoadClaims(List<LoadClaimEntity> items)
        {
            _loadClaims.AddRange(items);
            return this;
        }

        public MockDbBuilder WithLoadDetailViews(List<LoadDetailViewEntity> loadDetailViews)
        {
            _loadDetailViews.AddRange(loadDetailViews);
            return this;
        }

        public MockDbBuilder WithLocation(LocationEntity location)
        {
            _locations.Add(location);
            return this;
        }

        public MockDbBuilder WithLocations(List<LocationEntity> locations)
        {
            _locations.AddRange(locations);
            return this;
        }

        public MockDbBuilder WithCustomer(CustomerEntity customer)
        {
            _customers.Add(customer);
            return this;
        }

        public MockDbBuilder WithCustomers(List<CustomerEntity> customers)
        {
            _customers.AddRange(customers);
            return this;
        }

        public MockDbBuilder WithCustomerApi(CustomerApiEntity customerApi)
        {
            _customerApis.Add(customerApi);
            return this;
        }

        public MockDbBuilder WithCustomerApis(List<CustomerApiEntity> customerApis)
        {
            _customerApis.AddRange(customerApis);
            return this;
        }

        public MockDbBuilder WithLoadAuditLogs(List<LoadAuditLogEntity> loadAuditLogs)
        {
            _loadAuditLogs.AddRange(loadAuditLogs);
            return this;
        }

        public MockDbBuilder WithUser(UserEntity user)
        {
            _users.Add(user);
            return this;
        }

        public MockDbBuilder WithUsers(List<UserEntity> users)
        {
            _users.AddRange(users);
            return this;
        }

        public MockDbBuilder WithUserNotification(UserNotificationEntity item)
        {
            _userNotifications.Add(item);
            return this;
        }

        public MockDbBuilder WithUserNotifications(List<UserNotificationEntity> items)
        {
            _userNotifications.AddRange(items);
            return this;
        }

        public MockDbBuilder WithLoadCarrierScac(LoadCarrierScacEntity item)
        {
            _loadCarrierScacs.Add(item);
            return this;
        }

        public MockDbBuilder WithLoadCarrierScacs(List<LoadCarrierScacEntity> items)
        {
            _loadCarrierScacs.AddRange(items);
            return this;
        }

        public MockDbBuilder WithLoadCarrierScacRestriction(LoadCarrierScacRestrictionEntity item)
        {
            _loadCarrierScacRestrictions.Add(item);
            return this;
        }

        public MockDbBuilder WithLoadCarrierScacRestrictions(List<LoadCarrierScacRestrictionEntity> items)
        {
            _loadCarrierScacRestrictions.AddRange(items);
            return this;
        }

        public MockDbBuilder WithMessageTypes(List<MessageTypeEntity> messageTypeEntities)
        {
            _messageTypes.AddRange(messageTypeEntities);
            return this;
        }

        public MockDbBuilder WithCarrierScacs(List<CarrierScacEntity> items)
        {
            _carrierScacs.AddRange(items);
            return this;
        }

        public MockDbBuilder WithCarriers(List<CarrierEntity> items)
        {
            _carriers.AddRange(items);
            return this;
        }

        public MockDbBuilder WithLoadStatusTransactions(List<LoadStatusTransactionEntity> loadStatusTransactions)
        {
            _loadStatusTransactions.AddRange(loadStatusTransactions);
            return this;
        }

        public MockDbBuilder WithLoadCarrierGroupCarriers(List<LoadCarrierGroupCarrierEntity> items)
        {
            _loadCarrierGroupCarriers.AddRange(items);
            return this;
        }

        public MockDbBuilder WithLoadCarrierGroups(List<LoadCarrierGroupEntity> items)
        {
            _loadCarrierGroups.AddRange(items);
            return this;
        }

        public MockDbBuilder WithPostedLoadCarrierGroups(List<PostedLoadCarrierGroupEntity> items)
        {
            _postedLoadCarrierGroups.AddRange(items);
            return this;
        }

        public MockDbBuilder WithServiceTypes(List<ServiceTypeEntity> items)
        {
            _serviceTypes.AddRange(items);
            return this;
        }

        public MockDbBuilder WithLoadServiceTypes(List<LoadServiceTypeEntity> items)
        {
            _loadServiceTypes.AddRange(items);
            return this;
        }

        public MockDbBuilder WithBookedLoads(List<LoadViewEntity> items)
        {
            _bookedLoads.AddRange(items);
            return this;
        }

        public MockDbBuilder WithLoadStops(List<LoadStopEntity> items)
        {
            _loadStops.AddRange(items);
            return this;
        }

        public MockDbBuilder WithLoadLineItems(List<LoadLineItemEntity> items)
        {
            _loadLineItems.AddRange(items);
            return this;
        }
        public MockDbBuilder WithEquipement(List<EquipmentEntity> items)
        {
            _equipment.AddRange(items);
            return this;
        }

        public MockDbBuilder WithUserLanes(List<UserLaneEntity> items)
        {
            _userLanes.AddRange(items);
            return this;
        }

        public MockDbBuilder WithNotificationData(List<NotificationDataEntity> items)
        {
            _notificationData.AddRange(items);
            return this;
        }

        public MockDbBuilder WithNotifications(List<NotificationEntity> items)
        {
            _notifications.AddRange(items);
            return this;
        }

        public MockDbBuilder WithUserLaneEquipment(List<UserLaneEquipmentEntity> items)
        {
            _userLaneEquipment.AddRange(items);
            return this;
        }

        public MockDbBuilder WithUserLaneMessageTypes(List<UserLaneMessageTypeEntity> items)
        {
            _userLaneMessageTypes.AddRange(items);
            return this;
        }

        public MockDbBuilder WithSecurityAccessRoles(List<SecurityAccessRoleEntity> items)
        {
            _securityAccessRoles.AddRange(items);
            return this;
        }

        public MockDbBuilder WithSecurityUserAccessRoles(List<SecurityUserAccessRoleEntity> items)
        {
            _securityUserAccessRoles.AddRange(items);
            return this;
        }

        public MockDbBuilder WithSecurityAccessRoleParents(List<SecurityAccessRoleParentEntity> items)
        {
            _securityAccessRoleParents.AddRange(items);
            return this;
        }

        public MockDbBuilder WithSecurityAccessRoleAppActions(List<SecurityAccessRoleAppActionEntity> items)
        {
            _securityAccessRoleAppActions.AddRange(items);
            return this;
        }

        public MockDbBuilder WithSecurityAppActions(List<SecurityAppActionEntity> items)
        {
            _securityAppActions.AddRange(items);
            return this;
        }

        public MockDbBuilder WithUserCarrierScacs(List<UserCarrierScacEntity> items)
        {
            _userCarrierScacs.AddRange(items);
            return this;
        }

        public MockDbBuilder WithUserShippers(List<UserShipperEntity> items)
        {
            _userShippers.AddRange(items);
            return this;
        }

        public MockDbBuilder WithSpecialInstructions(List<SpecialInstructionEntity> items)
        {
            _specialInstructions.AddRange(items);
            return this;
        }

        public MockDbBuilder WithSpecialInstructionEquipment(List<SpecialInstructionEquipmentEntity> items)
        {
            _specialInstructionEquipment.AddRange(items);
            return this;
        }
 
        public MockDbBuilder WithTransportationModes(List<TransportationModeEntity> items)
        {
            _transportationModes.AddRange(items);
            return this;
        }

        public MockDbBuilder WithStopTypes(List<StopTypeEntity> items)
        {
            _stopTypes.AddRange(items);
            return this;
        }

        public MockDbBuilder WithAppointmentSchedulerConfirmationTypes(List<AppointmentSchedulerConfirmationTypeEntity> items)
        {
            _appointmentSchedulerConfirmationTypes.AddRange(items);
            return this;
        }

        public MockDbBuilder WithUnitsOfMeasure(List<UnitOfMeasureEntity> items)
        {
            _unitsOfMeasure.AddRange(items);
            return this;
        }

        public MockDbBuilder WithUserCommunications(List<UserCommunicationEntity> items)
        {
            _userCommunications.AddRange(items);
            return this;
        }

        public MockDbBuilder WithUserCommunicationAcknowledgements(List<UserCommunicationAcknowledgementEntity> items)
        {
            _userCommunicationAcknowlegedments.AddRange(items);
            return this;
        }

        public MockDbBuilder WithCustomerCarrierScacContracts(List<CustomerCarrierScacContractEntity> items)
        {
            _customerCarrierScacContracts.AddRange(items);
            return this;
        }

        private void UpdateMockDbSetup()
        {
            _mockDb.Setup(x => x.Loads).Returns(_loads.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.LoadDetailViews).Returns(_loadDetailViews.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.Locations).Returns(_locations.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.Customers).Returns(_customers.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.CustomerApis).Returns(_customerApis.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.LoadAuditLogs).Returns(_loadAuditLogs.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.LoadCarrierScacs).Returns(_loadCarrierScacs.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.LoadCarrierScacRestrictions).Returns(_loadCarrierScacRestrictions.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.CarrierScacs).Returns(_carrierScacs.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.CustomerTransactionLogs).Returns(_customerTransactionLogs.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.MessageTypes).Returns(_messageTypes.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.LoadServiceTypes).Returns(_loadServiceTypes.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.ServiceTypes).Returns(_serviceTypes.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.LoadClaims).Returns(_loadClaims.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.RatingQuestionAnswers).Returns(_ratingQuestionAnswers.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.RatingQuestions).Returns(_ratingQuestions.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.UserAgreements).Returns(_userAgreements.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.AgreementDocuments).Returns(_agreementDocuments.AsQueryable().BuildMockDbSet().Object);
            MockCarriers = SetupDbSet(_carriers, x => x.Carriers);
            _mockDb.Setup(x => x.LoadCarrierGroupEquipment).Returns(_loadCarrierGroupEquipment.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.LoadStops).Returns(_loadStops.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.Equipment).Returns(_equipment.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.PostedLoadCarrierGroups).Returns(_postedLoadCarrierGroups.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.Notifications).Returns(_notifications.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.LoadDocuments).Returns(_loadDocuments.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.SecurityAccessRoles).Returns(_securityAccessRoles.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.SecurityUserAccessRoles).Returns(_securityUserAccessRoles.AsQueryable().BuildMockDbSet().Object);
            MockSecurityAccessRoleParent = SetupDbSet(_securityAccessRoleParents, x => x.SecurityAccessRoleParents);
            _mockDb.Setup(x => x.SecurityAccessRoleAppActions).Returns(_securityAccessRoleAppActions.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.SecurityAppActions).Returns(_securityAppActions.AsQueryable().BuildMockDbSet().Object);
            _mockDb.Setup(x => x.LoadLineItems).Returns(_loadLineItems.AsQueryable().BuildMockDbSet().Object);


            MockLoadCarrierGroupCarriers = _loadCarrierGroupCarriers.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.LoadCarrierGroupCarriers).Returns(MockLoadCarrierGroupCarriers.Object);

            MockLoadCarrierGroups = _loadCarrierGroups.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.LoadCarrierGroups).Returns(MockLoadCarrierGroups.Object);

            MockLoadHistories = _loadHistories.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.LoadHistories).Returns(MockLoadHistories.Object);

            MockLoadTransactions = _loadTransactions.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.LoadTransactions).Returns(MockLoadTransactions.Object);

            MockLoadStatusTransactions = _loadStatusTransactions.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.LoadStatusTransactions).Returns(MockLoadStatusTransactions.Object);

            MockSSPQuoteLogs = _sspQuoteLogs.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.SmartSpotPriceQuoteLogs).Returns(MockSSPQuoteLogs.Object);

            MockNotificationData = _notificationData.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.NotificationDatas).Returns(MockNotificationData.Object);

            MockNotificationMessages = _notificationMessages.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.NotificationMessages).Returns(MockNotificationMessages.Object);

            MockUserLanes = _userLanes.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.UserLanes).Returns(MockUserLanes.Object);

            MockUserLaneEquipments = _userLaneEquipment.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.UserLaneEquipments).Returns(MockUserLaneEquipments.Object);

            MockUserLaneMessageTypes = _userLaneMessageTypes.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.UserLaneMessageTypes).Returns(MockUserLaneMessageTypes.Object);

            MockUserNotifications = _userNotifications.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.UserNotifications).Returns(MockUserNotifications.Object);

            MockUserShippers = _userShippers.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.UserShippers).Returns(MockUserShippers.Object);

            MockUserCarrierScacs = _userCarrierScacs.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.UserCarrierScacs).Returns(MockUserCarrierScacs.Object);

            MockUsers = SetupDbSet(_users, x => x.Users);
            MockTransportationModes = _transportationModes.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.TransportationModes).Returns(MockTransportationModes.Object);

            MockStopTypes = _stopTypes.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.StopTypes).Returns(MockStopTypes.Object);

            MockAppointmentSchedulerConfirmationType = _appointmentSchedulerConfirmationTypes.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.AppointmentSchedulerConfirmationTypes).Returns(MockAppointmentSchedulerConfirmationType.Object);

            MockUnitsOfMeasure = _unitsOfMeasure.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.UnitOfMeasures).Returns(MockUnitsOfMeasure.Object);

            MockUserCommunications = SetupDbSet(_userCommunications, x => x.UserCommunications);
            MockUserCommunicationAcknowledgements = SetupDbSet(_userCommunicationAcknowlegedments, x => x.UserCommunicationAcknowledgements);

            MockSpecialInstructions = _specialInstructions.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.SpecialInstructions).Returns(MockSpecialInstructions.Object);

            MockSpecialInstructionEquipment = _specialInstructionEquipment.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(x => x.SpecialInstructionEquipments).Returns(MockSpecialInstructionEquipment.Object);

            MockCustomerCarrierScacContracts = SetupDbSet(_customerCarrierScacContracts, x => x.CustomerCarrierScacContracts);

            // Default the mock db to return a 1 on save changes calls
            // If tests require different numbers of affected rows, then they
            // can manually override the setup
            _mockDb.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            //_mockDb.Setup(x => x.GetLoadDetailViews(It.IsAny<GetLoadDetailOptions>()))
            //    .Returns(
            //        _loadDetailViews
            //    );

            //_mockDb.Setup(x => x.GetBookedLoadsByUserId(It.IsAny<Guid>(),
            //    It.IsAny<DateTime?>(),
            //    It.IsAny<bool>(),
            //    It.IsAny<bool>()))
            //.Returns(_bookedLoads);

            //_mockDb.Setup(x => x.GetLoadDetailViewUnprocessedAsync())
            //    .ReturnsAsync(_loadDetailViews);

            _mockDb.Setup(x => x.CreateLoadAuditLogEntryAsync(
                It.IsAny<Guid>(),
                It.IsAny<AuditTypeData>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ))
                .ReturnsAsync(1);
        }

        private Mock<DbSet<T>> SetupDbSet<T>(List<T> items, Expression<Func<LoadshopDataContext, DbSet<T>>> dbSetSelector)
            where T : class
        {
            var mockDbSet = items.AsQueryable().BuildMockDbSet();
            _mockDb.Setup(dbSetSelector).Returns(mockDbSet.Object);
            _mockDb.Setup(x => x.Set<T>()).Returns(mockDbSet.Object);
            return mockDbSet;
        }
    }
}
