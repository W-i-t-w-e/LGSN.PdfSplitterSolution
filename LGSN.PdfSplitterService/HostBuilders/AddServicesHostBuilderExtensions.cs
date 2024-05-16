using LGSN.PdfSharpLibrary;
using LGSN.PdfSplitterService.Services;

namespace LGSN.PdfSplitterService.HostBuilders
{
    public static class AddServicesHostBuilderExtensions
    {
        public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, services) => 
            {
                services.AddHostedService<Worker>();
                services.AddTransient<SplitterService>();
                services.AddTransient<PdfBuilder>();
                
            });
            return hostBuilder;
        }       
    }
}
