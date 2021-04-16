using System;

namespace WebsiteTraverser
{
    class Program
    {
        static void Main(string[] args)
        {
            var websiteTraverser = new Traverser("https://tretton37.com");
            websiteTraverser.Run();

            Console.ReadLine();
        }
    }
}
