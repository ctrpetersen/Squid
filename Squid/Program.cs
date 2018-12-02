using System;
using Newtonsoft;

namespace Squid
{
    internal class Program
    {
        private static void Main(string[] args) => new Squid().StartAsync().GetAwaiter().GetResult();
    }
}
