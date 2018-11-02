using Microsoft.AspNetCore.Mvc;

namespace ExampleServiceNov2018.Api.Controllers
{
    public class HelloController : Controller
    {
        public string Index()
        {
            return "Hello world";
        }
    }
}