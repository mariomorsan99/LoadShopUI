using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System;

namespace Loadshop.DomainServices.Utilities
{
    public static class FscUtilities
    {
        public static readonly int DEFAULT_FUEL_RERATING_NUMBER_OF_DAYS = 5;

        public static bool IsEstimatedFsc(CustomerEntity customer, DateTime originPickupDate, DateTime now)
        {
            if (customer == null || !customer.UseFuelRerating)
            {
                return false;
            }

            return IsEstimatedFsc(originPickupDate, now, customer.FuelReratingNumberOfDays);
        }

        public static bool IsEstimatedFsc(CustomerData customer, DateTime originPickupDate, DateTime now)
        {
            if (customer == null || !customer.UseFuelRerating)
            {
                return false;
            }

            return IsEstimatedFsc(originPickupDate, now, customer.FuelReratingNumberOfDays);
        }

        private static bool IsEstimatedFsc(DateTime originPickupDate, DateTime now, int fuelReratingNumberOfDays)
        {
            // If the customer is set to use fuel rerating, then the load's fuel rate is an estimate
            // if the load's pickup date is further out in the future than the customers fuel rerating
            // number of days.
            return originPickupDate > now.AddDays(fuelReratingNumberOfDays);
        }
    }
}
