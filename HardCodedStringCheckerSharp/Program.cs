using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace HardCodedStringCheckerSharp
{
   
   public class Program
   {
      private static bool _bCommenting = false;
      private static int _WarningCount = 0;
      static int Main( string[] args )
      {
         CommandLine cmdLine = new CommandLine();
         if( !cmdLine.Parse( args ) )
         {
            Environment.ExitCode = 1;
            return 1;
         }

         if ( !Directory.Exists( cmdLine.ReposityPath ) )
         {
            Console.WriteLine( String.Format("Directory \"{0}\" doesn't exist.  Failed", cmdLine.ReposityPath ) );
            Environment.ExitCode = 1;
            return 1;
         }

         bool bChanges = false;
         foreach ( var strFile in Directory.EnumerateFiles( cmdLine.ReposityPath, "*.cs", SearchOption.AllDirectories ) )
            bChanges |= MakeFixesOnFile( strFile, cmdLine );

         if ( cmdLine.FailBuildWithHSC && bChanges )
         {
            Environment.ExitCode = 1;
            return 1;
         }

         return 0;
      }

      private static bool MakeFixesOnFile( string strFile, CommandLine cmdLine )
      {
         string strFilename = Path.GetFileName(strFile);
         if ( strFilename.CompareTo( "AssemblyInfo.cs" ) == 0 )
            return false;
         if ( strFilename.CompareTo( "CurrentVersion.cs" ) == 0 )
            return false;
         if ( strFilename.CompareTo( "Resources.Designer.cs" ) == 0 )
            return false;
         if ( strFilename.CompareTo( "SmokeTest.feature.cs" ) == 0 )//This is "automatically" generated
            return false;

         if ( strFile.ToLower().Contains( "packages" ) )
            return false;

         if ( strFile.ToLower().Contains( "TemporaryGeneratedFile" ) )
            return false;

         if ( strFilename.ToLower().Contains( ".i." ) )
            return false;
         if ( strFilename.ToLower().Contains( ".g." ) )
            return false;

         var cultureInfo = new CultureInfo("en-US");
         if ( cmdLine.ExcludePaths.Any( exclude => cultureInfo.CompareInfo.IndexOf( strFile, exclude, CompareOptions.IgnoreCase ) >= 0 ) )
            return false;

         _bCommenting = false;

         bool bMadeChanges = false;
         int nLine = 0;
         var encoding = GetEncoding(strFile);

         string strLines = String.Empty;
         foreach ( string strOriginalLine in File.ReadAllLines( strFile, encoding ) )
         {
            nLine++;

            string strLine = strOriginalLine;
            bool bWarningLine = FixUpLine( ref strLine );
            strLines += strLine + Environment.NewLine;

            if ( bWarningLine )
            {
               bMadeChanges = true;
               _WarningCount++;
               string strFirstDirectory = FirstDirectory(strFile, cmdLine.ReposityPath);
               Console.WriteLine( String.Format( "{0}: [{1}|{2}:{3}] HCS \"{4}\"", _WarningCount, strFirstDirectory, strFilename, nLine, strOriginalLine.Trim() ) );
            }
         }

         if ( cmdLine.Action == Action.FixHCS && bMadeChanges == true )
         {
            strLines = String.Format( "using static NeverTranslateNS.NeverTranslateClass;{0}{1}", Environment.NewLine, strLines );
            File.WriteAllText( strFile, strLines, encoding );
         }

         return bMadeChanges;
      }

      enum StringType
      {
         None,
         NormalString,
         VerbaitimString,
         StringInterpolation
      }
      private static bool FixUpLine( ref string strLine )
      {
         if ( HasIgnoreableKeyword( strLine ) )
            return false;

         //Does the line have a NeverTranslate on it?
         if ( strLine.IndexOf( "NeverTranslate" ) >= 0 )
            return false;

         StringType eType = StringType.None;
         int nStringStart = -1;
         bool bChanges = false;
         bool bRestOfLineCommented = false;
         for ( int i = 0; i < strLine.Length; i++ )
         {
            char ch = strLine[i];
            if ( ch == '*' && i > 0 && strLine[i - 1] == '/' )
            {
               _bCommenting = true;
            }
            if ( ch == '/' && i > 0 && strLine[i - 1] == '*' )
            {
               _bCommenting = false;
            }
            if ( ch == '/' && i > 0 && strLine[i - 1] == '/' && eType == StringType.None )
            {
               bRestOfLineCommented = true;
            }
            if( ch == '{' && eType == StringType.StringInterpolation )
            {
               int nEndBrace = strLine.IndexOf('}', i+1);
               if( nEndBrace != -1 )
               {
                  int nStart = i;
                  int nLength = nEndBrace-i+1;
                  string sub = strLine.Substring(nStart, nLength);
                  bool bFix = FixUpLine(ref sub);
                  if( bFix )
                  {
                     strLine = strLine.Remove( nStart, nLength );
                     strLine = strLine.Insert( nStart, sub );
                     i = nStart + sub.Length;
                     continue;
                  }
               }
            }

            if ( bRestOfLineCommented )
               continue;

            if ( ch == '"' && !_bCommenting )
            {
               if ( eType == StringType.None )
               {
                  if ( i > 0 && strLine[i - 1] == '@' )
                  {
                     eType = StringType.VerbaitimString;
                     nStringStart = i - 1;
                  }
                  else if ( i > 0 && strLine[i - 1] == '$' )
                  {
                     eType = StringType.StringInterpolation;
                     nStringStart = i - 1;
                  }
                  else
                  {
                     eType = StringType.NormalString;
                     nStringStart = i;
                  }
               }
               else
               {
                  if ( eType == StringType.VerbaitimString && ( i + 1 ) < strLine.Length && strLine[i + 1] != '"' )
                  {
                     eType = StringType.None;
                     if ( DoReplacement( ref strLine, ref i, nStringStart, eType ) == false )
                        continue;
                     bChanges = true;
                  }
                  else if ( ( eType == StringType.NormalString || eType == StringType.StringInterpolation ) &&
                     i > 0 && strLine[i - 1] != '\\' )
                  {
                     eType = StringType.None;
                     if ( DoReplacement( ref strLine, ref i, nStringStart, eType ) == false )
                        continue;
                     bChanges = true;
                  }
               }
            }
         }

         return bChanges;
      }

      private static bool DoReplacement( ref string strLine, ref int i, int nStringStart, StringType eType )
      {
         int nLength = i-nStringStart+1;
         string strSegment = strLine.Substring(nStringStart, nLength);
         if ( nLength == 2 || ( nLength == 3 && ( eType == StringType.StringInterpolation || eType == StringType.StringInterpolation ) ) )
            return false;
         strLine = strLine.Remove( nStringStart, nLength );
         string strInsert = String.Format( "NeverTranslate( {0} )", strSegment );
         strLine = strLine.Insert( nStringStart, strInsert );
         i = nStringStart + strInsert.Length;
         return true;
      }

      public static Encoding GetEncoding( string strFile )
      {
         // Read the BOM
         var bom = new byte[4];
         using ( var file = new FileStream( strFile, FileMode.Open, FileAccess.Read ) )
         {
            file.Read( bom, 0, 4 );
         }

         // Analyze the BOM
         if ( bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76 ) return Encoding.UTF7;
         if ( bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf ) return Encoding.UTF8;
         if ( bom[0] == 0xff && bom[1] == 0xfe ) return Encoding.Unicode; //UTF-16LE
         if ( bom[0] == 0xfe && bom[1] == 0xff ) return Encoding.BigEndianUnicode; //UTF-16BE
         if ( bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff ) return Encoding.UTF32;
         return Encoding.GetEncoding( "Windows-1252" );
      }

      private static string FirstDirectory( string strFile, string strDirectory )
      {
         Debug.Assert( strFile.StartsWith( strDirectory ) );

         string str = strFile.Remove(0, strDirectory.Length).TrimStart('\\');
         int nSlash = str.IndexOf('\\');
         Debug.Assert( nSlash > 0 );
         string strFirstDirectory = String.Empty;
         if ( nSlash > 0 )
            strFirstDirectory = str.Substring( 0, nSlash );

         return strFirstDirectory;
      }

      private static bool HasIgnoreableKeyword( string strLine )
      {
         string[] arrKeywords = new string[]
         {
            "const",
            "Guid",
            "TemplatePart",
            "DllImport",
            "assembly:",
            "ConstructorArgument",
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
            "Then"
         };

         foreach ( string s in arrKeywords )
         {
            if ( strLine.Contains( s ) )
               return true;
         }
         return false;
      }
   }
}
