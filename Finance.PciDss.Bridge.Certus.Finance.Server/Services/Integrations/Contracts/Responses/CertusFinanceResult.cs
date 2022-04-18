using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses
{
    public class CertusFinanceResult
    {
        [JsonProperty("resultCode")] public string ResultCode { get; set; }
        [JsonProperty("resultMessage")] public string ResultMessage { get; set; }
        [JsonProperty("errorId")] public string ErrorId { get; set; }
        [JsonProperty("error")] public IEnumerable<CertusFinanceResultError> Error { get; set; }
        [JsonProperty("reasonCode")] public string ReasonCode { get; set; }
    }
}
