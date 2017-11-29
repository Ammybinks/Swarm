using System;

namespace Swarm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Swarm game = new Swarm())
            {
                game.Run();
            }
        }
    }
}

