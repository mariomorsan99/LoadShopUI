using FluentAssertions;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Validation.Services;
using Loadshop.Tests.DomainServices;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Loadshop.DomainServices.Loadshop.Services.Enum;
using Loadshop.DomainServices.Validation.Data.Address;
using Xunit;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;

namespace LoadBoard.Testing.DomainServices.Validation
{
    public class LoadValidationServiceUnitTest : IClassFixture<TestFixture>
    {
        private Mock<LoadshopDataContext> _db;
        private readonly Mock<IAddressValidationService> _mockAddressValidationService;
        private readonly LoadValidationService _service;

        private static Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
        private static Guid CUSTOMER_IDENT_ID = new Guid("22222222-2222-2222-2222-222222222222");

        private static CustomerEntity CUSTOMER => new CustomerEntity
        {
            CustomerId = CUSTOMER_ID,
            IdentUserId = CUSTOMER_IDENT_ID,
            ValidateAddresses = true
        };

        private static List<TransportationModeEntity> TRANSPORTATION_MODES => new List<TransportationModeEntity>
        {
            new TransportationModeEntity { TransportationModeId = 1, Name = "TRUCK" },
            new TransportationModeEntity { TransportationModeId = 2, Name ="INTERMODAL" }
        };

        private static List<StopTypeEntity> STOP_TYPES => new List<StopTypeEntity>
        {
            new StopTypeEntity { StopTypeId = 1, Name = "Pickup" },
            new StopTypeEntity { StopTypeId = 2, Name = "Delivery" }
        };

        private static List<AppointmentSchedulerConfirmationTypeEntity> APPOINTMENT_SCHEDULER_CONFIRMATION_TYPES =>
            new List<AppointmentSchedulerConfirmationTypeEntity>
            {
                new AppointmentSchedulerConfirmationTypeEntity { AppointmentSchedulerConfirmationTypeId = 1, AppointmentSchedulingCode = "CAR", AppointmentConfirmationCode = "CONF", Description = "Carrier to Schedule Appointment. Confirmation Required." },
                new AppointmentSchedulerConfirmationTypeEntity { AppointmentSchedulerConfirmationTypeId = 2, AppointmentSchedulingCode = "CSR", AppointmentConfirmationCode = "CONF", Description = "CSR to Schedule Appointment. Confirmation Required." },
                new AppointmentSchedulerConfirmationTypeEntity { AppointmentSchedulerConfirmationTypeId = 3, AppointmentSchedulingCode = "CSR", AppointmentConfirmationCode = "DNTC", Description = "CSR to Schedule Appointment. No Confirmation Required." },
                new AppointmentSchedulerConfirmationTypeEntity { AppointmentSchedulerConfirmationTypeId = 4, AppointmentSchedulingCode = "GPT", AppointmentConfirmationCode = "CONF", Description = "KBX Planner to Schedule Appointment. Confirmation Required." },
                new AppointmentSchedulerConfirmationTypeEntity { AppointmentSchedulerConfirmationTypeId = 5, AppointmentSchedulingCode = "GPT", AppointmentConfirmationCode = "DNTC", Description = "KBX Planner to Schedule Appointment. No Confirmation Required." },
                new AppointmentSchedulerConfirmationTypeEntity { AppointmentSchedulerConfirmationTypeId = 6, AppointmentSchedulingCode = "NAN", AppointmentConfirmationCode = "CONF", Description = "No Appointment Necessary. Confirmation Required." },
                new AppointmentSchedulerConfirmationTypeEntity { AppointmentSchedulerConfirmationTypeId = 7, AppointmentSchedulingCode = "NAN", AppointmentConfirmationCode = "FCFS", Description = "No Appointment Necessary. First Come, First Serve." },
                new AppointmentSchedulerConfirmationTypeEntity { AppointmentSchedulerConfirmationTypeId = 8, AppointmentSchedulingCode = "SS", AppointmentConfirmationCode = "CONF", Description = "Source System to Schedule Appointment. Confirmation Required." },
                new AppointmentSchedulerConfirmationTypeEntity { AppointmentSchedulerConfirmationTypeId = 9, AppointmentSchedulingCode = "SS", AppointmentConfirmationCode = "DNTC", Description = "Source System to Schedule Appointment. No Confirmation Required." }
            };

