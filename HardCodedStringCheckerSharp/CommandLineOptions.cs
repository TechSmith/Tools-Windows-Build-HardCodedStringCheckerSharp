using System.Collections.Generic;
using System.Linq;

namespace HardCodedStringCheckerSharp
{
   public class CommandLineOptions
   {
      public CommandLineOptions()
      {
         // Default values
         RepoDirectory = string.Empty;
         Action = Action.ReportHCS;
         FailOnHCS = false;
         ExcludeFile = string.Empty;
         SpecificFiles = Enumerable.Empty<string>();
      }

      public string RepoDirectory { get; set; }
      public Action Action { get; set; }
      public bool FailOnHCS { get; set; }
      public string ExcludeFile { get; set; }
      public IEnumerable<string> SpecificFiles { get; set; }
   }
}
