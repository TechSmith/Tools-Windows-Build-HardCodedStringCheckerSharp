using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HardCodedStringCheckerSharp
{
   enum Action
   {
      ReportHCS,
      FixHCS
   }
   public class Program
   {
      private static string _strDirectory;// = @"E:\Git\CamtasiaWin";
      private static bool _bCommenting = false;
      private static int _WarningCount = 0;
      static void Main( string[] args )
      {
         if ( args.Count() != 2 )
         {
            Console.WriteLine( $"Usage: <Program> RepoDirectory (Report or Fix)" );
            return;
         }

         _strDirectory = args[0];
         Action eAction = Action.ReportHCS;
         if ( args[1] == "Fix" )
            eAction = Action.FixHCS;

         if ( !Directory.Exists( _strDirectory ) )
         {
            Console.WriteLine( $"Directory \"{_strDirectory}\" doesn't exist.  Failed" );
            return;
         }

         foreach ( var strFile in Directory.EnumerateFiles( _strDirectory, "*.cs", SearchOption.AllDirectories ) )
            MakeFixesOnFile( strFile, eAction );
      }

      private static void MakeFixesOnFile( string strFile, Action eAction )
      {
         string strFilename = Path.GetFileName(strFile);
         if ( strFilename.CompareTo( "AssemblyInfo.cs" ) == 0 )
            return;
         if ( strFilename.CompareTo( "CurrentVersion.cs" ) == 0 )
            return;
         if ( strFilename.CompareTo( "Resources.Designer.cs" ) == 0 )
            return;
         if ( strFilename.CompareTo( "SmokeTest.feature.cs" ) == 0 )//This is "automatically" generated
            return;

         if ( strFile.ToLower().Contains( "packages" ) )
            return;

         if ( strFile.ToLower().Contains( "TemporaryGeneratedFile" ) )
            return;

         if ( strFilename.ToLower().Contains( ".i." ) )
            return;
         if ( strFilename.ToLower().Contains( ".g." ) )
            return;

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
               string strFirstDirectory = FirstDirectory(strFile, _strDirectory);
               Console.WriteLine( $"{_WarningCount}: [{strFirstDirectory}|{strFilename}:{nLine}] HCS: \"{strOriginalLine.Trim()}\"" );
            }
         }

         if ( eAction == Action.FixHCS && bMadeChanges == true )
         {
            strLines = $"using static NeverTranslateNS.NeverTranslateClass;{Environment.NewLine}{strLines}";
            File.WriteAllText( strFile, strLines, encoding );
         }
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
            if ( ch == '/' && i > 0 && strLine[i - 1] == '/' )
               bRestOfLineCommented = true;

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
         string strInsert = $"NeverTranslate( {strSegment} )";
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
