using System;
using System.IO;
using System.Linq;

namespace HardCodedStringCheckerSharp
{
   class CommandLineParser : ICommandLineParser
   {
      public CommandLineOptions ParseCommandLine( string[] args )
      {
         // Expecting between 2 and 5 arguments
         if ( args.Length < 2 || args.Length > 5 )
         {
            return null;
         }

         var opts = new CommandLineOptions();

         // The first argument is assumed to be the repo directory
         opts.RepoDirectory = args[0];

         // The second argument must be "Fix" or "Report"
         string secondArg = args[1];
         if ( secondArg == "Fix" )
         {
            opts.Action = Action.FixHCS;
         }
         else if ( secondArg == "Report" )
         {
            opts.Action = Action.ReportHCS;
         }
         else
         {
            return null;
         }

         if ( args.Length >= 3 )
         {
            // Start at third arg (index 2) because we handled first and second args above
            for ( int i = 2; i < args.Length; i++ )
            {
               if ( args[i] == "--FailOnHCS" )
               {
                  opts.FailOnHCS = true;
               }
               else if ( args[i] == "--Exclude" )
               {
                  // The exclude file is the next argument
                  i = i + 1;
                  if ( i >= args.Length )
                  {
                     return null;  // not enough arguments
                  }
                  opts.ExcludeFile = args[i];
               }
               else if ( args[i] == "--Files" )
               {
                  i++;
                  if ( i >= args.Length )
                  {
                     return null;
                  }
                  
                  var files = args[i].Split( new[] { "," }, StringSplitOptions.RemoveEmptyEntries );
                  opts.SpecificFiles = files.Where( f => Path.GetExtension( f ).ToLower() == ".cs" );
               }
               else
               {
                  // Invalid argument encountered
                  return null;
               }
            }
         }

         return opts;
      }
   }
}
