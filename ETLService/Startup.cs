using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ETLCommon;

using Microsoft.AspNetCore.StaticFiles;

namespace ETLService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;            
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        /// <summary>
        /// Разрешение отдавать файлы указанных типов
        /// </summary>
        private StaticFileOptions GetStaticFileConfiguration()
        {
            var provider = new FileExtensionContentTypeProvider { Mappings = { [".tmp"] = "text/html" } };
            return new StaticFileOptions { ContentTypeProvider = provider };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopped.Register(OnStopped);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Статический контент
            app.UseDefaultFiles();
            app.UseStaticFiles(GetStaticFileConfiguration());
            
            app.UseMvc();
        }

        #region Lifetime events

        private void OnStopped()
        {
            // Корректное уничтожение объекта
            Program.Manager?.Dispose();
            Logger.CloseLogFile();
        }

        #endregion Lifetime events
    }
}
