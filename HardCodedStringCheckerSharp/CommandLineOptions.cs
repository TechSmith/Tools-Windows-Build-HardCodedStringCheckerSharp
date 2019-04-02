using System.Collections.Generic;
using System.Linq;

namespace HardCodedStringCheckerSharp
{
   public class CommandLineOptions
   {
      public string RepoDirectory { get; set; } = string.Empty;
      public Action Action { get; set; } = Action.ReportHCS;
      public bool FailOnHCS { get; set; } = false;
      public string ExcludeFile { get; set; } = string.Empty;
      public IEnumerable<string> SpecificFiles { get; set; } = Enumerable.Empty<string>();
   }
}
