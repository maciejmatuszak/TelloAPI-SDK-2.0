using System;

namespace TelloUdpSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            SimUdpTelloControl telloControl = new SimUdpTelloControl(8889);
            var task = telloControl.StartListener();
            task.Wait();
            Console.WriteLine("Hello World!");
        }
    }
}