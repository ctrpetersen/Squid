using System;
using Newtonsoft;

namespace Squid
{
    class Program
    {
        private static void Main(string[] args) => new Squid().StartAsync().GetAwaiter().GetResult();
    }
}
