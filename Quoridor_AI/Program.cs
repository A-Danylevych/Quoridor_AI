using System;
using Quoridor_AI.View;

namespace Quoridor_AI
{
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new Controller.Controller();
            var console = new ConsoleView(controller);
            while (true)
            {
                console.Run();
            }

        }
    }
}
