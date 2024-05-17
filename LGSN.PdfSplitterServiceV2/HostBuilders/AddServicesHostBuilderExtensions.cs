using LGSN.PdfHandlerLibrary;

namespace LGSN.PdfSplitterServiceV2.HostBuilders
{
    public static class AddServicesHostBuilderExtensions
    {
        public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, services) => 
            {
                services.AddHostedService<Worker>();
                services.AddTransient<DocLib>();
                
            });
            return hostBuilder;
        }       
    }
}
