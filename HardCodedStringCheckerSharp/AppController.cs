using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HardCodedStringCheckerSharp
{
   public class AppController
   {
      private readonly IFileSystem _fileSystem;
      private readonly IConsole _consoleAdapter;

      private string _directory;
      private bool _commenting;
      private int _warningCount;

      public AppController( IFileSystem fileSystem, IConsole consoleAdapter )
      {
         _fileSystem = fileSystem;
         _consoleAdapter = consoleAdapter;
      }

      public int Main( string[] args )
      {
         int argsCount = args.Length;
         if ( argsCount != 2 && argsCount != 3 )
         {
            _consoleAdapter.WriteLine( "Usage: <Program> RepoDirectory (Report or Fix) (--FailOnHCS optional)" );
            return 1;
         }

         _directory = args[0];
         Action action = Action.ReportHCS;
         if ( args[1] == "Fix" )
            action = Action.FixHCS;

         bool failOnErrors = argsCount == 3 && args[2] == "--FailOnHCS";

         if ( !_fileSystem.DirectoryExists( _directory ) )
         {
            _consoleAdapter.WriteLine( $"Directory \"{_directory}\" doesn't exist.  Failed" );
            return 1;
         }

         bool hasChanges = false;
         foreach ( var file in _fileSystem.EnumerateFiles( _directory, "*.cs", SearchOption.AllDirectories ) )
            hasChanges |= MakeFixesOnFile( file, action );

         if ( failOnErrors && hasChanges )
         {
            return 1;
         }

         return 0;
      }

      private bool MakeFixesOnFile( string file, Action eAction )
      {
         string fileName = Path.GetFileName( file );
         if ( fileName.CompareTo( "AssemblyInfo.cs" ) == 0 )
            return false;
         if ( fileName.CompareTo( "CurrentVersion.cs" ) == 0 )
            return false;
         if ( fileName.Contains( ".Designer.cs" ) )
            return false;
         if ( fileName.CompareTo( "SmokeTest.feature.cs" ) == 0 )//This is "automatically" generated
            return false;

         if ( file.ToLower().Contains( "packages" ) )
            return false;

         if ( file.ToLower().Contains( "TemporaryGeneratedFile" ) )
            return false;

         if ( fileName.ToLower().Contains( ".i." ) )
            return false;
         if ( fileName.ToLower().Contains( ".g." ) )
            return false;

         _commenting = false;

         bool madeChanges = false;
         int lineNumber = 0;
         var encoding = GetEncoding( file );

         string lines = string.Empty;
         foreach ( string originalLine in _fileSystem.ReadAllLines( file, encoding ) )
         {
            lineNumber++;

            string line = originalLine;
            bool isWarningLine = FixUpLine( ref line );
            lines += line + Environment.NewLine;

            if ( isWarningLine )
            {
               madeChanges = true;
               _warningCount++;
               string firstDirectory = FirstDirectory( file, _directory );
               _consoleAdapter.WriteLine( $"{_warningCount}: [{firstDirectory}|{fileName}:{lineNumber}] HCS \"{originalLine.Trim()}\"" );
            }
         }

         if ( eAction == Action.FixHCS && madeChanges )
         {
            lines = $"using static NeverTranslateNS.NeverTranslateClass;{Environment.NewLine}{lines}";
            _fileSystem.WriteAllText( file, lines, encoding );
         }

         return madeChanges;
      }

      private bool FixUpLine( ref string line )
      {
         if ( HasIgnoreableKeyword( line ) )
            return false;

         //Does the line have a NeverTranslate on it?
         if ( line.IndexOf( "NeverTranslate" ) >= 0 )
            return false;

         StringType stringType = StringType.None;
         int stringStart = -1;
         bool hasChanges = false;
         bool isLineCommented = false;
         for ( int i = 0; i < line.Length; i++ )
         {
            char ch = line[i];
            if ( ch == '*' && i > 0 && line[i - 1] == '/' )
            {
               _commenting = true;
            }
            if ( ch == '/' && i > 0 && line[i - 1] == '*' )
            {
               _commenting = false;
            }
            if ( ch == '/' && i > 0 && line[i - 1] == '/' && stringType == StringType.None )
            {
               isLineCommented = true;
            }
            if ( ch == '{' && stringType == StringType.StringInterpolation )
            {
               int endBracePosition = line.IndexOf( '}', i + 1 );
               if ( endBracePosition != -1 )
               {
                  int start = i;
                  int length = endBracePosition - i + 1;
                  string sub = line.Substring( start, length );
                  bool shouldFix = FixUpLine( ref sub );
                  if ( shouldFix )
                  {
                     line = line.Remove( start, length );
                     line = line.Insert( start, sub );
                     i = start + sub.Length;
                     continue;
                  }
               }
            }

            if ( isLineCommented )
               continue;

            if ( ch == '"' && !_commenting )
            {
               if ( stringType == StringType.None )
               {
                  if ( i > 0 && line[i - 1] == '@' )
                  {
                     stringType = StringType.VerbatimString;
                     stringStart = i - 1;
                  }
                  else if ( i > 0 && line[i - 1] == '$' )
                  {
                     stringType = StringType.StringInterpolation;
                     stringStart = i - 1;
                  }
                  else
                  {
                     stringType = StringType.NormalString;
                     stringStart = i;
                  }
               }
               else
               {
                  if ( stringType == StringType.VerbatimString && ( i + 1 ) < line.Length && line[i + 1] != '"' )
                  {
                     stringType = StringType.None;
                     if ( DoReplacement( ref line, ref i, stringStart, stringType ) == false )
                        continue;
                     hasChanges = true;
                  }
                  else if ( ( stringType == StringType.NormalString || stringType == StringType.StringInterpolation ) &&
                     i > 0 && line[i - 1] != '\\' )
                  {
                     stringType = StringType.None;
                     if ( DoReplacement( ref line, ref i, stringStart, stringType ) == false )
                        continue;
                     hasChanges = true;
                  }
               }
            }
         }

         return hasChanges;
      }

      private static bool DoReplacement( ref string line, ref int i, int stringStart, StringType stringType )
      {
         int length = i - stringStart + 1;
         string segment = line.Substring( stringStart, length );
         if ( length == 2 || ( length == 3 && ( stringType == StringType.StringInterpolation || stringType == StringType.StringInterpolation ) ) )
            return false;
         line = line.Remove( stringStart, length );
         string insertString = $"NeverTranslate( {segment} )";
         line = line.Insert( stringStart, insertString );
         i = stringStart + insertString.Length;
         return true;
      }

      public Encoding GetEncoding( string filePath )
      {
         var bom = _fileSystem.GetByteOrderMarker( filePath );

         // Analyze the BOM
         if ( bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76 )
            return Encoding.UTF7;
         if ( bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf )
            return Encoding.UTF8;
         if ( bom[0] == 0xff && bom[1] == 0xfe )
            return Encoding.Unicode; //UTF-16LE
         if ( bom[0] == 0xfe && bom[1] == 0xff )
            return Encoding.BigEndianUnicode; //UTF-16BE
         if ( bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff )
            return Encoding.UTF32;
         return Encoding.GetEncoding( "Windows-1252" );
      }

      private static string FirstDirectory( string file, string directory )
      {
         Debug.Assert( file.StartsWith( directory ) );

         string str = file.Remove( 0, directory.Length ).TrimStart( '\\' );
         int slashPosition = str.IndexOf( '\\' );
         Debug.Assert( slashPosition > 0 );
         string firstDirectory = string.Empty;
         if ( slashPosition > 0 )
            firstDirectory = str.Substring( 0, slashPosition );

         return firstDirectory;
      }

      private static bool HasIgnoreableKeyword( string line )
      {
         string[] keywords =
         {
            "const",
            "Guid",
            "TemplatePart",
            "DllImport",
            "assembly:",
            "ConstructorArgument",
            "Obsolete",
            "System.CodeDom.Compiler.GeneratedCodeAttribute",
            "SuppressMessage",
            "DebuggerDisplay",
            "Description",
            "Category",
            "Conditional",
            "DefaultProperty",
            "ContentProperty",
            "Given",
            "When",
            "Then",
            "XmlAttribute",
            "XmlRoot",
            "XmlElement"
         };

         return keywords.Any( line.Contains );
      }
   }
}
