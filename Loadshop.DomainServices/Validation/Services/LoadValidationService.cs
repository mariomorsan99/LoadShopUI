using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Validation.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Loadshop.DomainServices.Loadshop.Services.Enum;

namespace Loadshop.DomainServices.Validation.Services
{
    public class LoadValidationService : ILoadValidationService
    {
        private readonly LoadshopDataContext _context;
        private readonly IAddressValidationService _addressValidationService;
        private LoadValidationMessages _messages;

        public LoadValidationService(LoadshopDataContext context, IAddressValidationService addressValidationService)
        {
            _context = context;
            _addressValidationService = addressValidationService;
        }
        public void ValidateLoad(LoadDetailData load, Guid customerId, OrderAddressValidationEnum validateAddress, bool manuallyCreated, string urnPrefix, BaseServiceResponse response)
        {
            _messages = manuallyCreated ? new WebUILoadValidationMessages() : new LoadValidationMessages();

            if (string.IsNullOrWhiteSpace(load.ReferenceLoadId))
            {
                response.AddError($"{urnPrefix}:{nameof(load.ReferenceLoadId)}", _messages.ReferenceLoadIdRequired);
            }
            if (string.IsNullOrWhiteSpace(load.ReferenceLoadDisplay))
            {
                response.AddError($"{urnPrefix}:{nameof(load.ReferenceLoadDisplay)}", _messages.ReferenceLoadDisplayRequired);
            }
            if (string.IsNullOrWhiteSpace(load.EquipmentType))
            {
                response.AddError($"{urnPrefix}:{nameof(load.EquipmentType)}", _messages.EquipmentTypeRequired);
            }
            else
            {
                var equipment = _context.Equipment.FirstOrDefault(_ => _.EquipmentId == load.EquipmentType || _.EquipmentDesc == load.EquipmentType);
                if (equipment == null)
                    response.AddError($"{urnPrefix}:{nameof(load.EquipmentType)}", _messages.EquipmentTypeInvalid);
                else
                    load.EquipmentType = equipment.EquipmentId;
            }

            if (load.LineHaulRate < 0)
            {
                response.AddError($"{urnPrefix}:{nameof(load.LineHaulRate)}", _messages.LineHaulRateInvalid);
            }
            if (load.FuelRate < 0)
            {
                response.AddError($"{urnPrefix}:{nameof(load.FuelRate)}", _messages.FuelRateInvalid);
            }

            var totalLineWeight = load.LineItems?.Sum(_ => _.Weight);
            if (load.Weight <= 0 && totalLineWeight > 0)
            {
                load.Weight = totalLineWeight.Value;
            }
            else if (load.Weight < 0)
            {
                response.AddError($"{urnPrefix}:{nameof(load.Weight)}", _messages.WeightInvalid);
            }
            else if (totalLineWeight > 0 && totalLineWeight != load.Weight)
            {
                response.AddError($"{urnPrefix}:{nameof(load.Weight)}", _messages.TotalWeightInvalid);
            }

            //default line item volume's to 1
            if (load.LineItems != null)
            {
                foreach (var lineItem in load.LineItems.Where(_ => _.Volume == 0))
                    lineItem.Volume = 1;
            }


            var totalLineVolume = load.LineItems?.Sum(_ => _.Volume);
            if (load.Cube <= 0 && totalLineVolume > 0)
            {
                load.Cube = totalLineVolume.Value;
            }
            else if (load.Cube < 0)
            {
                response.AddError($"{urnPrefix}:{nameof(load.Cube)}", _messages.CubeInvalid);
            }
            else if (totalLineVolume > 0 && load.Cube != totalLineVolume)
                response.AddError($"{urnPrefix}:{nameof(load.Cube)}", _messages.TotalVolumeInvalid);


            ConvertAndValidateTransportationMode(load, manuallyCreated, urnPrefix, response);
            ConvertAndValidateStopData(load, customerId, validateAddress, manuallyCreated, urnPrefix, response);
            ValidateContactData(load, manuallyCreated, urnPrefix, response);
            ConvertAndValidateLineData(load, manuallyCreated, urnPrefix, response);
            ConvertAndValidateServiceTypes(load, urnPrefix, response);
        }

