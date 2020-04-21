using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using System;
using System.Linq;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class CustomerTransactionLogService : ICustomerTransactionLogService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        private readonly string _username = "LoadBoard.Customer.API";

        public CustomerTransactionLogService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public bool LogTransaction(Guid identUserId, string requestUri, string request, string response)
        {
            try
            {
                var customerId = GetCustomerId(identUserId);
                if (customerId.HasValue)
                {
                    _context.CustomerTransactionLogs.Add(new CustomerTransactionLogEntity()
                    {
                        CustomerId = customerId.Value,
                        RequestUri = requestUri,
                        RequestJSON = request,
                        ResponseJSON = response,
                        IsSuccess = WasResponseSuccessful(response)
                    });
                    _context.SaveChanges(_username);

                    return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        /// <summary>
        /// Gets customerId
        /// </summary>
        /// <param name="identUserId"></param>
        /// <returns></returns>
        public Guid? GetCustomerId(Guid identUserId)
        {
            var customer = _context.Customers.SingleOrDefault(x => x.IdentUserId == identUserId);
            return customer?.CustomerId;
        }

        /// <summary>
        /// Checks if a response was successful
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public bool WasResponseSuccessful(string response)
        {
            return response?.ToUpper().Contains("\"success\":true".ToUpper()) ?? false;
        }
    }
}
