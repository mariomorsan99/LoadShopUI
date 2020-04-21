using System;
using System.Linq;
using System.Reflection;
using Loadshop.Data;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.Services.Utility
{
    public class ServiceUtilities
    {
        private IDateTimeProvider _dateTime;

        public ServiceUtilities(IDateTimeProvider dateTime)
        {
            _dateTime = dateTime;
        }
        /// <summary>
        /// Gets the contract rate for a load
        /// </summary>
        /// <param name="context"></param>
        /// <param name="loadId"></param>
        /// <param name="userId"></param>
        /// <returns name="result"></returns>
        public decimal? GetContractRate(LoadshopDataContext context, Guid loadId, Guid userId)
        {
            decimal? result = null;

            var user = context.Users.Where(x => x.IdentUserId == userId).FirstOrDefault();
            if (user != null && !string.IsNullOrWhiteSpace(user.PrimaryScac))
            {
                var loadScac = context.LoadCarrierScacs
                                        .Include(x => x.CarrierScac)
                                        .SingleOrDefault(x => x.LoadId == loadId && x.Scac == user.PrimaryScac);
                if (loadScac != null)
                {
                    result = !loadScac.CarrierScac.IsDedicated ? loadScac.ContractRate : null;
                }
            }

            return result;
        }

        public bool HasLoadChanged(LoadHistoryEntity originalValues, LoadHistoryEntity newValues)
        {
            var type = typeof(LoadHistoryEntity);
            foreach (PropertyInfo pi in type.GetProperties())
            {
                object value1 = type.GetProperty(pi.Name).GetValue(originalValues, null);
                object value2 = type.GetProperty(pi.Name).GetValue(newValues, null);

                if (value1 != value2 && (value1 == null || !value1.Equals(value2)))
                {
                    return true;
                }
            }
            return false;
        }

        public string[] MapShipperSearchTypeToTransactionList(ShipperSearchTypeData searchType)
        {
            string[] transactionTypeIds;

            switch (searchType)
            {
                // "All", excluding Removed
                case ShipperSearchTypeData.All:
                    {
                        transactionTypeIds = new string[] {
                            TransactionTypes.Posted,
                            TransactionTypes.PendingFuel,
                            TransactionTypes.PendingRates,
                            TransactionTypes.New,
                            TransactionTypes.Updated,
                            TransactionTypes.PendingUpdate,
                            TransactionTypes.PreTender,
                            TransactionTypes.Pending,
                            TransactionTypes.SentToShipperTender,
                            TransactionTypes.Accepted,
                            TransactionTypes.Delivered,
                            TransactionTypes.PendingRemoveScac
                        };
                        break;
                    }

                case ShipperSearchTypeData.PendingAdd:
                    {
                        transactionTypeIds = new string[] { TransactionTypes.PendingAdd, TransactionTypes.PendingUpdate };
                        break;
                    }

                case ShipperSearchTypeData.Posted:
                    {
                        transactionTypeIds = new string[] { TransactionTypes.Posted, TransactionTypes.PendingFuel, TransactionTypes.PendingRates, TransactionTypes.New, TransactionTypes.Updated };
                        break;
                    }

                case ShipperSearchTypeData.Booked:
                    {
                        transactionTypeIds = new string[] { TransactionTypes.PreTender, TransactionTypes.Pending, TransactionTypes.SentToShipperTender, TransactionTypes.Accepted };
                        break;
                    }

                case ShipperSearchTypeData.Delivered:
                    {
                        transactionTypeIds = new string[] { TransactionTypes.Delivered };
                        break;
                    }

                default:
                    throw new Exception("ShipperSearchType not valid");
            }

            return transactionTypeIds;
        }

        /// <summary>
        /// Returns the distance (in ticks) from the passed in from today.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public long DistanceFromToday(DateTime dateTime)
        {
            var offset = new DateTimeOffset(dateTime);

            return Math.Abs((_dateTime.UtcNow - offset).Ticks);
        }
    }
}
