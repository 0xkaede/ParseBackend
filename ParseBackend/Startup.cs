using Newtonsoft.Json;
using ParseBackend.Exceptions.Common;
using ParseBackend.Filters;
using ParseBackend.Services;
using ParseBackend.Utils;
using System.Net;

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
            services.AddSingleton<IUserService, UserService>();
            //services.AddSingleton<IXmppService, XmppService>();

            services.AddHttpContextAccessor();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMongoService mongo)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //mongo.Ping();

            app.UseStatusCodePages(async context =>
            {
                var json = "";
                context.HttpContext.Response.Headers["Content-Type"] = "application/json";

                json = (HttpStatusCode)context.HttpContext.Response.StatusCode switch
                {
                    HttpStatusCode.NotFound => JsonConvert.SerializeObject(new NotFoundException()),
                    _ => json
                };

                Logger.Log($"Cant find \"{context.HttpContext.Request.Path}\"", Utils.LogLevel.Error);
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
