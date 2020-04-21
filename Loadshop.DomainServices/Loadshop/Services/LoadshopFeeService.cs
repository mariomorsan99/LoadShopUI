using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class LoadshopFeeService : ILoadshopFeeService
    {
        public void ApplyLoadshopFee(string usersPrimaryScac, ILoadFeeData load, IList<CustomerEntity> customers)
        {
            // get the customer 
            var customer = customers?.FirstOrDefault(_ => load.CustomerId == _.CustomerId);
            if (customer == null)
            {
                throw new Exception("Customer not found");
            }

            var inNetwork = customer.CustomerCarrierScacContracts?.Any(_ => _.Scac == (load.Scac ?? usersPrimaryScac)) ?? false;
            var flatFee = inNetwork ? customer.InNetworkFlatFee : customer.OutNetworkFlatFee;
            var percentFee = inNetwork ? customer.InNetworkPercentFee : customer.OutNetworkPercentFee;
            var feeAdd = inNetwork ? customer.InNetworkFeeAdd : customer.OutNetworkFeeAdd;
            var fee = flatFee + Math.Round(percentFee * (load.LineHaulRate + load.FuelRate), 2, MidpointRounding.AwayFromZero);

            if (!feeAdd)
                load.LineHaulRate -= fee;

            load.FeeData = new LoadshopFeeData
            {
                LoadshopFlatFee = flatFee,
                LoadshopPercentFee = percentFee,
                LoadshopFee = fee,
                LoadshopFeeAdd = feeAdd
            };
        }

        public void ReapplyLoadshopFeeToLineHaul(ILoadFeeData load)
        {
            if (load.FeeData?.LoadshopFeeAdd == false)
                load.LineHaulRate -= load.FeeData.LoadshopFee;
        }
    }
}
