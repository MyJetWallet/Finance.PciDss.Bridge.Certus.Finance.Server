using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses
{
    public class CertusFinanceMetaData
    {
        [JsonProperty("isShowResultMsgScreen")] public string IsShowResultMsgScreen { get; set; }
    }
}
