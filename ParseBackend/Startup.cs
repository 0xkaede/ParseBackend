using Newtonsoft.Json;
using ParseBackend.Exceptions.Common;
using ParseBackend.Filters;
using ParseBackend.Services;
using ParseBackend.Utils;
using System.Net;
using static ParseBackend.Global;

namespace ParseBackend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public string Uptime { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.Filters.Add(new BaseExceptionFilterAttribute())).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddSingleton<IFileProviderService, FileProviderService>();
            services.AddSingleton<IMongoService, MongoService>();
            services.AddSingleton<IFriendService, FriendService>();
            services.AddSingleton<ICahceService, CahceService>();
            //services.AddSingleton<IWebSocketService, XmppService>();
            //services.AddSingleton<IXmppService, XmppService>();

            services.AddHttpContextAccessor();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMongoService mongo, IFileProviderService fp, ICahceService cs)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            cs.Loop();

            mongo.Ping();

            GlobalMongoService = mongo; //very skunky cba stuff
            GlobalXmppServer = new Xmpp.XmppServer();
            GlobalXmppServer.StartServer();

            fp.Ping();

            app.UseStatusCodePages(async context =>
            {
                var json = "";
                context.HttpContext.Response.Headers["Content-Type"] = "application/json";

                json = (HttpStatusCode)context.HttpContext.Response.StatusCode switch
                {
                    HttpStatusCode.NotFound => JsonConvert.SerializeObject(new NotFoundException()),
                    _ => json
                };

                Logger.Log($"Missing [{context.HttpContext.Request.Method}] \"{context.HttpContext.Request.Path}\"", Utils.LogLevel.Error);
                await context.HttpContext.Response.WriteAsync(json);
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
