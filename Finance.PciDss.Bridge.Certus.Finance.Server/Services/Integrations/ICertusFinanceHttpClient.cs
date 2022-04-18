using System.Threading.Tasks;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Requests;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations
{
    public interface ICertusFinanceHttpClient
    {
        /// <summary>
        /// A purchase deduct amount immediately. This transaction type is intended when the goods or services
        /// can be immediately provided to the customer. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        Task<Response<CertusFinanceInvoiceResponse, CertusFinanceFailResponseDataPayment>> RegisterInvoiceAsync(
            CertusFinanceInvoiceRequest request, string baseUrl);

        /// <summary>
        /// It allows to get previous transaction basic information
        /// </summary>
        /// <param name="request"></param>
        /// <param name="baseUrl"></param>
        /// <param name="endpointId"></param>
        /// <returns></returns>
        Task<Response<CertusFinanceStatusResponse, CertusFinanceFailResponseDataPayment>> GetStatusInvoiceAsync(
            CertusFinanceStatusRequest request, string baseUrl, string endpointId);

    }
}