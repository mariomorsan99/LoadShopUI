namespace Loadshop.DomainServices.Loadshop.Services.Dto
{
    public class UpdateLoadOptions
    {
        public bool MapObject { get;  set; } = false;
        public bool ValidateAddress { get; set; } = false;
        public bool ManuallyCreated { get; set; } = false;
        public bool UpdateSpecialInstructions { get; set; } = true;
        public bool AppendComments { get; set; } = false;
        public bool SaveChanges { get; set; } = true;

        public bool IgnoreFuel { get; set; } = false;
        public bool IgnoreLineHaulRate { get; set; } = false;

        /// <summary>
        /// Add the Smart Spot rate to the load entity db record
        /// </summary>
        public bool AddSmartSpot { get; set; }

        /// <summary>
        /// Must have AddSmartSpot enabled, and will take that value to determine Linehaul and override
        /// </summary>
        public bool OverrideLineHaulWithSmartSpot { get; set; }
    }
}
