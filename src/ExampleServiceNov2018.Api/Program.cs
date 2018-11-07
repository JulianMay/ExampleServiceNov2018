using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ExampleServiceNov2018.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Assertions:
            ReadService.CommitBatchPolicy.AssertShouldCommit();
            
            
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}