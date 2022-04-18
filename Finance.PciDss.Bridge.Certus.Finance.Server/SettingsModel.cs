using SimpleTrading.SettingsReader;

namespace Finance.PciDss.Bridge.Certus.Finance.Server
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("PciDssBridgeCertusFinance.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.PciDssBaseUrl")]
        public string PciDssBaseUrl { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.MerchantUserId")]
        public string MerchantUserId { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.MerchantAccountId")]
        public string MerchantAccountId { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.MerchantUserName")]
        public string MerchantUserName { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.MerchantUserPassword")]
        public string MerchantUserPassword { get; set; }
        
        [YamlProperty("PciDssBridgeCertusFinance.MerchantKey")]
        public string MerchantKey { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.DefaultRedirectUrl")]
        public string DefaultRedirectUrl { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.CallbackUrl")]
        public string CallbackUrl { get; set; }

        //{brand}@{prefix}@{redirectUrl}|{brand}@{prefix}@{redirectUrl} 
        [YamlProperty("PciDssBridgeCertusFinance.RedirectUrlMapping")]
        public string RedirectUrlMapping { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.AuditLogGrpcServiceUrl")]
        public string AuditLogGrpcServiceUrl { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.ConvertServiceGrpcUrl")]
        public string ConvertServiceGrpcUrl { get; set; }

        [YamlProperty("PciDssBridgeCertusFinance.TurnOffConvertion")]
        public bool TurnOffConvertion { get; set; }
    }
}
