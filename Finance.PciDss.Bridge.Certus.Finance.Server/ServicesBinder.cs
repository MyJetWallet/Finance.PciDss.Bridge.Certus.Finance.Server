using Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations;
using Microsoft.Extensions.DependencyInjection;
using MyCrm.AuditLog.Grpc;
using Serilog;
using SimpleTrading.ConvertService.Grpc;
using SimpleTrading.GrpcTemplate;
using SimpleTrading.MyLogger;
using SimpleTrading.SettingsReader;

namespace Finance.PciDss.Bridge.Certus.Finance.Server
{
    public static class ServicesBinder
    {
        public static string AppName { get; private set; } = "Finance.PciDss.Bridge.Certus.Finance.Server";

        public static void BindCertusFinanceHttpCLient(this IServiceCollection services)
        {
            services.AddSingleton<ICertusFinanceHttpClient, CertusFinanceHttpClient>();
        }

        public static void BindLogger(this IServiceCollection services, SettingsModel settings)
        {
            var logger = new MyLogger(AppName, settings.SeqServiceUrl);
            Log.Logger = logger;
            services.AddSingleton<ILogger>(logger);
        }

        public static void BindSettings(this IServiceCollection services, SettingsModel settings)
        {
            services.AddSingleton<ISettingsModelProvider, SettingsModelProvider>();
        }

        public static void BindGrpcServices(this IServiceCollection services, SettingsModel settings)
        {
            var client = new GrpcServiceClient<IMyCrmAuditLogGrpcService>(
                () => SettingsReader
                    .ReadSettings<SettingsModel>()
                    .AuditLogGrpcServiceUrl);
            services.AddSingleton(client);

            var clientConvertService = new GrpcServiceClient<IConvertService>(
                () => SettingsReader
                    .ReadSettings<SettingsModel>()
                    .ConvertServiceGrpcUrl);

            services.AddSingleton(clientConvertService);
        }
    }
}
