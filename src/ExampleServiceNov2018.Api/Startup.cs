using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleServiceNov2018.Application;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;

namespace ExampleServiceNov2018.Api
{
    public class Startup
    {
        private const string _connectionString = "Server=localhosts\\sqlexpress;Database=ExampleServiceNov2018;";
        
        private static MsSqlStreamStoreSettings _msSqlStreamStoreSettings 
            = new MsSqlStreamStoreSettings(_connectionString);
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            //Register infrastructure
            services.AddScoped<IStreamStore>((x)=>new MsSqlStreamStore(_msSqlStreamStoreSettings));
            //Register mediator
            services.AddMediatR();
            services.AddMediatR(typeof(ApplicationDummyClass).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
