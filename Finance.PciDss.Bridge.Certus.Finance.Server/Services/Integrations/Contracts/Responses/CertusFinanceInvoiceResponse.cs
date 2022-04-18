using System;
using System.Collections.Generic;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Enums;
using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses
{
    public class CertusFinanceInvoiceResponse
    {
        [JsonProperty("responseTime")] public string ResponseTime { get; set; }
        [JsonProperty("result")] public CertusFinanceResult Result { get; set; }
        [JsonProperty("signature")] public string Signature { get; set; }
        [JsonProperty("metaData")] public CertusFinanceMetaData MetaData { get; set; }
        [JsonProperty("txId")] public string TxId { get; set; }
        [JsonProperty("txTypeId")] public string TxTypeId { get; set; }
        [JsonProperty("txType")] public string TxType { get; set; }
        [JsonProperty("recurrentTypeId")] public string RecurrentTypeId { get; set; }
        [JsonProperty("requestId")] public string RequestId { get; set; }
        [JsonProperty("orderId")] public string OrderId { get; set; }
        [JsonProperty("sourceAmount")] public CertusFinanceAmount SourceAmount { get; set; }
        [JsonProperty("amount")] public CertusFinanceAmount Amount { get; set; }
        [JsonProperty("returnUrl")] public string ReturnUrl { get; set; }
        [JsonProperty("cancelUrl")] public string CancelUrl { get; set; }
        [JsonProperty("ccNumber")] public string CcNumber { get; set; }
        [JsonProperty("cardId")] public string CardId { get; set; }
        [JsonProperty("redirect3DUrl")] public string Redirect3DUrl { get; set; }

        public bool IsFailed()
        {
            if (string.IsNullOrEmpty(Result.ResultCode))
            {
                return true;
            }
            var status = (CertusFinanceTransactionResultCode)Enum.Parse(typeof(CertusFinanceTransactionResultCode), Result.ResultCode);
            return (status == CertusFinanceTransactionResultCode.Expired ||
                    status == CertusFinanceTransactionResultCode.Cancelled ||
                    status == CertusFinanceTransactionResultCode.Failed);
        }

        public bool IsSuccessWithoutRedirectTo3ds()
        {
            if (string.IsNullOrEmpty(Result.ResultCode))
            {
                return false;
            }
            var status = (CertusFinanceTransactionResultCode)Enum.Parse(typeof(CertusFinanceTransactionResultCode), Result.ResultCode);
            return !IsFailed() && 
                   status == CertusFinanceTransactionResultCode.CompletedSuccessfully;
        }

        public bool IsEnrolled3Ds()
        {
            if (string.IsNullOrEmpty(Result.ResultCode) || string.IsNullOrEmpty(Redirect3DUrl))
            {
                return false;
            }
            var status = (CertusFinanceTransactionResultCode)Enum.Parse(typeof(CertusFinanceTransactionResultCode), Result.ResultCode);
            return !IsFailed() && 
                   status == CertusFinanceTransactionResultCode.Enrolled3Ds &&
                   !string.IsNullOrEmpty(Redirect3DUrl);
        }
    }
}