        private static List<EquipmentEntity> EQUIPMENT => new List<EquipmentEntity>
        {
            new EquipmentEntity { EquipmentId = "TRUCK ID", EquipmentDesc = "TRUCK" },
            new EquipmentEntity { EquipmentId = "BOAT ID", EquipmentDesc = "BOAT" },
            new EquipmentEntity { EquipmentId = "PLANE ID", EquipmentDesc = "PLANE" },
        };

        private static List<UnitOfMeasureEntity> UNITS_OF_MEASURE => new List<UnitOfMeasureEntity>
        {
            new UnitOfMeasureEntity { UnitOfMeasureId = 1, Name = "Piece" },
            new UnitOfMeasureEntity { UnitOfMeasureId = 2, Name = "Pallets" },
            new UnitOfMeasureEntity { UnitOfMeasureId = 3, Name = "Slip Sheet" },
            new UnitOfMeasureEntity { UnitOfMeasureId = 4, Name = "Skid" }
        };

        private static List<ServiceTypeEntity> SERVICE_TYPES => new List<ServiceTypeEntity>
        {
                new ServiceTypeEntity { ServiceTypeId = 1, Name = "Hazmat" },
                new ServiceTypeEntity { ServiceTypeId = 2, Name = "Team" }
        };

        public LoadValidationServiceUnitTest(TestFixture testFixture)
        {
            _db = new MockDbBuilder()
                .WithCustomer(CUSTOMER)
                .WithTransportationModes(TRANSPORTATION_MODES)
                .WithStopTypes(STOP_TYPES)
                .WithAppointmentSchedulerConfirmationTypes(APPOINTMENT_SCHEDULER_CONFIRMATION_TYPES)
                .WithEquipement(EQUIPMENT)
                .WithUnitsOfMeasure(UNITS_OF_MEASURE)
                .WithServiceTypes(SERVICE_TYPES)
                .Build();

            _mockAddressValidationService = new Mock<IAddressValidationService>();
            _mockAddressValidationService.Setup(_ => _.IsAddressValid(It.IsAny<Guid>(), It.IsAny<LoadStopData>())).Returns(true);
            _service = new LoadValidationService(_db.Object, _mockAddressValidationService.Object);
        }

        private void AssertHasError(BaseServiceResponse response, string urn, string message)
        {
            response.ModelState.Should().Contain(s => s.Key == urn && s.Value.Errors.Any(e => e.ErrorMessage == message));
        }

        private void AssertNoError(BaseServiceResponse response, string urn, string message)
        {
            response.ModelState.Should().NotContain(s => s.Key == urn && s.Value.Errors.Any(e => e.ErrorMessage == message));
        }

        private LoadDetailData MinimalValidLoad() => new LoadDetailData
        {
            ReferenceLoadId = "SHIPPER_12345",
            ReferenceLoadDisplay = "12345",
            Commodity = "Cardboard",
            EquipmentType = "BOAT",
            LoadStops = new List<LoadStopData>
                {
                    new LoadStopData { StopNbr = 1, City = "Green Bay", State = "WI", Country = "USA", LateDtTm = DateTime.Now.AddDays(1), ApptType = "BY" },
                    new LoadStopData { StopNbr = 2, City = "SAINT CATHARINES", State = "ON", Country = "CAN", LateDtTm = DateTime.Now.AddDays(2), ApptType = "BY" }
                },
            Contacts = new List<LoadContactData>
                {
                    new LoadContactData { Display = "Contact 1", Email = "e", Phone = "P" }
                }
        };

        [Fact]
        public void ValidateLoad_Empty()
        {
            var response = new GenericResponse<LoadDetailData>();
            _service.ValidateLoad(new LoadDetailData(), CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:ReferenceLoadId", "Reference Load Id is required");
            AssertHasError(response, "urn:root:EquipmentType", "Equipment Type is required");
            AssertHasError(response, "urn:root:LoadStops", "At least 2 stops required on load");
            AssertHasError(response, "urn:root:Contacts", "At least one contact is required");
        }


        [Fact]
        public void ValidateLoad_MinimumValid()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_EquipmentId()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.EquipmentType = "BOAT ID";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_InvalidEquipmentType()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.EquipmentType = "Invalid Value";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:EquipmentType", "Equipment Type is invalid");
        }

        [Fact]
        public void ValidateLoad_InvalidLineHaulRate()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LineHaulRate = -1;
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LineHaulRate", "Invalid Line Haul Rate");
        }

