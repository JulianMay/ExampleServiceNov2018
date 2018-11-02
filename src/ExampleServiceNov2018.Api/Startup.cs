using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleServiceNov2018.Application;
using ExampleServiceNov2018.Infrastructure;
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
        private const string _connectionString = 
            "Server=.\\sqlexpress; Database=ExampleServiceNov2018; Trusted_Connection=True;";
        
        private static MsSqlStreamStoreSettings _msSqlStreamStoreSettings 
            = new MsSqlStreamStoreSettings(_connectionString);
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            //Register infrastructure
            SetupStreamStore(services);
            services.AddScoped<ITodoLists, TodoListRepository>();;
            services.AddScoped<TodoCommandHandler>();
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

        private void SetupStreamStore(IServiceCollection services)
        {
            var streamStore = new MsSqlStreamStore(_msSqlStreamStoreSettings);
            services.AddSingleton<IStreamStore>((x)=>streamStore);
            var schema = streamStore.CheckSchema().GetAwaiter().GetResult();
            if (!schema.IsMatch())
                streamStore.CreateSchema();

        }
    }
}
