using LGSN.PdfSplitterServiceV2.HostBuilders;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .AddLogging()
    .UseWindowsService()
    .AddServices()
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
try
{
    // ASCII generator at https://de.rakko.tools/tools/68/ font: clr5x10
    var appVersion = Assembly.GetExecutingAssembly().GetName().Version;

    logger.LogInformation("  ###      #    ##         ##         ##     #                                   ##                      #                      #  #   ##");
    logger.LogInformation("  #  #     #   #          #  #         #           #     #                      #  #                                            #  #  #  #");
    logger.LogInformation("  #  #   ###   #          #     ###    #    ##    ####  ####   ##   # ##        #      ##   # ##  #  #  ##     ###   ##         #  #     #");
    logger.LogInformation("  ###   #  #  ###          ##   #  #   #     #     #     #    #  #  ##           ##   #  #  ##    #  #   #    #     #  #        #  #    #");
    logger.LogInformation("  #     #  #   #             #  #  #   #     #     #     #    ####  #              #  ####  #     #  #   #    #     ####         ##    #");
    logger.LogInformation("  #     #  #   #          #  #  #  #   #     #     #     #    #     #           #  #  #     #      ##    #    #     #            ##   #");
    logger.LogInformation("  #      ###   #           ##   ###   ###   ###     ##    ##   ##   #            ##    ##   #      ##   ###    ###   ##          ##   ####");
    logger.LogInformation("                                #");
    logger.LogInformation("                                #");
    logger.LogInformation("               #                                  #                 #  #                                      #  #              ##    #");
    logger.LogInformation("                     #     #                      #                 #  #                                      #  #               #    #");
    logger.LogInformation(" #   #  # ##  ##    ####  ####   ##   ###         ###   #  #        #  #   ###  ###   ###    ##    ###        #  #   ##    ##    #    #  #");
    logger.LogInformation(" # # #  ##     #     #     #    #  #  #  #        #  #  #  #        ####  #  #  #  #  #  #  #  #  #           #  #  #  #  #  #   #    # #");
    logger.LogInformation(" # # #  #      #     #     #    ####  #  #        #  #  #  #        #  #  #  #  #  #  #  #  ####   ##         ####  #  #  ####   #    ##");
    logger.LogInformation(" # # #  #      #     #     #    #     #  #        #  #  #  #        #  #  # ##  #  #  #  #  #        #        ####  #  #  #      #    # #");
    logger.LogInformation("  # #   #     ###     ##    ##   ##   #  #        ###    ###        #  #   # #  #  #  #  #   ##   ###         #  #   ##    ##   ###   #  #");
    logger.LogInformation("                                                           #");
    logger.LogInformation("                                                         ##");
    logger.LogInformation("  #           #                       #  #  #  #  ####        #  #                                      #     ##                #");
    logger.LogInformation("  #           #                       ####  #  #     #        #  #               #                      #      #                #");
    logger.LogInformation("  #      ###  ###    ##   # ##        ####  #  #    #         #  #   ##    ###  #### ## #    ##    ###  #  #   #     ##   ###   ###   #  #  # ##   ###");
    logger.LogInformation("  #     #  #  #  #  #  #  ##          #  #  #  #   #          #  #  #  #  #      #   # # #  #  #  #     # #    #    #  #  #  #  #  #  #  #  ##    #  #");
    logger.LogInformation("  #     #  #  #  #  #  #  #           #  #   ##   #           ####  ####   ##    #   # # #  ####  #     ##     #    ####  #  #  #  #  #  #  #     #  #");
    logger.LogInformation("  #     # ##  #  #  #  #  #           #  #   ##   #           ####  #        #   #   # # #  #     #     # #    #    #     #  #  #  #  #  #  #     #  #");
    logger.LogInformation("  ####   # #  ###    ##   #           #  #   ##   ####        #  #   ##   ###     ## #   #   ##    ###  #  #  ###    ##   #  #  ###    ###  #      ###");
    logger.LogInformation("                                                                                                                                                     #");
    logger.LogInformation("                                                                                                                                                   ##");
    logger.LogInformation("   #    ####         ##   ####         ##    ##    ##      #");
    logger.LogInformation("  ##       #        #  #  #           #  #  #  #  #  #    ##");
    logger.LogInformation("   #       #        # ##  #              #  # ##     #    ##");
    logger.LogInformation("   #      #         ## #  ###           #   ## #    #    # #");
    logger.LogInformation("   #      #         #  #     #         #    #  #   #     # #");
    logger.LogInformation("   #     #     #    #  #     #   #    #     #  #  #     ####");
    logger.LogInformation("   #     #     #     ##   ###    #    ####   ##   ####     #");
    logger.LogInformation("");
    logger.LogInformation("");
    logger.LogInformation($"Version {appVersion?.Major}.{appVersion?.Minor}");
    logger.LogInformation("--------------------------------------------------");
    logger.LogInformation("------------>starting up the service<-------------");
    logger.LogInformation("--------------------------------------------------");
    await host.RunAsync();

}
catch (Exception ex)
{
    logger.LogCritical(ex, "There was a problem starting the service");
    return;
}
