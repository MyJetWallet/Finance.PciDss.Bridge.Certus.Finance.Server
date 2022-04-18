using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses
{
    public class CertusFinanceResultError
    {
        [JsonProperty("errorCode")] public string ErrorCode { get; set; }
        [JsonProperty("errorMessage")] public string ErrorMessage { get; set; }
        [JsonProperty("advice")] public string Advice { get; set; }
    }
}
