using System;


namespace RepositoryClient
{
    class ConsoleCommandSupport
    {
        RequestManager _requestManager = null;

        public ConsoleCommandSupport(RequestManager requestManager)
        {
            _requestManager = requestManager;
        }

        public bool ProcessInputArguments(string[] args)
        {
            foreach(string argument in args)
            {
                if(argument.ToLower() == "-help")
                {
                    EmitHelpMessage();
                    break;
                }
                else
                {
                    string[] argVal = argument.Split('=');

                    if (argVal.Length != 2)
                    {
                        Console.WriteLine("ERROR: Invalid argument - " + argument);
                        return false;
                    }

                    string arg = argVal[0];
                    string val = argVal[1];

                    if (arg.ToLower() == "-getp")
                    {
                        string[] packageInfo = val.Split(',');

                        if (packageInfo.Length != 3)
                        {
                            Console.WriteLine("ERROR: Invalid 'getp' argument value - " + val);
                            return false;
                        }

                        if (!_requestManager.GetPackage(packageInfo[0], packageInfo[1], packageInfo[2]))
                        {
                            Console.WriteLine("ERROR: Specified respository is not local - " + packageInfo[0]);
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Unknown argument - " + arg);
                        return false;
                    }
                }
            }

            return true;
        }

        private void EmitHelpMessage()
        {
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("   -getp=<repo>,<package>,<version>          Get specified package");
            Console.WriteLine("   -help                                     Emits this message");
            Console.WriteLine("");
        }
    }
}
