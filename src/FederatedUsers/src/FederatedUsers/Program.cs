using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Logging;

namespace FederatedUsers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IdentityModelEventSource.Logger.LogLevel = System.Diagnostics.Tracing.EventLevel.LogAlways;
            var wilsonTextLogger = new TextWriterEventListener("FederatedUsers2.log");
            wilsonTextLogger.EnableEvents(IdentityModelEventSource.Logger, System.Diagnostics.Tracing.EventLevel.Informational);

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
