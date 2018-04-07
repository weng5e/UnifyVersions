using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UnifyVersions
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length < 1 || !Directory.Exists(args[0]))
            {
                Console.WriteLine("Root directory is expected.");
                return;
            }

            string rootDirectory = args[0];

            UnifyVersionsWorker.UnifyVersion(rootDirectory);
        }


    }
}
