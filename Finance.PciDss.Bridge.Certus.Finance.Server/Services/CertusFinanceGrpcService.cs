using System;
using System.Threading.Tasks;
using Finance.PciDss.Abstractions;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Extensions;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Responses;
using Finance.PciDss.PciDssBridgeGrpc;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDss.PciDssBridgeGrpc.Contracts.Enums;
using Flurl;
using MyCrm.AuditLog.Grpc;
using MyCrm.AuditLog.Grpc.Models;
using Newtonsoft.Json;
using Serilog;
using SimpleTrading.Common.Helpers;
using SimpleTrading.ConvertService.Grpc;
using SimpleTrading.ConvertService.Grpc.Contracts;
using SimpleTrading.GrpcTemplate;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services
{
    public class CertusFinanceGrpcService : IFinancePciDssBridgeGrpcService
    {
        private const string PaymentSystemId = "pciDssCertusFinanceBankCards";
        private const string UsdCurrency = "USD";
        private const string EurCurrency = "EUR";
        private readonly ILogger _logger;
        private readonly GrpcServiceClient<IMyCrmAuditLogGrpcService> _myCrmAuditLogGrpcService;
        private readonly ISettingsModelProvider _optionsMonitorSettingsModelProvider;
        private readonly ICertusFinanceHttpClient _certusFinanceHttpClient;
        private readonly GrpcServiceClient<IConvertService> _convertServiceClient;

        public CertusFinanceGrpcService(ICertusFinanceHttpClient certusFinanceHttpClient,
            GrpcServiceClient<IMyCrmAuditLogGrpcService> myCrmAuditLogGrpcService,
            GrpcServiceClient<IConvertService> convertServiceClient,
            ISettingsModelProvider optionsMonitorSettingsModelProvider,
            ILogger logger)
        {
            _certusFinanceHttpClient = certusFinanceHttpClient;
            _myCrmAuditLogGrpcService = myCrmAuditLogGrpcService;
            _convertServiceClient = convertServiceClient;
            _optionsMonitorSettingsModelProvider = optionsMonitorSettingsModelProvider;
            _logger = logger;
        }

        private SettingsModel _settingsModel => _optionsMonitorSettingsModelProvider.Get();

        public async ValueTask<MakeBridgeDepositGrpcResponse> MakeDepositAsync(MakeBridgeDepositGrpcRequest request)
        {
            _logger.Information("CertusFinanceGrpcService start process MakeBridgeDepositGrpcRequest {@request}", request);
            try
            {
                request.PciDssInvoiceGrpcModel.Country = CountryManager.Iso3ToIso2(request.PciDssInvoiceGrpcModel.Country);

                var validateResult = request.Validate();
                if (validateResult.IsFailed)
                {
                    _logger.Warning("Certus.Finance request is not valid. Errors {@validateResult}", validateResult);
                    await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                        $"Fail Certus.Finance create invoice. Error {validateResult}");
                    return MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.PaymentDeclined,
                        validateResult.ToString());
                }
                //Preapare invoice
                var createInvoiceRequest = request.PciDssInvoiceGrpcModel.ToCreatePaymentInvoiceRequest(_settingsModel);
                createInvoiceRequest.PrepareDataForSending(_settingsModel.MerchantKey);

                _logger.Information("Certus.Finance send request {@Request}", createInvoiceRequest);
                await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                    @"Certus.Finance send sale request Amount: {createInvoiceRequest.Amount} currency: {createInvoiceRequest.Currency}");
                
                var createInvoiceResult =
                    await _certusFinanceHttpClient.RegisterInvoiceAsync(createInvoiceRequest, _settingsModel.PciDssBaseUrl);

                if (createInvoiceResult.IsFailed)
                {
                    await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                        $"{PaymentSystemId}. Fail Certus.Finance create invoice with kyc: {request.PciDssInvoiceGrpcModel.KycVerification}. Message: {createInvoiceResult.FailedResult.Message}. " +
                        $"Error: {JsonConvert.SerializeObject(createInvoiceResult.FailedResult.FieldError)}");
                    return MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError,
                        createInvoiceResult.FailedResult.Message);
                }


                if (createInvoiceResult.SuccessResult.IsFailed())
                {
                    await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                        $"{PaymentSystemId}. Fail Certus.Finance create invoice with kyc: {request.PciDssInvoiceGrpcModel.KycVerification}. Message: {createInvoiceResult.SuccessResult.Result.ResultMessage}. " +
                        $"Error: {JsonConvert.SerializeObject(createInvoiceResult.SuccessResult)}");
                    return MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.PaymentDeclined,
                        createInvoiceResult.SuccessResult.Result.ResultMessage);
                }

                // TODO: Delete.  В уловиях задачи было сказано, что работаем через 3DS
                //if (createInvoiceResult.SuccessResult.IsSuccessWithoutRedirectTo3ds())
                //{
                //    createInvoiceResult.SuccessResult.Redirect3DUrl = _settingsModel.DefaultRedirectUrl
                //        .SetQueryParam("OrderId", request.PciDssInvoiceGrpcModel.OrderId)
                //        .SetQueryParam("status", "success");
                //    _logger.Information("Certus.Finance is success without redirect to 3ds. RedirectUrl {url} was built for traderId {traderId} and orderid {orderid} {kyc}",
                //        createInvoiceResult.SuccessResult.Redirect3DUrl,
                //        request.PciDssInvoiceGrpcModel.TraderId,
                //        request.PciDssInvoiceGrpcModel.OrderId,
                //        request.PciDssInvoiceGrpcModel.KycVerification);

                //    await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel, 
                //        $"Certus.Finance was successful without redirect to 3ds. Orderid: {request.PciDssInvoiceGrpcModel.OrderId}");
                //}

                _logger.Information("Created deposit invoice {@Id} {@Kyc} {@RedirectUrl}",
                    request.PciDssInvoiceGrpcModel.OrderId, 
                    request.PciDssInvoiceGrpcModel.KycVerification, 
                    createInvoiceResult.SuccessResult.Redirect3DUrl);
                
                await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                    $"Created deposit invoice with id: {request.PciDssInvoiceGrpcModel.OrderId} " +
                    $"kyc: {request.PciDssInvoiceGrpcModel.KycVerification} " +
                    $"redirectUrl: {createInvoiceResult.SuccessResult.Redirect3DUrl}");

                return MakeBridgeDepositGrpcResponse.Create(createInvoiceResult.SuccessResult.Redirect3DUrl,
                    createInvoiceResult.SuccessResult.TxId, DepositBridgeRequestGrpcStatus.Success);
            }
            catch (Exception e)
            {
                _logger.Error(e, "MakeDepositAsync failed for traderId {traderId}",
                    request.PciDssInvoiceGrpcModel.TraderId);
                await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                    $"{PaymentSystemId}. MakeDeposit failed");
                return MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError, e.Message);
            }
        }

        public ValueTask<GetPaymentSystemGrpcResponse> GetPaymentSystemNameAsync()
        {
            return new ValueTask<GetPaymentSystemGrpcResponse>(GetPaymentSystemGrpcResponse.Create(PaymentSystemId));
        }

        public ValueTask<GetPaymentSystemCurrencyGrpcResponse> GetPsCurrencyAsync()
        {
            return new ValueTask<GetPaymentSystemCurrencyGrpcResponse>(
                GetPaymentSystemCurrencyGrpcResponse.Create(EurCurrency));
        }

        public async ValueTask<GetPaymentSystemAmountGrpcResponse> GetPsAmountAsync(GetPaymentSystemAmountGrpcRequest request)
        {
            if (_settingsModel.TurnOffConvertion)
            {
                return GetPaymentSystemAmountGrpcResponse.Create(request.Amount, request.Currency);
            }

            if (request.Currency.Equals(UsdCurrency, StringComparison.OrdinalIgnoreCase))
            {

                var convertResponse = await _convertServiceClient.Value.Convert(new CovertRequest
                {
                    InstrumentId = EurCurrency + UsdCurrency,
                    ConvertType = ConvertTypes.QuoteToBase,
                    Amount = request.Amount
                });

                return GetPaymentSystemAmountGrpcResponse.Create(convertResponse.ConvertedAmount, EurCurrency);
            }

            return default;
        }

        public ValueTask<GetDepositStatusGrpcResponse> GetDepositStatusAsync(GetDepositStatusGrpcRequest request)
        {
            throw new NotImplementedException();
        }

        public ValueTask<DecodeBridgeInfoGrpcResponse> DecodeInfoAsync(DecodeBridgeInfoGrpcRequest request)
        {
            throw new NotImplementedException();
        }

        public ValueTask<MakeConfirmGrpcResponse> MakeDepositConfirmAsync(MakeConfirmGrpcRequest request)
        {
            throw new NotImplementedException();
        }

        private ValueTask SendMessageToAuditLogAsync(IPciDssInvoiceModel invoice, string message)
        {
            return _myCrmAuditLogGrpcService.Value.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = invoice.TraderId,
                Action = "deposit",
                ActionId = invoice.OrderId,
                DateTime = DateTime.UtcNow,
                Message = message
            });
        }
    }
}