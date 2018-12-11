using System;


namespace RepositoryClient
{
    class Program
    {
        private static RequestManager _requestManager = new RequestManager();

        static int Main(string[] args)
        {
            Console.Title = "File Repository Client";

            try
            {
                if (args.Length != 0) // handle console
                {
                    // try to connect
                    _requestManager.Connect(32123);

                    // parse input arguments
                    ConsoleCommandSupport consoleSupport = new ConsoleCommandSupport(_requestManager);
                    if (!consoleSupport.ProcessInputArguments(args))
                    {
                        Console.WriteLine("ERROR: Cannot parse input arguments!");
                        Console.ReadKey();
                        return 1;
                    }
                }
                else // run intearctive command loop
                {
                    // try to connect
                    _requestManager.Connect(32123);

                    // run console
                    InteractiveConsole interactiveConsole = new InteractiveConsole(_requestManager);
                    interactiveConsole.run();
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine("SOCKET EXCEPTION: " + ex.Message);
                Console.ReadKey();
                return 1;
            }

            // no error
            return 0;
        }
    }
}
