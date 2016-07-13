using System;
using System.Collections.Generic;
using System.Linq;

namespace HardCodedStringCheckerSharp
{
   public enum Action
   {
      ReportHCS,
      FixHCS
   }
   public class CommandLine
   {
      public string ReposityPath { get; private set; } = String.Empty;
      public Action Action { get; private set; } = Action.ReportHCS;
      public bool FailBuildWithHSC { get; private set; } = false;
      public List<String> ExcludePaths { get; private set; } = new List<String>();

      public CommandLine()
      {

      }

      public bool Parse( string[] args )
      {
         int nNumCmdLineArgs = args.Count();

         if ( nNumCmdLineArgs < 2 )
         {
            Console.WriteLine( "Usage: <Program> RepoDirectory (Report or Fix) (--FailOnHCS optional) (--Exclude PartialPath optional)" );
            return false;
         }

         ReposityPath = args[0];

         for ( int nIndex = 1; nIndex < nNumCmdLineArgs; nIndex++ )
         {
            if ( args[nIndex] == "Fix" )
            {
               Action = Action.FixHCS;
               continue;
            }

            if ( args[nIndex] == "--FailOnHCS" )
            {
               FailBuildWithHSC = true;
               continue;
            }

            if( args[nIndex] == "--Exclude" )
            {
               nIndex++;
               ExcludePaths.Add( args[nIndex] );
               continue;
            }
         }

         return true;
      }
   }
}
