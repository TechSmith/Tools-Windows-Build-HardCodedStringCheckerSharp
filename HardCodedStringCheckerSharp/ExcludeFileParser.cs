using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HardCodedStringCheckerSharp
{
   public class ExcludeFileParser : IExcludeFileParser
   {
      private readonly IFileSystem _fileSystem;

      public ExcludeFileParser( IFileSystem fileSystem )
      {
         _fileSystem = fileSystem;
      }

      public List<string> ParseExcludeFile( string excludeFile )
      {
         var exclusions = new List<string>();

         string[] lines = _fileSystem.ReadAllLines( excludeFile );
         foreach( string line in lines )
         {
            // Strip any comments
            string lineMinusComments = line;
            int commentStart = line.IndexOf( "//" );
            if ( commentStart >= 0 )
            {
               lineMinusComments = line.Substring( 0, commentStart );
            }

            // If there's anything left after trimming whitespace, add it as an exclusion
            string trimmedLine = lineMinusComments.Trim();
            if ( !string.IsNullOrWhiteSpace( trimmedLine ) )
            {
               exclusions.Add( trimmedLine );
            }
         }

         return exclusions;
      }

      public List<string> PrependRepoRootDir( string repoRootDir, List<string> exclusions )
      {
         // Note: The Trim of the backslash is necessary because a leading backslash
         //       on the second argument causes Path.Combine to think the second path
         //       is a root path, and it doesn't actually combine the strings. -dro
         return exclusions.Select( ex => Path.Combine( repoRootDir, ex.Trim('\\') ) ).ToList();
      }
   }
}
