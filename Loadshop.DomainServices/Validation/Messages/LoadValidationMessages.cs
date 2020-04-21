namespace Loadshop.DomainServices.Validation.Messages
{
    public class LoadValidationMessages
    {
        public virtual string ReferenceLoadIdRequired => "Reference Load Id is required";
        public virtual string ReferenceLoadDisplayRequired => "Reference Load Display is required";
        public virtual string CommodityRequired => "Commodity is required";
        public virtual string EquipmentTypeRequired => "Equipment Type is required";
        public virtual string EquipmentTypeInvalid => "Equipment Type is invalid";
        public virtual string LineHaulRateInvalid => "Invalid Line Haul Rate";
        public virtual string FuelRateInvalid => "Invalid Fuel Rate";
        public virtual string WeightInvalid => "Invalid Weight";
        public virtual string TotalWeightInvalid => "The total load weight and the sum of the weights of the line items is not equal";
        public virtual string CubeInvalid => "Invalid Cube";
        public virtual string TotalVolumeInvalid => "The total load volume and the sum of the volume of the line items is not equal";


        public virtual string TransportationModeInvalid => "Invalid Transportation Mode";

        public virtual string LoadStopsTooFew => "At least 2 stops required on load";
        public virtual string StopNumberDuplicated => "Stop number has been duplicated";
        public virtual string StopTypeInvalid => "Stop type is invalid";
        public virtual string StopTypeRequired => "Stop type is required";
        public virtual string StopTypeOutOfOrder => "Pickup stop must come before any Delivery stops";
        public virtual string LocationNameRequired => "Location Name is required";
        public virtual string Address1Required => "Address Line 1 is required";
        public virtual string CityRequired => "City is required";
        public virtual string StateRequired => "State is required";
        public virtual string CountryRequired => "Country is required";
        public virtual string PostalCodeRequired => "Postal Code is required";
        public virtual string AddressInvalid => "Address information is invalid";
        public virtual string LateDtTmRequired => "Late Date Time is required";
        public virtual string EarlyDateTimeAfterLateDtTm => "Early Date Time must be prior to the Late Date Time";
        public virtual string EarlyDateTimeInPast => "Early Date Time cannot be in the past";
        public virtual string LateDateTimeInPast => "Late Date Time cannot be in the past";
        public virtual string DeliveryEarlyDateTimePriorToPickup => "Delivery Early Date Time must be after the Last Pickup Date Time";
        public virtual string DeliveryLateDateTimePriorToPickup => "Delivery Late Date Time must be after the Last Pickup Date Time";
        public virtual string ApptTypeRequired => "Appointment Type is required";
        public virtual string InvalidTime => "Time is invalid";
        public virtual string DeliveryItemRequired => "Pickup Stop Not Assigned to Delivery Item";

        public virtual string AppointmentSchedulingCodeRequired => "Appointment Scheduling Code is required";
        public virtual string AppointmentConfirmationCodeRequired => "Appointment Confirmation Code is required";
        public virtual string AppointmentSchedulingCodeMissing => "Appointment Scheduling Code is missing";
        public virtual string AppointmentConfirmationCodeMissing => "Appointment Confirmation Code is missing";
        public virtual string AppointmentSchedulingCodeInvalid => "Appointment Scheduling Code is invalid";
        public virtual string AppointmentConfirmationCodeInvalid => "Appointment Confirmation Code is invalid";
        public virtual string AppointmentCodeMismatch => "Appointment Confirmation Code cannot be used with the provided Appointment Scheduling Code";

        public virtual string ContactFirstNameRequired => "Contact First Name is required";
        public virtual string ContactLastNameRequired => "Contact Last Name is required";
        public virtual string ContactEmailOrPhoneRequired => "Contact Phone or Email is required";

        public virtual string ContactsTooFew => "At least one contact is required";
        public virtual string ContactDisplayRequired => "Contact Display is required";
        public virtual string ContactEmailRequired => "Email is required";
        public virtual string ContactPhoneRequired => "Phone is required";

        //public virtual string ProductDescriptionRequired => "Product description is required";
        public virtual string QuantityInvalid => "Quantity is invalid";
        public virtual string VolumeInvalid => "Volume is invalid";
        public virtual string UnitOfMeasureRequired => "Unit of Measure is required";
        public virtual string UnitOfMeasureInvalid => "Unit of Measure is invalid";
        public virtual string LineItemsTooFew => "At least one Line Item is required";
        public virtual string LineItemQuantityInvalid => "Quantity is invalid";
        public virtual string LineItemWeightInvalid => "Weight is invalid";
        public virtual string LineItemVolumeInvalid => "Volume is invalid";
        public virtual string LineItemPickupStopNumberRequired => "Pickup Stop Number is required";
        public virtual string LineItemPickupStopNumberInvalid => "Pickup Stop Number is invalid";
        public virtual string LineItemDeliveryStopNumberRequired => "Delivery Stop Number is required";
        public virtual string LineItemDeliveryStopNumberInvalid => "Delivery Stop Number is invalid";

        public virtual string ServiceTypeNameRequired => "Service Type Name is required";
        public virtual string ServiceTypeInvalid => "Service Type is invalid";
    }

    public class WebUILoadValidationMessages : LoadValidationMessages
    {
        public override string ReferenceLoadIdRequired => "Order Number Must Be Provided";
        public override string CommodityRequired => "Commodity Must Be Provided";
        public override string EquipmentTypeRequired => "Equipment Type Must Be Provided";
        public override string ContactDisplayRequired => "Order Contact Name Must Be Provided";
        public override string ContactEmailOrPhoneRequired => "Contact Phone/Email Must Be Provided";
        public override string LocationNameRequired => "Location Name Must Be Provided";
        public override string LateDtTmRequired => "Stop Late Date Must be Provided";
        public override string ApptTypeRequired => "Appointment Type Must Be Provided";
        public override string AppointmentSchedulingCodeRequired => "Appointment Scheduling Code Must Be Provided";
        public override string AppointmentConfirmationCodeRequired => "Appointment Confirmation Code Must Be Provided";
        public override string Address1Required => "Address Line 1 Must Be Provided";
        public override string CityRequired => "City Must Be Provided";
        public override string StateRequired => "State Must Be Provided";
        public override string CountryRequired => "Country Must Be Provided";
        public override string PostalCodeRequired => "Postal Code Must Be Provided";
        public override string AddressInvalid => "Address Not Found";
        public override string ContactFirstNameRequired => "Contact First Name Must Be Provided";
        public override string ContactLastNameRequired => "Contact Last Name Must Be Provided";
        public override string LineItemQuantityInvalid => "Item Quantity Must Be Provided";
        public override string UnitOfMeasureInvalid => "Item Unit Of Measurement Must Be Provided";
        public override string LineItemWeightInvalid => "Item Weight Must Be Provided";
    }
}
