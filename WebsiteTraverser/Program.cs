using System;
using System.Threading.Tasks;

namespace WebsiteTraverser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var websiteTraverser = new Traverser("https://tretton37.com");
            await websiteTraverser.Run();

            Console.ReadLine();
        }
    }
}
