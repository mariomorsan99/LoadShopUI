using System;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ICustomerTransactionLogService
    {
        /// <summary>
        /// Logs a customer transaction
        /// </summary>
        /// <param name="identUserId"></param>
        /// <param name="requestUri"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        bool LogTransaction(Guid identUserId, string requestUri, string request, string response);
    }
}