        [Fact]
        public void ValidateLoad_InvalidFuelRate()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.FuelRate = -1;
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:FuelRate", "Invalid Fuel Rate");
        }

        [Fact]
        public void ValidateLoad_InvalidWeight()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.Weight = -1;
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:Weight", "Invalid Weight");
        }

        [Fact]
        public void ValidateLoad_InvalidTotalAndLineItemWeight()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.Weight = 1000;
            load.LineItems = new List<LoadLineItemData>
            {
                new LoadLineItemData { LoadLineItemNumber = 1, ProductDescription = "Item 1", Quantity = 10, UnitOfMeasure = "Pallets", Weight = 100, Volume = 500, PickupStopNumber = 1, DeliveryStopNumber = 2 },
                new LoadLineItemData { LoadLineItemNumber = 2, ProductDescription = "Item 2", Quantity = 2, UnitOfMeasure = "Skid", Weight = 200, Volume = 500, PickupStopNumber = 1, DeliveryStopNumber = 2 }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:Weight", "The total load weight and the sum of the weights of the line items is not equal");
        }

        [Fact]
        public void ValidateLoad_SetsZeroWeightToLineItemTotal()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.Weight = 0;
            load.LineItems = new List<LoadLineItemData>
            {
                new LoadLineItemData { LoadLineItemNumber = 1, ProductDescription = "Item 1", Quantity = 10, UnitOfMeasure = "Pallets", Weight = 100, Volume = 500, PickupStopNumber = 1, DeliveryStopNumber = 2 },
                new LoadLineItemData { LoadLineItemNumber = 2, ProductDescription = "Item 2", Quantity = 2, UnitOfMeasure = "Skid", Weight = 200, Volume = 500, PickupStopNumber = 1, DeliveryStopNumber = 2 }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
            load.Weight.Should().Be(300);
        }


        [Fact]
        public void ValidateLoad_InvalidCube()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.Cube = -1;
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:Cube", "Invalid Cube");
        }

        [Fact]
        public void ValidateLoad_PickupStopWithoutDeliveryItem()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops = new List<LoadStopData>
            {
                new LoadStopData { StopNbr = 1, City = "Green Bay", State = "WI", Country = "USA", LateDtTm = DateTime.Now.AddDays(1), ApptType = "BY", StopType = "PICKUP" },
                new LoadStopData { StopNbr = 2, City = "SAINT CATHARINES", State = "ON", Country = "CAN", LateDtTm = DateTime.Now.AddDays(2), ApptType = "BY", StopType = "PICKUP" },
                new LoadStopData { StopNbr = 3, City = "Somewhere Else", State = "ON", Country = "CAN", LateDtTm = DateTime.Now.AddDays(2), ApptType = "BY", StopType = "DELIVERY"}
            };
            load.LineItems = new List<LoadLineItemData>
            {
                new LoadLineItemData { LoadLineItemNumber = 1, ProductDescription = "Item 1", Quantity = 10, UnitOfMeasure = "Pallets", Weight = 500, Volume = 100, PickupStopNumber = 1, DeliveryStopNumber = 3 },
                new LoadLineItemData { LoadLineItemNumber = 2, ProductDescription = "Item 2", Quantity = 2, UnitOfMeasure = "Skid", Weight = 500, Volume = 200, PickupStopNumber = 1, DeliveryStopNumber = 3 }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1", "Pickup Stop Not Assigned to Delivery Item");
        }

        [Fact]
        public void ValidateLoad_InvalidTransportationMode()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.TransportationMode = "Invalid Mode";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:TransportationMode", "Invalid Transportation Mode");
        }

        [Fact]
        public void ValidateLoad_DefaultsTransportationModeToTruck()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.TransportationMode = null;
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
            load.TransportationMode.Should().Be("TRUCK");
        }

        [Fact]
        public void ValidateLoad_ValidTransportationMode()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.TransportationMode = "INTERMODAL";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
            load.TransportationMode.Should().Be("INTERMODAL");
        }

        [Fact]
        public void ValidateLoad_InvalidWithOneStop()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops.RemoveRange(1, load.LoadStops.Count - 1);
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops", "At least 2 stops required on load");
        }

        [Fact]
        public void ValidateLoad_AssignsStopTypeWhenInterfacedAnd2StopsWithNoType()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "";
            load.LoadStops[1].StopType = "";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
            load.LoadStops[0].StopTypeId.Should().Be((int)StopTypeEnum.Pickup);
            load.LoadStops[1].StopTypeId.Should().Be((int)StopTypeEnum.Delivery);
        }

        [Fact]
        public void ValidateLoad_ManuallyCreatedDoesNotAssignsStopTypeWhenInterfacedAnd3StopsWithNoType()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "";
            load.LoadStops[1].StopType = "";
            load.LoadStops.Add(new LoadStopData { StopNbr = 3, City = "WAUSAU", State = "WI", Country = "US", LateDtTm = DateTime.Now.AddDays(2), ApptType = "BY" });
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            load.LoadStops[0].StopTypeId.Should().BeNull();
            load.LoadStops[1].StopTypeId.Should().BeNull();
            load.LoadStops[2].StopTypeId.Should().BeNull();
            AssertHasError(response, "urn:root:LoadStops:0:StopType", "Stop type is required");
            AssertHasError(response, "urn:root:LoadStops:1:StopType", "Stop type is required");
            AssertHasError(response, "urn:root:LoadStops:2:StopType", "Stop type is required");
        }

        [Fact]
        public void ValidateLoad_NotManuallyCreatedDoesNotAssignsStopTypeWhenInterfacedAnd3StopsWithNoType()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "";
            load.LoadStops[1].StopType = "";
            load.LoadStops.Add(new LoadStopData { StopNbr = 3, City = "WAUSAU", State = "WI", Country = "US", LateDtTm = DateTime.Now.AddDays(2), ApptType = "BY" });
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
            load.LoadStops[0].StopTypeId.Should().BeNull();
            load.LoadStops[1].StopTypeId.Should().BeNull();
            load.LoadStops[2].StopTypeId.Should().BeNull();
        }

        [Fact]
        public void ValidateLoad_DoesNotAssignsStopTypeWhenManualAnd2StopsWithNoType()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "";
            load.LoadStops[1].StopType = "";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            load.LoadStops[0].StopTypeId.Should().BeNull();
            load.LoadStops[1].StopTypeId.Should().BeNull();
            AssertHasError(response, "urn:root:LoadStops:0:StopType", "Stop type is required");
            AssertHasError(response, "urn:root:LoadStops:1:StopType", "Stop type is required");
        }

        [Fact]
        public void ValidateLoad_InvalidStopType()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "Invalid Stop Type";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:0:StopType", "Stop type is invalid");
        }

        [Fact]
        public void ValidateLoad_DuplicateStopNumbers()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopNbr = 10;
            load.LoadStops[1].StopNbr = 10;
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:StopNbr", "Stop number has been duplicated");
        }

        [Fact]
        public void ValidateLoad_StopTypesOutOfOrder()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "Delivery";
            load.LoadStops[1].StopType = "Pickup";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:StopType", "Pickup stop must come before any Delivery stops");
        }

        [Fact]
        public void ValidateLoad_StopTypesOutOfOrderMultiplePickups()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops.Add(new LoadStopData { StopNbr = 3, City = "WAUSAU", State = "WI", Country = "US", LateDtTm = DateTime.Now.AddDays(2), ApptType = "BY" });

            load.LoadStops[0].StopType = "Pickup";
            load.LoadStops[1].StopType = "Delivery";
            load.LoadStops[2].StopType = "Pickup";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:2:StopType", "Pickup stop must come before any Delivery stops");
        }

        [Fact]
        public void ValidateLoad_MultipleDeliveryStops()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops.Add(new LoadStopData { StopNbr = 3, City = "WAUSAU", State = "WI", Country = "US", LateDtTm = DateTime.Now.AddDays(2), ApptType = "BY" });

            load.LoadStops[0].StopType = "Pickup";
            load.LoadStops[1].StopType = "Delivery";
            load.LoadStops[2].StopType = "Delivery";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_MultiplePickupStops()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops.Add(new LoadStopData { StopNbr = 3, City = "WAUSAU", State = "WI", Country = "US", LateDtTm = DateTime.Now.AddDays(3), ApptType = "BY" });

            load.LoadStops[0].StopType = "Pickup";
            load.LoadStops[1].StopType = "Pickup";
            load.LoadStops[2].StopType = "Delivery";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_ManuallyCreateStopsRequiredAddressInformation()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0] = new LoadStopData { StopNbr = 1, StopType = "Pickup" };
            load.LoadStops[1] = new LoadStopData { StopNbr = 2, StopType = "Delivery" };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:0:LocationName", "Location Name Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:0:Address1", "Address Line 1 Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:0:City", "City Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:0:State", "State Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:0:Country", "Country Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:0:PostalCode", "Postal Code Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:1:LocationName", "Location Name Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:1:Address1", "Address Line 1 Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:1:City", "City Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:1:State", "State Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:1:Country", "Country Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:1:PostalCode", "Postal Code Must Be Provided");
        }

        [Fact]
        public void ValidateLoad_InvalidTimes()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].EarlyTime = "9999";
            load.LoadStops[0].LateTime = "";
            load.LoadStops[1].EarlyTime = "";
            load.LoadStops[1].LateTime = "9999";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:0:EarlyTime", "Time is invalid");
            AssertHasError(response, "urn:root:LoadStops:1:LateTime", "Time is invalid");
        }

        [Fact]
        public void ValidateLoad_ValidTimes()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].EarlyTime = "2359";
            load.LoadStops[0].LateTime = "";
            load.LoadStops[0].LocationName = "test";
            load.LoadStops[0].Address1 = "test";
            load.LoadStops[0].City = "test";
            load.LoadStops[0].State = "test";
            load.LoadStops[0].Country = "test";
            load.LoadStops[0].PostalCode = "test";
            load.LoadStops[0].StopType = "Pickup";
            load.LoadStops[0].AppointmentSchedulingCode = "CAR";
            load.LoadStops[0].AppointmentConfirmationCode = "CONF";

            load.LoadStops[1].EarlyTime = "";
            load.LoadStops[1].LateTime = "1010";
            load.LoadStops[1].LocationName = "test";
            load.LoadStops[1].Address1 = "test";
            load.LoadStops[1].City = "test";
            load.LoadStops[1].State = "test";
            load.LoadStops[1].Country = "test";
            load.LoadStops[1].PostalCode = "test";
            load.LoadStops[1].StopType = "Delivery";
            load.LoadStops[1].AppointmentSchedulingCode = "CAR";
            load.LoadStops[1].AppointmentConfirmationCode = "CONF";
            load.LineItems = new List<LoadLineItemData>()
            {
                new LoadLineItemData()
                {
                    PickupStopNumber = 1,
                    DeliveryStopNumber = 2,
                    Quantity = 10,
                    Weight = 10,
                    Volume = 10,
                    UnitOfMeasure = "Piece"
                }
            };

            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_SkipAddressVerification()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            _mockAddressValidationService.Verify(_ => _.IsAddressValid(It.IsAny<Guid>(), It.IsAny<LoadStopData>()), Times.Never);
        }

        [Fact]
        public void ValidateLoad_AddressVerificationSucceeds()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.Validate, false, "urn:root", response);

            foreach (var stop in load.LoadStops)
            {
                _mockAddressValidationService.Verify(_ => _.IsAddressValid(CUSTOMER_IDENT_ID, stop));
            }
            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_AddressVerificationFails()
        {
            var load = MinimalValidLoad();

            _mockAddressValidationService.Reset();
            _mockAddressValidationService.Setup(_ => _.IsAddressValid(CUSTOMER_IDENT_ID, load.LoadStops.FirstOrDefault(x => x.Country == "CAN"))).Returns(false);

            var response = new GenericResponse<LoadDetailData>();
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.Validate, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1", "Address information is invalid");
        }

        [Fact]
        public void ValidateLoad_AddressReplacementSucceeds()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();

            var addresses = load.LoadStops.Select(x => new GeocodeAddress(x)).ToList();

            foreach (var stop in load.LoadStops)
            {
                var address = addresses.FirstOrDefault(x => x.Address1 == stop.Address1);
                if (stop.City == "Green Bay")
                {
                    stop.City = "GreeneBay";
                }
                _mockAddressValidationService.Setup(_ => _.GetValidAddress(stop)).Returns(address);
            }

            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.Replace, false, "urn:root", response);

            foreach (var stop in load.LoadStops)
            {
                _mockAddressValidationService.Verify(_ => _.IsAddressValid(CUSTOMER_IDENT_ID, stop), Times.Never);
                _mockAddressValidationService.Verify(_ => _.GetValidAddress(stop), Times.Once);
            }
            response.IsSuccess.Should().BeTrue();
            load.LoadStops.Should().Contain(x => x.City == "Green Bay");
        }

        [Fact]
        public void ValidateLoad_ManuallyCreateNoAppointmentCodes()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:0:AppointmentSchedulingCode", "Appointment Scheduling Code Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:0:AppointmentConfirmationCode", "Appointment Confirmation Code Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:1:AppointmentSchedulingCode", "Appointment Scheduling Code Must Be Provided");
            AssertHasError(response, "urn:root:LoadStops:1:AppointmentConfirmationCode", "Appointment Confirmation Code Must Be Provided");
        }

        [Fact]
        public void ValidateLoad_InterfacedHasSchedulingCodeWithNoAppointmentCode()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].AppointmentSchedulingCode = "NAN";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:0:AppointmentConfirmationCode", "Appointment Confirmation Code is missing");
        }

        [Fact]
        public void ValidateLoad_InterfacedHasConfirmationCodeWithNoSchedulingCode()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].AppointmentConfirmationCode = "DNTC";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:AppointmentSchedulingCode", "Appointment Scheduling Code is missing");
        }

        [Fact]
        public void ValidateLoad_InvalidAppointmentCodes()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].AppointmentSchedulingCode = "Invalid Code";
            load.LoadStops[1].AppointmentConfirmationCode = "Invalid Code";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:AppointmentSchedulingCode", "Appointment Scheduling Code is invalid");
            AssertHasError(response, "urn:root:LoadStops:1:AppointmentConfirmationCode", "Appointment Confirmation Code is invalid");
        }

        [Fact]
        public void ValidateLoad_ValidAppointmentCodes()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].AppointmentSchedulingCode = "GPT";
            load.LoadStops[1].AppointmentConfirmationCode = "DNTC";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
            load.LoadStops[1].AppointmentSchedulerConfirmationTypeId.Should().Be(5);
        }

        [Fact]
        public void ValidateLoad_InvalidAppointmentCodesCombination()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].AppointmentSchedulingCode = "GPT";
            load.LoadStops[1].AppointmentConfirmationCode = "FCFS";
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:AppointmentConfirmationCode", "Appointment Confirmation Code cannot be used with the provided Appointment Scheduling Code");
        }

        [Fact]
        public void ValidateLoad_ValidLoadStopContact()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].Contacts = new List<LoadStopContactData>
            {
                new LoadStopContactData
                {
                    FirstName = "Test First",
                    LastName = "Test Last",
                    Email = "Test Email",
                    PhoneNumber = "Test Phone"
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_InvalidLoadStopContactFirstName()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].Contacts = new List<LoadStopContactData>
            {
                new LoadStopContactData
                {
                    LastName = "Test Last",
                    Email = "Test Email",
                    PhoneNumber = "Test Phone"
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:Contacts:0:FirstName", "Contact First Name is required");
        }

        [Fact]
        public void ValidateLoad_InvalidLoadStopContactLastName()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].Contacts = new List<LoadStopContactData>
            {
                new LoadStopContactData
                {
                    FirstName = "Test First",
                    Email = "Test Email",
                    PhoneNumber = "Test Phone"
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:Contacts:0:LastName", "Contact Last Name is required");
        }

        [Fact]
        public void ValidateLoad_EmptyLoadStopContactPositionValid()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].Contacts = new List<LoadStopContactData>
            {
                new LoadStopContactData
                {
                    FirstName = "Test First",
                    LastName = "Test Last",
                    Email = "Test Email",
                    PhoneNumber = "Test Phone"
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_EmptyLoadStopContactEmailValidWithValidPhone()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].Contacts = new List<LoadStopContactData>
            {
                new LoadStopContactData
                {
                    FirstName = "Test First",
                    LastName = "Test Last",
                    PhoneNumber = "Test Phone"
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_EmptyLoadStopContactPhoneValidWithValidEmail()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].Contacts = new List<LoadStopContactData>
            {
                new LoadStopContactData
                {
                    FirstName = "Test First",
                    LastName = "Test Last",
                    Email = "Test Email"
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_InvalidLoadStopContactEmailAndPhone()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[1].Contacts = new List<LoadStopContactData>
            {
                new LoadStopContactData
                {
                    FirstName = "Test First",
                    LastName = "Test Last"
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:Contacts:0:EmailOrPhoneNumber", "Contact Phone or Email is required");
        }

        [Fact]
        public void ValidateLoad_ContactMissingEmailAndPhone()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.Contacts.Add(new LoadContactData { Display = "Test", Email = "", Phone = "" });
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:Contacts:1:Email", "Email is required");
            AssertHasError(response, "urn:root:Contacts:1:Phone", "Phone is required");
        }
        
        [Fact]
        public void ValidateLoad_EmptyLineItem()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LineItems = new List<LoadLineItemData> { new LoadLineItemData() };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            //AssertHasError(response, "urn:root:LineItems:0:ProductDescription", "Product description is required");
            AssertHasError(response, "urn:root:LineItems:0:Quantity", "Quantity is invalid");
            AssertHasError(response, "urn:root:LineItems:0:Weight", "Weight is invalid");
            //AssertHasError(response, "urn:root:LineItems:0:Volume", "Volume is invalid");
            AssertHasError(response, "urn:root:LineItems:0:UnitOfMeasure", "Unit of Measure is required");
            AssertHasError(response, "urn:root:LineItems:0:PickupStopNumber", "Pickup Stop Number is required");
            AssertHasError(response, "urn:root:LineItems:0:DeliveryStopNumber", "Delivery Stop Number is required");
        }

        [Fact]
        public void ValidateLoad_InvalidUnitOfMeasure()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LineItems = new List<LoadLineItemData>
            {
                new LoadLineItemData
                {
                    ProductDescription = "Desc", Quantity = 15, Weight = 100, Volume = 200, UnitOfMeasure = "Invalid UOM",
                    PickupStopNumber = 1, DeliveryStopNumber = 2
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LineItems:0:UnitOfMeasure", "Unit of Measure is invalid");
        }

        [Fact]
        public void ValidateLoad_ValidLineItems()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LineItems = new List<LoadLineItemData>
            {
                new LoadLineItemData { LoadLineItemNumber = 1, ProductDescription = "Desc 1", Quantity = 15, Weight = 100, Volume = 200, UnitOfMeasure = "Pallets", PickupStopNumber = 1, DeliveryStopNumber = 2 },
                new LoadLineItemData { LoadLineItemNumber = 2, ProductDescription = "Desc 2", Quantity = 25, Weight = 200, Volume = 300, UnitOfMeasure = "Skid", PickupStopNumber = 1, DeliveryStopNumber = 2 }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
            load.LineItems[0].UnitOfMeasureId.Should().Be(2);
            load.LineItems[1].UnitOfMeasureId.Should().Be(4);
        }

        [Fact]
        public void ValidateLoad_LineItemsWithNoVolumeGetDefaultValue()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LineItems = new List<LoadLineItemData>
            {
                new LoadLineItemData { LoadLineItemNumber = 1, ProductDescription = "Desc 1", Quantity = 15, Weight = 100, Volume = 0, UnitOfMeasure = "Pallets", PickupStopNumber = 1, DeliveryStopNumber = 2 },
                new LoadLineItemData { LoadLineItemNumber = 2, ProductDescription = "Desc 2", Quantity = 25, Weight = 200, Volume = 0, UnitOfMeasure = "Skid", PickupStopNumber = 1, DeliveryStopNumber = 2 }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
            load.LineItems[0].Volume.Should().Be(1);
            load.LineItems[1].Volume.Should().Be(1);
            load.Cube.Should().Be(2);
        }

        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [Theory]
        public void IsValidTime_NullOrEmpty(string time)
        {
            _service.IsValidTime(time).Should().BeFalse();
        }

        [Fact]
        public void IsValidTime_TooShort()
        {
            _service.IsValidTime("123").Should().BeFalse();
        }

        [Fact]
        public void IsValidTime_TooLong()
        {
            _service.IsValidTime("12345").Should().BeFalse();
        }

        [Fact]
        public void IsValidTime_InvalidHours()
        {
            _service.IsValidTime("2400").Should().BeFalse();
        }

        [Fact]
        public void IsValidTime_InvalidMinutes()
        {
            _service.IsValidTime("1060").Should().BeFalse();
        }

        [InlineData("test")]
        [InlineData("-100")]
        [InlineData("00-1")]
        [InlineData("0a00")]
        [InlineData("000a")]
        [Theory]
        public void IsValidTime_Invalid(string time)
        {
            _service.IsValidTime(time).Should().BeFalse();
        }

        [InlineData("0000")]
        [InlineData("0101")]
        [InlineData("1010")]
        [InlineData("2359")]
        [Theory]
        public void IsValidTime_Valid(string time)
        {
            _service.IsValidTime(time).Should().BeTrue();
        }

        [Fact]
        public void ValidateLoad_EmptyServiceTypeName()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.ServiceTypes = new List<ServiceTypeData>
            {
                new ServiceTypeData
                {
                    Name = " "
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:ServiceTypes:0", "Service Type Name is required");
        }

        [Fact]
        public void ValidateLoad_InvalidServiceType()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.ServiceTypes = new List<ServiceTypeData>
            {
                new ServiceTypeData
                {
                    Name = "Invalid"
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:ServiceTypes:0", "Service Type is invalid");
        }

        [Fact]
        public void ValidateLoad_ValidServiceTypeSetsId()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.ServiceTypes = new List<ServiceTypeData>
            {
                new ServiceTypeData
                {
                    Name = "Team"
                }
            };
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, false, "urn:root", response);

            response.IsSuccess.Should().BeTrue();
            load.ServiceTypes[0].ServiceTypeId.Should().Be(2);
        }

        [Fact]
        public void ValidateLoad_PickupStopEarlyTimeInPast()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "PICKUP";
            load.LoadStops[1].StopType = "DELIVERY";
            load.LoadStops[0].EarlyDtTm = DateTime.Now.AddDays(-1);
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:0:EarlyDtTm", "Early Date Time cannot be in the past");
        }

        [Fact]
        public void ValidateLoad_PickupStopLateTimeInPast()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "PICKUP";
            load.LoadStops[1].StopType = "DELIVERY";
            load.LoadStops[0].LateDtTm = DateTime.Now.AddDays(-1);
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:0:LateDtTm", "Late Date Time cannot be in the past");
        }

        [Fact]
        public void ValidateLoad_DeliveryStopEarlyTimeInPast()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "PICKUP";
            load.LoadStops[1].StopType = "DELIVERY";
            load.LoadStops[1].EarlyDtTm = DateTime.Now.AddDays(-1);
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:EarlyDtTm", "Early Date Time cannot be in the past");
        }

        [Fact]
        public void ValidateLoad_DeliveryStopLateTimeInPast()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "PICKUP";
            load.LoadStops[1].StopType = "DELIVERY";
            load.LoadStops[1].LateDtTm = DateTime.Now.AddDays(-1);
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:LateDtTm", "Late Date Time cannot be in the past");
        }

        [Fact]
        public void ValidateLoad_PickupEarlyTimeAfterLateTime()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "PICKUP";
            load.LoadStops[1].StopType = "DELIVERY";
            load.LoadStops[0].EarlyDtTm = DateTime.Now.AddDays(2);
            load.LoadStops[0].LateDtTm = DateTime.Now.AddDays(1);
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:0:EarlyDtTm", "Early Date Time must be prior to the Late Date Time");
        }


        [Fact]
        public void ValidateLoad_DeliveryEarlyTimeAfterLateTime()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "PICKUP";
            load.LoadStops[1].StopType = "DELIVERY";
            load.LoadStops[1].EarlyDtTm = DateTime.Now.AddDays(2);
            load.LoadStops[1].LateDtTm = DateTime.Now.AddDays(1);
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:EarlyDtTm", "Early Date Time must be prior to the Late Date Time");
        }

        [Fact]
        public void ValidateLoad_DeliveryEarlyTimePriorToLastPickupTime()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "PICKUP";
            load.LoadStops[1].StopType = "DELIVERY";
            load.LoadStops[0].LateDtTm = DateTime.Now.AddDays(10);
            load.LoadStops[1].EarlyDtTm = DateTime.Now.AddDays(9);
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:EarlyDtTm", "Delivery Early Date Time must be after the Last Pickup Date Time");
        }

        [Fact]
        public void ValidateLoad_DeliveryLateTimePriorToLastPickupTime()
        {
            var response = new GenericResponse<LoadDetailData>();
            var load = MinimalValidLoad();
            load.LoadStops[0].StopType = "PICKUP";
            load.LoadStops[1].StopType = "DELIVERY";
            load.LoadStops[0].LateDtTm = DateTime.Now.AddDays(10);
            load.LoadStops[1].LateDtTm = DateTime.Now.AddDays(9);
            _service.ValidateLoad(load, CUSTOMER_IDENT_ID, OrderAddressValidationEnum.None, true, "urn:root", response);

            response.IsSuccess.Should().BeFalse();
            AssertHasError(response, "urn:root:LoadStops:1:LateDtTm", "Delivery Late Date Time must be after the Last Pickup Date Time");
        }
    }
}