        public virtual void ConvertAndValidateTransportationMode(LoadDetailData load, bool manuallyCreated, string urnPrefix, BaseServiceResponse response)
        {
            //default to truck
            if (string.IsNullOrWhiteSpace(load.TransportationMode))
                load.TransportationMode = "TRUCK";

            load.TransportationModeId = TransportationModes?.FirstOrDefault(_ => string.Compare(load.TransportationMode, _.Name, true) == 0)?.TransportationModeId;
            if (load.TransportationModeId == null)
                response.AddError($"{urnPrefix}:{nameof(load.TransportationMode)}", _messages.TransportationModeInvalid);
        }

        public virtual void ConvertAndValidateStopData(LoadDetailData load, Guid customerId, OrderAddressValidationEnum validateAddress, bool manuallyCreated, string urnPrefix, BaseServiceResponse response)
        {
            if (load.LoadStops == null || load.LoadStops.Count < 2)
            {
                response.AddError($"{urnPrefix}:{nameof(load.LoadStops)}", _messages.LoadStopsTooFew);
                return;
            }

            //convert the stop type
            var usedStopNumbers = new List<int>();
            var assignStopTypeIfOmitted = load.LoadStops.Count == 2 && !manuallyCreated;
            bool firstDeliveryStopFound = false;
            DateTime lastPickupDateTime = DateTime.MinValue;

            for (int i = 0; i < load.LoadStops.Count; i++)
            {
                var stopUrnPrefix = $"{urnPrefix}:{nameof(load.LoadStops)}:{i}";
                var stop = load.LoadStops[i];
                if (usedStopNumbers.Contains(stop.StopNbr))
                    response.AddError($"{stopUrnPrefix}:{nameof(stop.StopNbr)}", _messages.StopNumberDuplicated);
                usedStopNumbers.Add(stop.StopNbr);

                if (!string.IsNullOrWhiteSpace(stop.StopType))
                {
                    stop.StopTypeId = StopTypes?.FirstOrDefault(_ => string.Compare(_.Name, stop.StopType, true) == 0)?.StopTypeId;
                    if (stop.StopTypeId == null)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.StopType)}", _messages.StopTypeInvalid);
                }
                else if (assignStopTypeIfOmitted)
                    stop.StopTypeId = i == 0 ? (int?)StopTypeEnum.Pickup : (int?)StopTypeEnum.Delivery;
                else if (manuallyCreated)
                    response.AddError($"{stopUrnPrefix}:{nameof(stop.StopType)}", _messages.StopTypeRequired);

