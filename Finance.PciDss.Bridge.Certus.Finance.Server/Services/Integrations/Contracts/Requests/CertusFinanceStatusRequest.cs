using System.Collections.Generic;
using Destructurama.Attributed;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Extensions;
using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Requests
{
    public class CertusFinanceStatusRequest
    {
        [JsonProperty("requestTime")] public string RequestTime { get; set; }
        [JsonProperty("apiVersion")] public string ApiVersion { get; set; }
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("mId")] public string Mid { get; set; }
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("maId")] public string MaId { get; set; }
        [LogMasked]
        [JsonProperty("userName")] public string UserName { get; set; }
        [LogMasked]
        [JsonProperty("password")] public string Password { get; set; }
        [JsonProperty("txId")] public string TxId { get; set; }
        public void PrepareDataForSending(string key)
        {
            Mid = Mid.HashDataInBase64();
            MaId = MaId.HashDataInBase64();
            UserName = UserName.EncryptDataInBase64(UserName, key);
            Password = Password.EncryptDataInBase64(Password, key);
        }
    }
}