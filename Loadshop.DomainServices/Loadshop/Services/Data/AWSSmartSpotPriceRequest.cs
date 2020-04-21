using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class AWSSmartSpotPriceRequest
    {
        public Guid LoadId { get; set; }
        public DateTime TransactionCreate { get; set; }
        public string TransactionTypeId { get; set; }
        public int LoadShopMiles { get; set; }
        /// <summary>
        /// TBD: We don't know how to get DirectMiles yet
        /// </summary>
        public int DirectMiles { get; set; }
        public int Stops { get; set; }
        public int Weight { get; set; }
        public string Commodity { get; set; }
        public string EquipmentId { get; set; }
        public DateTime PkupDate { get; set; }
        public string OrigState { get; set; }
        public string OriginZip { get; set; }
        public string O3Zip { get; set; }
        public string DestState { get; set; }
        public string DestZip { get; set; }
        public string D3Zip { get; set; }
        /// <summary>
        /// * For Shop It Post tab - total number of Scacs based on the number of carriers selected 
        ///   in the drop-downs
        /// * For Customer API - total number of CarrierScacs sent in from customer
        /// </summary>
        public int NbrSCACsRequest { get; set; }
        /// <summary>
        /// * For Shop It Post tab - total number of Carriers selected in the drop-downs
        /// * For Customer API - total number of unique Carriers based off Scacs sent in from customer
        /// </summary>
        public int NbrCarriersRequest { get; set; }
        /// <summary>
        /// * For manually-created loads that have not been posted yet, this is always zero (0).
        /// * For Shop It Post tab loads that are already on the Marketplace, this is the number
        ///   of LoadCarrierScac with a NULL ContractRate or a ContractRate <= the load's LineHaulRate.
        /// * For brand new loads being created via the Customer API, this number is always zero (0).
        /// * For loads being updated via the Customer API a Scac is included in this number if the 
        ///   LoadCarrierScac.ContractRate is NULL or <= the current LineHaulRate sent on this load.
        /// </summary>
        public int NbrSCACsPosted { get; set; }
        /// <summary>
        /// * For manually-created loads that have not been posted yet, this is always zero (0).
        /// * For Shop It Post tab loads that are already on the Marketplace, this is the number
        ///   of LoadCarrierScac with a NOT NULL ContractRate <= the load's LineHaulRate.
        /// * For brand new loads being created via the Customer API, this number is always zero (0).
        /// * For loads being updated via the Customer API a Scac is included in this number if the 
        ///   LoadCarrierScac.ContractRate <= the current LineHaulRate sent on this load.  The difference 
        ///   between this and NbrScacsPosted is that the LoadCarrierScac.ContractRate cannot be NULL for 
        ///   the Scac to be included here.
        /// </summary>
        public int NbrContractSCACsPosted { get; set; }
        /// <summary>
        /// * For manually-created loads that have not been posted yet, this is always zero (0).
        /// * For Shop It Post tab loads that are already on the Marketplace, this is the number
        ///   of LoadCarrierScac with a NOT NULL ContractRate > the load's LineHaulRate.
        /// * For brand new loads being created via the Customer API, this number is always zero (0).
        /// * For loads being updated via the Customer API a Scac is included in this number if the 
        ///   LoadCarrierScac.ContractRate > the current LineHaulRate sent on this load.
        ///   If LoadCarrierScac.ContractRate is NULL the Scac is not included in this number.  It only
        ///   tracks LoadCarrierScacs with valid ContractRates that are greater than the load's LineHaulRate.
        /// </summary>
        public int NbrSCACsHidden { get; set; }

        public override string ToString()
        {
            var transCreate = this.TransactionCreate.ToString("yyyy-MM-dd");
            var pickup = this.PkupDate.ToString("yyyy-MM-dd");
            var outputFields = new List<string>
            {
                LoadId.ToString("D"),
                TransactionTypeId,
                transCreate,
                LoadShopMiles.ToString(),
                DirectMiles.ToString(),
                Stops.ToString(),
                Weight.ToString(),
                // Commodity, Ivan from AWS has removed Commodity from Smart Spot Pricing due to data quality issues
                EquipmentId,
                NbrSCACsRequest.ToString(),
                NbrCarriersRequest.ToString(),
                NbrSCACsPosted.ToString(),
                NbrContractSCACsPosted.ToString(),
                NbrSCACsHidden.ToString(),
                D3Zip,
                O3Zip,
                pickup,
                OrigState,
                DestState
            };

            return string.Join(",", outputFields);
        }
    }
}
