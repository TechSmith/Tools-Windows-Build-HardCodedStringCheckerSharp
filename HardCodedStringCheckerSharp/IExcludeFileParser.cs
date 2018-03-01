using System.Collections.Generic;

namespace HardCodedStringCheckerSharp
{
   public interface IExcludeFileParser
   {
      List<string> ParseExcludeFile( string excludeFile );
      List<string> PrependRepoRootDir( string repoRootDir, List<string> exclusions );
   }
}
