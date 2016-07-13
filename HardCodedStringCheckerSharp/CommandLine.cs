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

      public CommandLine()
      {

      }

      public bool Parse( string[] args )
      {
         if ( args.Count() < 2 )
         {
            Console.WriteLine( "Usage: <Program> RepoDirectory (Report or Fix) (--FailOnHCS optional)" );
            return false;
         }

         ReposityPath = args[0];

         //------------------------------
         if ( args[1] == "Fix" )
            Action = Action.FixHCS;

         //------------------------------

         if ( args.Count() >= 3 && args[2] == "--FailOnHCS" )
            FailBuildWithHSC = true;

         return true;
      }
   }
}
