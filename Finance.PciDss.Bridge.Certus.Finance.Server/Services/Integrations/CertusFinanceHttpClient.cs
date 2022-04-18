using System.Threading.Tasks;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Extensions;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Requests;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations
{
    public class CertusFinanceHttpClient : ICertusFinanceHttpClient
    {
        public async Task<Response<CertusFinanceInvoiceResponse, CertusFinanceFailResponseDataPayment>> 
            RegisterInvoiceAsync(CertusFinanceInvoiceRequest request, string baseUrl)
        {
            var result = await baseUrl
                .AppendPathSegments("sync","purchase")
                .WithHeader("Content-Type", "application/json")
                //.AllowHttpStatus("400")
                .PostJsonAsync(request);
            return await result.DeserializeTo<CertusFinanceInvoiceResponse, CertusFinanceFailResponseDataPayment>();
        }

        public async Task<Response<CertusFinanceStatusResponse, CertusFinanceFailResponseDataPayment>> 
            GetStatusInvoiceAsync(CertusFinanceStatusRequest request, string baseUrl, string endpointId)
        {
            var result = await baseUrl
                .AppendPathSegments("getStatus")
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(request);
            return await result.DeserializeTo<CertusFinanceStatusResponse, CertusFinanceFailResponseDataPayment>();
        }
    }
}