using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HardCodedStringCheckerSharp
{
   public class FileSystem : IFileSystem
   {
      public bool FileExists( string path )
         => File.Exists( path );

      public bool DirectoryExists( string path )
         => Directory.Exists( path );

      public IEnumerable<string> EnumerateFiles( string path, string searchPattern, SearchOption searchOption )
         => Directory.EnumerateFiles( path, searchPattern, searchOption );

      public string[] ReadAllLines( string path )
         => File.ReadAllLines( path );

      public string[] ReadAllLines( string path, Encoding encoding )
         => File.ReadAllLines( path, encoding );

      public void WriteAllText( string path, string contents, Encoding encoding )
         => File.WriteAllText( path, contents, encoding );

      public byte[] GetByteOrderMarker( string path )
      {
         var bytes = new byte[4];

         using ( var fileStream = new FileStream( path, FileMode.Open, FileAccess.Read ) )
         {
            fileStream.Read( bytes, 0, 4 );
         }

         return bytes;
      }
   }
}
