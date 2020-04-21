using Loadshop.DomainServices.Loadshop.Services.Enum;

namespace Loadshop.DomainServices.Loadshop.Services.Dto
{
    public class CreateLoadOptionsDto
    {
        public OrderAddressValidationEnum ValidateAddress { get; set; } = OrderAddressValidationEnum.None;
        public bool ManuallyCreated { get; set; } = false;
        public bool AddSpecialInstructions { get; set; } = true;

        /// <summary>
        /// Add the Smart Spot rate to the load entity db record
        /// </summary>
        public bool AddSmartSpot { get; set; }

        /// <summary>
        /// Must have AddSmartSpot enabled, and will take that value to determine Linehaul and override
        /// </summary>
        public bool OverrideLineHaulWithSmartSpot { get; set; }
        /// <summary>
        /// Used to 0 any provided line haul
        /// </summary>
        public bool RemoveLineHaulRate { get; set; } = true;
    }
}
