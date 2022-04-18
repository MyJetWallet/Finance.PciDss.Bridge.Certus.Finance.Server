using System;
using System.Diagnostics;
using System.Globalization;
using Finance.PciDss.Abstractions;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Requests;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses;
using Finance.PciDss.PciDssBridgeGrpc;
using Flurl;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Extensions
{
    public static class MapperExtensions
    {
        public static CertusFinanceInvoiceRequest ToCreatePaymentInvoiceRequest(this IPciDssInvoiceModel model, SettingsModel settingsModel)
        {
            var activityId = Activity.Current?.Id;
            return new CertusFinanceInvoiceRequest
            {
                TxDetails = new CertusFinanceInvoiceRequestTxDetails
                {
                    OrderData = new CertusFinanceInvoiceRequestOrderData()
                    {
                        OrderId = model.OrderId.Trim(),
                        OrderDescription = "Platform deposit",
                        Amount = string.Format("{0:0.00}", model.PsAmount).Trim(),
                        CurrencyCode= model.PsCurrency.Trim(),
                        Cc = new CertusFinanceInvoiceRequestCc()
                        {
                            CcNumber = model.CardNumber.Trim(),
                            CardHolderName = model.FullName.Length > 35 ? model.FullName[..35].Trim() : model.FullName.Trim(),
                            Cvv = model.Cvv.Trim(),
                            ExpirationMonth = model.ExpirationDate.ToString("MM").Trim(),
                            ExpirationYear = model.ExpirationDate.ToString("yyyy").Trim()
                        },
                        BillingAddress = new CertusFinanceInvoiceRequestBillingAddress()
                        {
                            FirstName = model.GetName().Trim(),
                            LastName = model.GetLastName().Trim(),
                            City = model.City.Trim(),
                            Zipcode = model.Zip.Trim(),
                            Address1 = model.Address.Trim(),
                            CountryCode = model.Country.Trim(),
                            Email = model.Email.Trim(),
                            Phone = model.PhoneNumber.Trim(),
                        },
                        OrderDetail = new CertusFinanceInvoiceRequestOrderDetail()
                        {
                            //TODO ???
                            InvoiceNo = "",
                            MctMemo = "",
                            ItemList = new CertusFinanceInvoiceRequestOrderDetailItemList[1]
                                {new CertusFinanceInvoiceRequestOrderDetailItemList()
                                    { ItemName = "Platform deposit"}},
                        },
                    },
                    Perform3DS = "1",
                    ApiVersion = "1.0.1",
                    RequestId = model.OrderId.Trim(),  //TODO ???
                    // callback
                    NotificationUrl = settingsModel.CallbackUrl.SetQueryParam(nameof(activityId), activityId),
                    // redirect
                    ReturnUrl = model.GetRedirectUrlForInvoice(settingsModel.RedirectUrlMapping.Trim(), settingsModel.DefaultRedirectUrl.Trim())
                        .SetQueryParam(nameof(activityId), activityId),
                    CancelUrl = model.GetRedirectUrlForInvoice(settingsModel.RedirectUrlMapping.Trim(), settingsModel.DefaultRedirectUrl.Trim())
                        .SetQueryParam(nameof(activityId), activityId)
                },
                
                MaId = settingsModel.MerchantAccountId.Trim(),
                Mid = settingsModel.MerchantUserId.Trim(),
                Password = settingsModel.MerchantUserPassword.Trim(),
                RemoteIP = model.Ip.Trim(),
                UserName = settingsModel.MerchantUserName.Trim(),
                RequestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss").Trim(),
                MetaData = new CertusFinanceInvoiceRequestMetaData()
                {
                    MerchantUserId = settingsModel.MerchantUserId.Trim()
                }
            };
        }

        public static CertusFinanceStatusRequest ToStatusRequest(this CertusFinanceInvoiceResponse src, SettingsModel settingsModel)
        {
            return new CertusFinanceStatusRequest
            {
                ApiVersion = "1.0.1",
                MaId = settingsModel.MerchantAccountId,
                Mid = settingsModel.MerchantUserId,
                Password = settingsModel.MerchantUserPassword,
                UserName = settingsModel.MerchantUserName,
                RequestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                TxId = src.TxId
            };
        }
    }
}