                if (stop.StopTypeId != null)
                {
                    if (stop.StopTypeId == (int)StopTypeEnum.Delivery)
                        firstDeliveryStopFound = true;
                    else if (firstDeliveryStopFound)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.StopType)}", _messages.StopTypeOutOfOrder);
                }

                if (manuallyCreated && stop.StopType == StopTypeEnum.Delivery.ToString())
                {
                    var stopLineItems = load.LineItems?.Where(x => x.DeliveryStopNumber == stop.StopNbr);
                    if (stopLineItems == null || stopLineItems.Count() == 0)
                    {
                        response.AddError($"{stopUrnPrefix}:{nameof(load.LineItems)}", _messages.LineItemsTooFew);
                    }
                }

                if (manuallyCreated)
                {
                    if (string.IsNullOrWhiteSpace(stop.LocationName))
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.LocationName)}", _messages.LocationNameRequired);
                    if (string.IsNullOrWhiteSpace(stop.Address1))
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.Address1)}", _messages.Address1Required);
                    if (string.IsNullOrWhiteSpace(stop.City))
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.City)}", _messages.CityRequired);
                    if (string.IsNullOrWhiteSpace(stop.State))
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.State)}", _messages.StateRequired);
                    if (string.IsNullOrWhiteSpace(stop.Country))
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.Country)}", _messages.CountryRequired);
                    if (string.IsNullOrWhiteSpace(stop.PostalCode))
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.PostalCode)}", _messages.PostalCodeRequired);
                }

                if (validateAddress == OrderAddressValidationEnum.Validate)
                {
                    if (!_addressValidationService.IsAddressValid(customerId, stop))
                        response.AddError($"{stopUrnPrefix}", _messages.AddressInvalid);
                }
                else if (validateAddress == OrderAddressValidationEnum.Replace)
                {
                    var address = _addressValidationService.GetValidAddress(stop);
                    if (address != null)
                    {
                        stop.Address1 = address.Address1;
                        stop.City = address.City;
                        stop.State = address.State;
                        stop.Country = address.Country;
                        stop.PostalCode = address.PostalCode;
                    }
                }

                if (stop.StopTypeId == (int)StopTypeEnum.Pickup)
                    lastPickupDateTime = stop.LateDtTm;

                ValidateStopTimes(stop, manuallyCreated, lastPickupDateTime, stopUrnPrefix, response);

                if (string.IsNullOrWhiteSpace(stop.ApptType))
                {
                    response.AddError($"{stopUrnPrefix}", _messages.ApptTypeRequired);
                }


                var schedulingCode = string.IsNullOrWhiteSpace(stop.AppointmentSchedulingCode) ? null : stop.AppointmentSchedulingCode;
                var confirmationCode = string.IsNullOrWhiteSpace(stop.AppointmentConfirmationCode) ? null : stop.AppointmentConfirmationCode;
                if (manuallyCreated)
                {
                    if (schedulingCode == null)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.AppointmentSchedulingCode)}", _messages.AppointmentSchedulingCodeRequired);
                    if (confirmationCode == null)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.AppointmentConfirmationCode)}", _messages.AppointmentConfirmationCodeRequired);
                }
                else
                {
                    //API loads can have both or none
                    if (schedulingCode == null && confirmationCode != null)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.AppointmentSchedulingCode)}", _messages.AppointmentSchedulingCodeMissing);
                    if (schedulingCode != null && confirmationCode == null)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.AppointmentConfirmationCode)}", _messages.AppointmentConfirmationCodeMissing);
                }

                if (schedulingCode != null && confirmationCode != null)
                {
                    if (!(AppointmentSchedulerConfirmationTypes?.Where(_ => string.Compare(_.AppointmentSchedulingCode, schedulingCode, true) == 0).Any() ?? false))
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.AppointmentSchedulingCode)}", _messages.AppointmentSchedulingCodeInvalid);
                    if (!(AppointmentSchedulerConfirmationTypes?.Where(_ => string.Compare(_.AppointmentConfirmationCode, confirmationCode, true) == 0).Any() ?? false))
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.AppointmentConfirmationCode)}", _messages.AppointmentConfirmationCodeInvalid);

                    stop.AppointmentSchedulerConfirmationTypeId = AppointmentSchedulerConfirmationTypes?
                        .FirstOrDefault(_ => string.Compare(_.AppointmentSchedulingCode, schedulingCode, true) == 0
                                            && string.Compare(_.AppointmentConfirmationCode, confirmationCode, true) == 0)
                        ?.AppointmentSchedulerConfirmationTypeId;

                    if (stop.AppointmentSchedulerConfirmationTypeId == null)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.AppointmentConfirmationCode)}", _messages.AppointmentCodeMismatch);
                }

                if (stop.StopTypeId == (int)StopTypeEnum.Pickup)
                {
                    if (manuallyCreated && !(load.LineItems?.Any(_ => _.PickupStopNumber == stop.StopNbr) ?? false))
                    {
                        response.AddError($"{stopUrnPrefix}", _messages.DeliveryItemRequired);
                    }
                }

                ValidateStopContactData(stop, stopUrnPrefix, response);
            }
        }

        private void ValidateStopTimes(LoadStopData stop, bool manuallyCreated, DateTime lastPickupTime, string stopUrnPrefix, BaseServiceResponse response)
        {
            //Validate the broken out times if provided
            var separateDateTimeFieldsUsed = !string.IsNullOrWhiteSpace(stop.EarlyTime) || !string.IsNullOrWhiteSpace(stop.LateTime);

            if (!string.IsNullOrWhiteSpace(stop.EarlyTime) && !IsValidTime(stop.EarlyTime))
                response.AddError($"{stopUrnPrefix}:{nameof(stop.EarlyTime)}", _messages.InvalidTime);
            if (!string.IsNullOrWhiteSpace(stop.LateTime) && !IsValidTime(stop.LateTime))
                response.AddError($"{stopUrnPrefix}:{nameof(stop.LateTime)}", _messages.InvalidTime);

            if (stop.LateDtTm == DateTime.MinValue)
            {
                response.AddError($"{stopUrnPrefix}:{nameof(stop.LateDtTm)}", _messages.LateDtTmRequired);
            }
            else if (manuallyCreated)
            {
                if (stop.EarlyDtTm.HasValue)
                {
                    if (stop.EarlyDtTm > stop.LateDtTm)
                    {
                        if (separateDateTimeFieldsUsed && stop.EarlyDtTm.Value.Date == stop.LateDtTm.Date)
                        {
                            response.AddError($"{stopUrnPrefix}:{nameof(stop.EarlyTime)}", _messages.EarlyDateTimeAfterLateDtTm);
                        }
                        else
                        {
                            response.AddError($"{stopUrnPrefix}:{nameof(stop.EarlyDtTm)}", _messages.EarlyDateTimeAfterLateDtTm);
                        }
                    }

                    if (stop.EarlyDtTm < DateTime.Now)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.EarlyDtTm)}", _messages.EarlyDateTimeInPast);
                }

                if (stop.LateDtTm < DateTime.Now)
                    response.AddError($"{stopUrnPrefix}:{nameof(stop.LateDtTm)}", _messages.LateDateTimeInPast);

                if (stop.StopTypeId == (int)StopTypeEnum.Delivery)
                {
                    if (stop.EarlyDtTm <= lastPickupTime)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.EarlyDtTm)}", _messages.DeliveryEarlyDateTimePriorToPickup);
                    if (stop.LateDtTm <= lastPickupTime)
                        response.AddError($"{stopUrnPrefix}:{nameof(stop.LateDtTm)}", _messages.DeliveryLateDateTimePriorToPickup);
                }
            }
        }

        /// <summary>
        /// Validates if a time is valid
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual bool IsValidTime(string time)
        {
            bool result = false;
            if (!string.IsNullOrWhiteSpace(time))
            {
                if (time.Length == 4)
                {
                    string hours = time.Substring(0, 2);
                    string minutes = time.Substring(2, 2);

                    if (int.TryParse(hours, out int hoursInt) && int.TryParse(minutes, out int minutesInt))
                    {
                        if (hoursInt >= 0 && hoursInt < 24 &&
                            minutesInt >= 0 && minutesInt < 60)
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        public virtual void ValidateStopContactData(LoadStopData stop, string urnPrefix, BaseServiceResponse response)
        {
            if (stop.Contacts == null)
                return;

            for (int i = 0; i < stop.Contacts.Count; i++)
            {
                var contactUrnPrefix = $"{urnPrefix}:{nameof(stop.Contacts)}:{i}";
                var contact = stop.Contacts[i];

                if (string.IsNullOrWhiteSpace(contact.FirstName))
                    response.AddError($"{contactUrnPrefix}:{nameof(contact.FirstName)}", _messages.ContactFirstNameRequired);

                if (string.IsNullOrWhiteSpace(contact.LastName))
                    response.AddError($"{contactUrnPrefix}:{nameof(contact.LastName)}", _messages.ContactLastNameRequired);


                var hasEmail = !string.IsNullOrWhiteSpace(contact.Email);
                var hasPhone = !string.IsNullOrWhiteSpace(contact.PhoneNumber);
                if (!hasEmail && !hasPhone)
                    response.AddError($"{contactUrnPrefix}:{nameof(contact.Email)}Or{nameof(contact.PhoneNumber)}", _messages.ContactEmailOrPhoneRequired);

            }
        }

        public virtual void ValidateContactData(LoadDetailData load, bool manuallyCreated, string urnPrefix, BaseServiceResponse response)
        {
            if (load.Contacts == null || load.Contacts.Count == 0)
                response.AddError($"{urnPrefix}:{nameof(load.Contacts)}", _messages.ContactsTooFew);
            else
            {
                for (int i = 0; i < load.Contacts.Count; i++)
                {
                    var contactUrnPrefix = $"{urnPrefix}:{nameof(load.Contacts)}:{i}";
                    var contact = load.Contacts[i];

                    if (manuallyCreated && string.IsNullOrWhiteSpace(contact.Display))
                        response.AddError($"{contactUrnPrefix}:{nameof(contact.Display)}", _messages.ContactDisplayRequired);

                    if (manuallyCreated)
                    {
                        // for manual loads we require both phone and email
                        if (string.IsNullOrWhiteSpace(contact.Email))
                            response.AddError($"{contactUrnPrefix}:{nameof(contact.Email)}", _messages.ContactEmailRequired);
                        if (string.IsNullOrWhiteSpace(contact.Phone))
                            response.AddError($"{contactUrnPrefix}:{nameof(contact.Phone)}", _messages.ContactPhoneRequired);
                    }
                    else
                    {
                        // for loads getting sent to use we require at least 1 phone OR email
                        if (string.IsNullOrWhiteSpace(contact.Email) && string.IsNullOrWhiteSpace(contact.Phone))
                        {
                            response.AddError($"{contactUrnPrefix}:{nameof(contact.Phone)}", _messages.ContactPhoneRequired);
                            response.AddError($"{contactUrnPrefix}:{nameof(contact.Email)}", _messages.ContactEmailRequired);
                        }
                    }
                }
            }
        }

        public virtual void ConvertAndValidateLineData(LoadDetailData load, bool manuallyCreated, string urnPrefix, BaseServiceResponse response)
        {
            if (load.LineItems?.Count > 0)
            {
                for (int i = 0; i < load.LineItems.Count; i++)
                {
                    var itemUrnPrefix = $"{urnPrefix}:{nameof(load.LineItems)}:{i}";
                    var lineItem = load.LineItems[i];
                    //if (string.IsNullOrWhiteSpace(lineItem.ProductDescription))
                    //    response.AddError($"{itemUrnPrefix}:{nameof(lineItem.ProductDescription)}", _messages.ProductDescriptionRequired);
                    if (lineItem.Quantity <= 0)
                        response.AddError($"{itemUrnPrefix}:{nameof(lineItem.Quantity)}", _messages.LineItemQuantityInvalid);
                    if (lineItem.Weight <= 0)
                        response.AddError($"{itemUrnPrefix}:{nameof(lineItem.Weight)}", _messages.LineItemWeightInvalid);
                    if (lineItem.Volume <= 0)
                        response.AddError($"{itemUrnPrefix}:{nameof(lineItem.Volume)}", _messages.LineItemVolumeInvalid);

                    if (string.IsNullOrWhiteSpace(lineItem.UnitOfMeasure))
                        response.AddError($"{itemUrnPrefix}:{nameof(lineItem.UnitOfMeasure)}", _messages.UnitOfMeasureRequired);
                    else
                    {
                        //match unit of measure on name or code
                        lineItem.UnitOfMeasureId = UnitsOfMeasure?.FirstOrDefault(_ =>
                            string.Compare(_.Name, lineItem.UnitOfMeasure, true) == 0
                            || string.Compare(_.Code, lineItem.UnitOfMeasure, true) == 0)?.UnitOfMeasureId;
                        if (lineItem.UnitOfMeasureId == null)
                            response.AddError($"{itemUrnPrefix}:{nameof(lineItem.UnitOfMeasure)}", _messages.UnitOfMeasureInvalid);
                    }

                    if (lineItem.PickupStopNumber <= 0)
                        response.AddError($"{itemUrnPrefix}:{nameof(lineItem.PickupStopNumber)}", _messages.LineItemPickupStopNumberRequired);
                    else if (!(load.LoadStops?.Any(_ => _.StopTypeId == (int)StopTypeEnum.Pickup && _.StopNbr == lineItem.PickupStopNumber) ?? false))
                        response.AddError($"{itemUrnPrefix}:{nameof(lineItem.PickupStopNumber)}", _messages.LineItemPickupStopNumberInvalid);

                    if (lineItem.DeliveryStopNumber <= 0)
                        response.AddError($"{itemUrnPrefix}:{nameof(lineItem.DeliveryStopNumber)}", _messages.LineItemDeliveryStopNumberRequired);
                    else if (!(load.LoadStops?.Any(_ => _.StopTypeId == (int)StopTypeEnum.Delivery && _.StopNbr == lineItem.DeliveryStopNumber) ?? false))
                        response.AddError($"{itemUrnPrefix}:{nameof(lineItem.DeliveryStopNumber)}", _messages.LineItemDeliveryStopNumberInvalid);
                }
            }
        }

        public virtual void ConvertAndValidateServiceTypes(LoadDetailData load, string urnPrefix, BaseServiceResponse response)
        {
            if (load.ServiceTypes?.Count > 0)
            {
                for (int i = 0; i < load.ServiceTypes.Count; i++)
                {
                    var serviceTypesUrnPrefix = $"{urnPrefix}:{nameof(load.ServiceTypes)}:{i}";
                    var serviceType = load.ServiceTypes[i];
                    if (string.IsNullOrWhiteSpace(serviceType.Name))
                        response.AddError($"{serviceTypesUrnPrefix}", _messages.ServiceTypeNameRequired);
                    else
                    {
                        serviceType.ServiceTypeId = ServiceTypes?.FirstOrDefault(_ => string.Compare(_.Name, serviceType.Name, true) == 0)?.ServiceTypeId ?? 0;
                        if (serviceType.ServiceTypeId <= 0)
                            response.AddError($"{serviceTypesUrnPrefix}", _messages.ServiceTypeInvalid);
                    }
                }
            }
        }


        private IList<TransportationModeEntity> _transportationModes = null;
        private IList<TransportationModeEntity> TransportationModes
        {
            get
            {
                if (_transportationModes == null)
                    _transportationModes = _context.TransportationModes.ToList();
                return _transportationModes;
            }
        }

        private IList<StopTypeEntity> _stopTypes = null;
        private IList<StopTypeEntity> StopTypes
        {
            get
            {
                if (_stopTypes == null)
                    _stopTypes = _context.StopTypes.ToList();
                return _stopTypes;
            }
        }

        private IList<AppointmentSchedulerConfirmationTypeEntity> _appointmentSchedulerConfirmationTypes = null;
        private IList<AppointmentSchedulerConfirmationTypeEntity> AppointmentSchedulerConfirmationTypes
        {
            get
            {
                if (_appointmentSchedulerConfirmationTypes == null)
                    _appointmentSchedulerConfirmationTypes = _context.AppointmentSchedulerConfirmationTypes.ToList();
                return _appointmentSchedulerConfirmationTypes;
            }
        }

        private IList<UnitOfMeasureEntity> _unitsOfMeasure = null;
        private IList<UnitOfMeasureEntity> UnitsOfMeasure
        {
            get
            {
                if (_unitsOfMeasure == null)
                    _unitsOfMeasure = _context.UnitOfMeasures.ToList();
                return _unitsOfMeasure;
            }
        }

        private IList<ServiceTypeEntity> _serviceTypes = null;
        private IList<ServiceTypeEntity> ServiceTypes
        {
            get
            {
                if (_serviceTypes == null)
                    _serviceTypes = _context.ServiceTypes.ToList();
                return _serviceTypes;
            }
        }
    }
}
