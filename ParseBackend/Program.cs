using ParseBackend;
using ParseBackend.Utils;

namespace KaedeBackend.Exceptions
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            await Task.WhenAny(
                host.RunAsync()
            );
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:4010");
                    webBuilder.UseStartup<Startup>();

                    Logger.Log($"Parse Backend, A universal fortnite backend!");
                    Logger.Log($"Backend Created by 0xkaede");
                    Logger.Log($"Backend started on port 4010");
                });
    }
}