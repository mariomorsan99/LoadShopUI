using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class ShippingLoadData
    {
        public Guid? LoadId { get; set; }
        [Required]
        public string ReferenceLoadId { get; set; }
        [Required]
        public string ReferenceLoadDisplay { get; set; }
        [Required]
        public int Mileage { get; set; }
        [Required]
        public string Commodity { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        public string EquipmentId { get; set; }
        public string EquipmentCategoryId { get; set; }
        public string EquipmentCategoryDesc { get; set; }
        public string EquipmentTypeDisplay { get; set; }
        public string Comments { get; set; }
        [Required]
        public decimal LineHaulRate { get; set; }
        [Required]
        public decimal? SmartSpotRate { get; set; }
        [Required]
        public decimal ShippersFSC { get; set; }
        [Required]
        public bool OnLoadshop { get; set; }
        [Required]
        public List<LoadStopData> LoadStops { get; set; }

        public bool ScacsSentWithLoad { get; set; }
        public bool IsEstimatedFSC { get; set; }

        public CustomerData Customer { get; set; } = new CustomerData();
        public bool AllowEditingFuel { get { return Customer?.AllowEditingFuel ?? false; } }
        public bool? ManuallyCreated { get; set; }
        public int? CustomerLoadTypeId { get; set; }
        public decimal? HCapRate { get; set; }
        public decimal? XCapRate { get; set; }
        public decimal? DATGuardRate { get; set; }
        public decimal? MachineLearningRate { get; set; }
        public bool HasScacRestrictions { get; set; }

        public List<long> CarrierGroupIds { get; set; }

        public List<ServiceTypeData> ServiceTypes { get; set; }
    }
}
