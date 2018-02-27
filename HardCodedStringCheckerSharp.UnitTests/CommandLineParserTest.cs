using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HardCodedStringCheckerSharp.UnitTests
{
   [TestClass]
   public class CommandLineParserTest
   {
      [TestMethod]
      public void ParseCommandLine_ZeroArgs_ReturnsNull()
      {
         var parser = new CommandLineParser();

         string[] args = new string[0];
         var opts = parser.ParseCommandLine( args );

         opts.Should().BeNull();
      }

      [TestMethod]
      public void ParseCommandLine_OneArg_ReturnsNull()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot" };
         var opts = parser.ParseCommandLine( args );

         opts.Should().BeNull();
      }

      [TestMethod]
      public void ParseCommandLine_SecondArgIsInvalid_ReturnsNull()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot", "Invalid" };
         var opts = parser.ParseCommandLine( args );

         opts.Should().BeNull();
      }

      [TestMethod]
      public void ParseCommandLine_RepoDirectorySpecified_RepoDirectoryIsSetCorrectly()
      {
         var parser = new CommandLineParser();

         const string repoRoot = @"C:\RepoRoot";
         string[] args = { repoRoot, "Report" };
         var opts = parser.ParseCommandLine( args );

         opts.RepoDirectory.Should().Be( repoRoot );
      }

      [TestMethod]
      public void ParseCommandLine_SecondArgIsFix_ActionIsFixHCS()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot", "Fix" };
         var opts = parser.ParseCommandLine( args );

         opts.Action.Should().Be( Action.FixHCS );
      }

      [TestMethod]
      public void ParseCommandLine_SecondArgIsReport_ActionIsReportHCS()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot", "Report" };
         var opts = parser.ParseCommandLine( args );

         opts.Action.Should().Be( Action.ReportHCS );
      }

      [TestMethod]
      public void ParseCommandLine_FailOnHCSNotSpecified_SetsFailOnHCSToFalse()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot", "Report" };
         var opts = parser.ParseCommandLine( args );

         opts.FailOnHCS.Should().BeFalse();
      }

      [TestMethod]
      public void ParseCommandLine_ThirdArgIsInvalid_ReturnsNull()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot", "Report", "Invalid" };
         var opts = parser.ParseCommandLine( args );

         opts.Should().BeNull();
      }

      [TestMethod]
      public void ParseCommandLine_ThirdArgIsFailOnHCS_SetsFailOnHCSToTrue()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot", "Report", "--FailOnHCS" };
         var opts = parser.ParseCommandLine( args );

         opts.FailOnHCS.Should().BeTrue();
      }

      [TestMethod]
      public void ParseCommandLine_ThirdArgIsExcludeButMissingExcludeFile_ReturnsNull()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot", "Report", "--Exclude" };
         var opts = parser.ParseCommandLine( args );

         opts.Should().BeNull();
      }

      [TestMethod]
      public void ParseCommandLine_ThirdArgIsExcludeWithFourthArgExcludeFile_ReturnsExcludeFile()
      {
         var parser = new CommandLineParser();

         string excludeFile = @"C:\Path\To\ExcludeFile.txt";
         string[] args = { @"C:\RepoRoot", "Report", "--Exclude", excludeFile };
         var opts = parser.ParseCommandLine( args );

         opts.ExcludeFile.Should().Be( excludeFile );
      }

      [TestMethod]
      public void ParseCommandLine_FourthArgIsInvalid_ReturnsNull()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot", "Report", "--FailOnHCS", "Invalid" };
         var opts = parser.ParseCommandLine( args );

         opts.Should().BeNull();
      }

      [TestMethod]
      public void ParseCommandLine_FourthArgIsExcludeButMissingExcludeFile_ReturnsNull()
      {
         var parser = new CommandLineParser();

         string[] args = { @"C:\RepoRoot", "Report", "--FailOnHCS", "--Exclude" };
         var opts = parser.ParseCommandLine( args );

         opts.Should().BeNull();
      }

      [TestMethod]
      public void ParseCommandLine_FourthArgIsExcludeWithFifthArgExcludeFile_ReturnsExcludeFile()
      {
         var parser = new CommandLineParser();

         string excludeFile = @"C:\Path\To\ExcludeFile.txt";
         string[] args = { @"C:\RepoRoot", "Report", "--FailOnHCS", "--Exclude", excludeFile };
         var opts = parser.ParseCommandLine( args );

         opts.ExcludeFile.Should().Be( excludeFile );
      }
   }
}
