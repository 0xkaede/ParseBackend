using ParseBackend;
using ParseBackend.Utils;

namespace KaedeBackend.Exceptions
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

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
                });
    }
}