using Amazon;
using ImageDis.AspNetCore;
using ImageDis.Local;
using ImageDis.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageDis.Examples.AspNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            UseImageDis(app, env);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// 
        ///   The example below shows how you can use the Local storage provider for local development
        ///   and the S3 storage provider for deployed versions of your app.The S3 keys below are fake
        ///   so please try them out using your own credentials.
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        private void UseImageDis(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseImageDis(new ImageDisOptions(
                    new LocalStorageProvider(
                        filePath: "wwwroot/images",
                        redirectPath: "/images"
                    )
                ));
            }
            else
            {
                app.UseImageDis(new ImageDisOptions(
                    new S3StorageProvider(
                        awsAccessKeyId: "UR8AIRAJRKABWXGIJFUA",
                        awsSecretAccessKey: "nthu0aa51g0fWSVhj74/w/gYhHeAaupAQT37hast",
                        region: RegionEndpoint.USWest1,
                        bucketName: "the-bucket-name",
                        imageRedirect: "https://s3-us-west-1.amazonaws.com/the-bucket-name/"
                    )
                ));
            }
        }
    }
}
