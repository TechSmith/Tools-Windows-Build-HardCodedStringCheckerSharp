using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HardCodedStringCheckerSharp
{
   public interface IFileSystem
   {
      bool FileExists( string path );
      bool DirectoryExists( string path );
      IEnumerable<string> EnumerateFiles( string path, string searchPattern, SearchOption searchOption );

      string[] ReadAllLines( string path );
      string[] ReadAllLines( string path, Encoding encoding );
      void WriteAllText( string path, string contents, Encoding encoding );
      byte[] GetByteOrderMarker( string path );
   }
}
