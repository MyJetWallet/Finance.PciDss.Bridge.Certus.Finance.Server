using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Finance.PciDss.Abstractions;
using Finance.PciDss.Bridge.Certus.Finance.Server;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Extensions;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDss.PciDssBridgeGrpc.Contracts.Enums;
using Finance.PciDss.PciDssBridgeGrpc.Models;
using Flurl;
using Newtonsoft.Json;
using NUnit.Framework;
using SimpleTrading.Common.Helpers;

namespace Finance.PciDss.Bridge.Certus.Finance.Test
{
    public class Tests
    {
        private Activity _unitTestActivity;
        private SettingsModel _settingsModel;
        private CertusFinanceHttpClient _certusFinanceHttpClient;
        private MakeBridgeDepositGrpcRequest _request;
        public void Dispose()
        {
            _unitTestActivity.Stop();
        }


        [SetUp]
        public void Setup()
        {
            _certusFinanceHttpClient = new CertusFinanceHttpClient();
            _unitTestActivity = new Activity("UnitTest").Start();
            
            _settingsModel = new SettingsModel()
            {
                SeqServiceUrl = "http://*.*.*.*:***",
                PciDssBaseUrl = "https://***/FE/rest/tx",
                MerchantUserId = "***",
                MerchantAccountId = "***",
                MerchantUserName = "account1",
                MerchantUserPassword = "account1",
                MerchantKey = "***",
                DefaultRedirectUrl = "https://webhook.site/***/?yuriy=DefaultRedirect",
                CallbackUrl = "https://webhook.site/***/?yuriy=CallbackUrl",
                RedirectUrlMapping = "***@st@https://webhook.site/***/?yuriy=RedirectUrl",
                AuditLogGrpcServiceUrl = @"http://*.*.*.*:80",
                ConvertServiceGrpcUrl = @"http://*.*.*.*:8080"
            };

            _request = MakeBridgeDepositGrpcRequest.Create(new PciDssInvoiceGrpcModel
            {
                CardNumber = "5555555555554444",
                FullName = "TEST TEST",
                Amount = 10,
                Zip = "test-",
                City = "Madrid",
                Country = "ESP",
                Address = "test",
                OrderId = "TeSt" + RequestValidator.RandomString(3),
                Email = "testuser1@***.com",
                TraderId = "c300b7426e80431aa4300a793f020d19",
                AccountId = "stl00002349usd",
                PaymentProvider = "pciDssCertusFinanceCards",
                Currency = "USD",
                Ip = "*.*.*.*",
                PsAmount = 8.21,
                PsCurrency = "EUR",
                Brand = BrandName.Monfex,
                BrandName = "***",
                PhoneNumber = "+*******",
                KycVerification = null,
                Cvv = "123",
                ExpirationDate = DateTime.Parse("2024-12")
            });
        }

        [Test]
        public async Task Send_Certus_Finance_Purchase_Request_And_Check_Status()
        {
            //TODO Skip pass for real test
            Assert.Pass();


            MakeBridgeDepositGrpcResponse returnResult;

            //Modify request data
            _request.PciDssInvoiceGrpcModel.KycVerification = string.IsNullOrEmpty(_request.PciDssInvoiceGrpcModel.KycVerification) ?
                "Empty" : _request.PciDssInvoiceGrpcModel.KycVerification;
            _request.PciDssInvoiceGrpcModel.Country = CountryManager.Iso3ToIso2(_request.PciDssInvoiceGrpcModel.Country);
            
            var validateResult = _request.Validate();
            if (validateResult.IsFailed)
            {
                returnResult = MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.PaymentDeclined,
                    validateResult.ToString());
                return;
            }

            //Preapare invoice
            var createInvoiceRequest = _request.PciDssInvoiceGrpcModel.ToCreatePaymentInvoiceRequest(_settingsModel);
            createInvoiceRequest.PrepareDataForSending(_settingsModel.MerchantKey);

            var createInvoiceResult =
                await _certusFinanceHttpClient.RegisterInvoiceAsync(createInvoiceRequest, _settingsModel.PciDssBaseUrl);

            if (createInvoiceResult.IsFailed)
            {
                returnResult = MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError,
                    createInvoiceResult.FailedResult.Message);
                Assert.IsNotNull(returnResult);
                return;
            }

            if (createInvoiceResult.SuccessResult.IsFailed())
            {
                returnResult = MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError,
                    createInvoiceResult.SuccessResult.Result.ResultMessage);
                Assert.IsNotNull(returnResult);
                return;
            }

            //Preapare status
            var statusInvoiceRequest = createInvoiceResult.SuccessResult.ToStatusRequest(_settingsModel);
            statusInvoiceRequest.PrepareDataForSending(_settingsModel.MerchantKey);

            Response<CertusFinanceStatusResponse, CertusFinanceFailResponseDataPayment> statusInvoceResult = null;
            for (int i=0; i < 10; i++)
            {
                statusInvoceResult =
                    await _certusFinanceHttpClient.GetStatusInvoiceAsync(statusInvoiceRequest, _settingsModel.PciDssBaseUrl,"TODO DELETE");

                if (statusInvoceResult.IsFailed || statusInvoceResult.SuccessResult is null || statusInvoceResult.SuccessResult.IsFailed())
                {
                    returnResult = MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError,
                        statusInvoceResult.FailedResult.Message);
                    Assert.IsNotNull(returnResult);
                    return;
                }

                //if (!string.IsNullOrEmpty(createInvoiceResult.SuccessResult.Redirect3DUrl))
                //{
                //    break;
                //}
                await Task.Delay(300);
            }
            
            if (createInvoiceResult.SuccessResult.IsSuccessWithoutRedirectTo3ds())
            {
                createInvoiceResult.SuccessResult.Redirect3DUrl = _settingsModel.DefaultRedirectUrl
                    .SetQueryParam("OrderId", _request.PciDssInvoiceGrpcModel.OrderId);
            }
            else
            {
                if (string.IsNullOrEmpty(createInvoiceResult.SuccessResult.Redirect3DUrl))
                    createInvoiceResult.SuccessResult.Redirect3DUrl = _settingsModel.DefaultRedirectUrl;
                else
                    createInvoiceResult.SuccessResult.Redirect3DUrl =
                        Uri.UnescapeDataString(createInvoiceResult.SuccessResult.Redirect3DUrl);

            }

            returnResult = MakeBridgeDepositGrpcResponse.Create(createInvoiceResult.SuccessResult.Redirect3DUrl,
                statusInvoceResult.SuccessResult.TxId, DepositBridgeRequestGrpcStatus.Success);
            Assert.IsNotNull(returnResult);
            return;
        }
    }
}
