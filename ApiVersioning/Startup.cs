using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiVersioning
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Api Version proposer information
        /// </summary>
        private IApiVersionDescriptionProvider provider;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddApiVersioning(option =>
            {
                option.DefaultApiVersion = new ApiVersion(1, 0);
                option.ReportApiVersions = true;
                //option.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version"); ==> this is for Header that defined by yourself
                option.ApiVersionReader = new MediaTypeApiVersionReader("vnd.thuongkmt.v");
            });
            /* services.AddSwaggerGen(c =>
             {
                 c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiVersioning", Version = "v1" });
             });*/

            services.AddVersionedApiExplorer(option =>
            {
                option.GroupNameFormat = "Interface:'v'VVV";
                option.AssumeDefaultVersionWhenUnspecified = true;
            });

            this.provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
            services.AddSwaggerGen(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName,
                            new Microsoft.OpenApi.Models.OpenApiInfo()
                            {
                                Title = $"Interface v{description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "To switch versions, please click version switch in the upper right corner"
                            }
                    );
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiVersioning v1"));

            app.UseSwaggerUI(c =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
            }

            app.UseHttpsRedirection();

            app.UseApiVersioning();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
