using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses
{
    public class CertusFinanceAmount
    {
        [JsonProperty("amount")] public string Amount { get; set; }
        [JsonProperty("currencyCode")] public string CurrencyCode { get; set; }
    }
}
