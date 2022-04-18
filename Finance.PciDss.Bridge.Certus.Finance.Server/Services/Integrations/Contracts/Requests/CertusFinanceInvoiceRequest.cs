using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Destructurama.Attributed;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Extensions;
using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Requests
{
    public class CertusFinanceInvoiceRequest
    {
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("mId")] public string Mid { get; set; }
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("maId")] public string MaId { get; set; }
        [LogMasked]
        [JsonProperty("userName")] public string UserName { get; set; }
        [LogMasked]
        [JsonProperty("password")] public string Password { get; set; }
        [JsonProperty("remoteIP")] public string RemoteIP { get; set; }
        /// <summary>
        /// YYYY-MM-DD hh:mm:ss Ex. “2014-03-16 14:32:56”
        /// </summary>
        [JsonProperty("requestTime")] public string RequestTime { get; set; }
        [JsonProperty("signature")] public string Signature { get; set; }
        [JsonProperty("metaData")] public CertusFinanceInvoiceRequestMetaData MetaData { get; set; }
        [JsonProperty("txDetails")] public CertusFinanceInvoiceRequestTxDetails TxDetails { get; set; }
        public void PrepareDataForSending(string key)
        {
            Mid = Mid.HashDataInBase64();
            MaId = MaId.HashDataInBase64();
            UserName = UserName.EncryptDataInBase64(UserName, key);
            Password = Password.EncryptDataInBase64(Password, key);

            var values = new List<string>()
            {
                RequestTime,
                Mid,
                MaId,
                UserName,
                Password,
                TxDetails.ApiVersion,
                TxDetails.RequestId,
                TxDetails.Perform3DS,
                TxDetails.OrderData.OrderId,
                TxDetails.OrderData.OrderDescription,
                TxDetails.OrderData.Amount,
                TxDetails.OrderData.CurrencyCode,
                TxDetails.OrderData.Cc.CcNumber.ToString(),
                TxDetails.OrderData.Cc.CardHolderName,
                TxDetails.OrderData.Cc.Cvv.ToString(),
                TxDetails.OrderData.Cc.ExpirationMonth.ToString(),
                TxDetails.OrderData.Cc.ExpirationYear.ToString(),
                TxDetails.OrderData.BillingAddress.FirstName,
                TxDetails.OrderData.BillingAddress.LastName,
                TxDetails.OrderData.BillingAddress.Address1,
                TxDetails.OrderData.BillingAddress.City,
                TxDetails.OrderData.BillingAddress.Zipcode,
                TxDetails.OrderData.BillingAddress.CountryCode,
                TxDetails.OrderData.BillingAddress.Phone,
                TxDetails.OrderData.BillingAddress.Email,
                TxDetails.CancelUrl,
                TxDetails.ReturnUrl,
                TxDetails.NotificationUrl
            };
            var data = string.Join("", values);
            Signature = InvoiceUtils.CreateSignature(data, key);
        }
    }

    public class CertusFinanceInvoiceRequestMetaData
    {
        [NotLogged]
        [JsonProperty("merchantUserId")] public string MerchantUserId { get; set; }
    }

    public class CertusFinanceInvoiceRequestTxDetails
    {
        [JsonProperty("apiVersion")] public string ApiVersion { get; set; }
        [JsonProperty("requestId")] public string RequestId { get; set; }
        /// <summary>
        /// Should be "1"
        /// </summary>
        [JsonProperty("perform3DS")] public string Perform3DS { get; set; }
        [JsonProperty("orderData")] public CertusFinanceInvoiceRequestOrderData OrderData { get; set; }
        [JsonProperty("returnUrl")] public string ReturnUrl { get; set; }
        [JsonProperty("cancelUrl")] public string CancelUrl { get; set; }
        [JsonProperty("notificationUrl")] public string NotificationUrl { get; set; }
    }

    public class CertusFinanceInvoiceRequestOrderData
    {
        [JsonProperty("orderId")] public string OrderId { get; set; }
        [JsonProperty("orderDescription")] public string OrderDescription { get; set; }
        [JsonProperty("amount")] public string Amount { get; set; }
        /// <summary>
        /// ISO 4217 currency code. Ex. “USD”,”RUB”
        /// </summary>
        [JsonProperty("currencyCode")] public string CurrencyCode { get; set; }
        [JsonProperty("cc")] public CertusFinanceInvoiceRequestCc Cc { get; set; }
        [JsonProperty("billingAddress")] public CertusFinanceInvoiceRequestBillingAddress BillingAddress { get; set; }
        [JsonProperty("orderDetail")] public CertusFinanceInvoiceRequestOrderDetail OrderDetail { get; set; }
    }

    public class CertusFinanceInvoiceRequestCc
    {
        [LogMasked(ShowFirst = 6, ShowLast = 4, PreserveLength = true)]
        [JsonProperty("ccNumber")] public string CcNumber { get; set; }
        [LogMasked(PreserveLength = true)]
        [JsonProperty("cvv")] public string Cvv { get; set; }
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("cardHolderName")] public string CardHolderName { get; set; }
        [LogMasked(PreserveLength = true)]
        [JsonProperty("expirationMonth")] public string ExpirationMonth { get; set; }
        [LogMasked(PreserveLength = true)]
        [JsonProperty("expirationYear")] public string ExpirationYear { get; set; }
    }

    public class CertusFinanceInvoiceRequestBillingAddress
    {
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("firstName")] public string FirstName { get; set; }
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("lastName")] public string LastName { get; set; }
        [JsonProperty("address1")] public string Address1 { get; set; }
        //TODO ???//[JsonProperty("address2")] public string Address2 { get; set; }
        [JsonProperty("city")] public string City { get; set; }
        [JsonProperty("zipcode")] public string Zipcode { get; set; }
        //TODO ???//[JsonProperty("stateCode")] public string StateCode { get; set; }
        [JsonProperty("countryCode")] public string CountryCode { get; set; }
        //[LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        //TODO ???//[JsonProperty("mobile")] public string Mobile { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("phone")] public string Phone { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("email")] public string Email { get; set; }
        //[LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        //TODO ???//[JsonProperty("fax")] public string Fax { get; set; }
    }
    public class CertusFinanceInvoiceRequestOrderDetail
    {
        //TODO ???
        [JsonProperty("invoiceNo")] public string InvoiceNo { get; set; }
        //TODO ???
        [JsonProperty("mctMemo")] public string MctMemo { get; set; }
        [JsonProperty("ItemList")] public CertusFinanceInvoiceRequestOrderDetailItemList[] ItemList { get; set; }
    }

    public class CertusFinanceInvoiceRequestOrderDetailItemList
    {
        [JsonProperty("itemName")] public string ItemName { get; set; }
    }
}