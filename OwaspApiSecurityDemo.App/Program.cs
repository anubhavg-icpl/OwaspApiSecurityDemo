using System;
using Microsoft.Owin.Hosting;

namespace OwaspApiSecurityDemo.App
{
    internal static class Program
    {
        private static void Main()
        {
            const string baseAddress = "http://localhost:5050/";

            using (WebApp.Start<Startup>(baseAddress))
            {
                Console.WriteLine("OWASP API Security demo is running.");
                Console.WriteLine("Base URL: " + baseAddress);
                Console.WriteLine("Press ENTER to stop.");
                Console.ReadLine();
            }
        }
    }
}
